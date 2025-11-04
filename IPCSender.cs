using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XCWallPaper
{
    internal class IPCSender
    {
        private const string PIPE_NAME = "PaperCacheIPC";
        private const int MAX_QUEUE_SIZE = 8; // 最大队列长度

        private static readonly Lazy<IPCSender> _instance = new Lazy<IPCSender>(() => new IPCSender());
        public static IPCSender Instance => _instance.Value;

        private readonly ConcurrentQueue<IPCMessage> _messageQueue = new ConcurrentQueue<IPCMessage>();
        private readonly AutoResetEvent _messageAvailable = new AutoResetEvent(false);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private IPCSender()
        {
            // 启动后台消息处理线程
            Task.Run(() => ProcessMessageQueueAsync(_cts.Token));
        }

        // 同步发送IPC消息
        public bool SendMessage(IPCMessage message)
        {
            // 更新时间戳
            message.Timestamp = DateTime.Now.Ticks;

            try
            {
                // 创建管道客户端
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.Out))
                {
                    // 连接到服务器（最多等待1秒）
                    pipeClient.Connect(1000);

                    using (BinaryWriter writer = new BinaryWriter(pipeClient, Encoding.UTF8, true))
                    {
                        // 写入消息类型
                        writer.Write(message.MessageType);

                        // 写入时间戳
                        writer.Write(message.Timestamp);

                        // JSON序列化
                        string json = JsonSerializer.Serialize(message, message.GetType());
                        writer.Write(json);
                        Console.WriteLine("发送: " + json);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message: {ex.Message}");
                return false;
            }
        }

        // 异步发送改为入队操作
        public Task<bool> SendMessageAsync(IPCMessage message)
        {
            try
            {
                // 更新时间戳
                message.Timestamp = DateTime.Now.Ticks;

                // 检查队列长度，超过最大长度时移除最旧消息
                while (_messageQueue.Count >= MAX_QUEUE_SIZE)
                {
                    if (_messageQueue.TryDequeue(out _))
                    {
                        Console.WriteLine($"队列已满({_messageQueue.Count}/{MAX_QUEUE_SIZE})，移除最旧消息");
                    }
                }

                // 将新消息加入队列
                _messageQueue.Enqueue(message);
                Console.WriteLine($"消息已入队，当前队列长度: {_messageQueue.Count}");

                // 通知处理线程有新消息
                _messageAvailable.Set();

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enqueue message: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        // 后台消息处理线程
        private async Task ProcessMessageQueueAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    // 等待消息可用
                    _messageAvailable.WaitOne(1000);

                    // 处理队列中的所有消息
                    while (_messageQueue.TryDequeue(out var message))
                    {
                        try
                        {
                            // 使用管道发送
                            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.Out))
                            {
                                // 异步连接（带超时）
                                await pipeClient.ConnectAsync(1000, ct).ConfigureAwait(false);

                                // 使用BinaryWriter进行二进制数据传输
                                using (BinaryWriter writer = new BinaryWriter(pipeClient, Encoding.UTF8, true))
                                {
                                    // 写入消息类型
                                    writer.Write(message.MessageType);

                                    // 写入时间戳
                                    writer.Write(message.Timestamp);

                                    // ### WallPaperUpdateMessage ###
                                    switch(message)
                                    {
                                        case WallPaperUpdateMessage wallPaperUpdateMessage:
                                            await SendWallPaperUpdateMessage(pipeClient, writer, wallPaperUpdateMessage);
                                            break;
                                        case AddEffectMessage addEffectMessage:
                                            await SendAddEffectMessage(pipeClient, writer, addEffectMessage);
                                            break;
                                        case UpdateEffectParameterMessage updateEffectParameterMessage:
                                            SendUpdateEffectParameter(pipeClient, writer, updateEffectParameterMessage);
                                            break;
                                        default:
                                            await SendJsonMessage(writer, message);
                                            break;
                                    };
                                    // 确保所有数据都被发送
                                    await pipeClient.FlushAsync().ConfigureAwait(false);
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // 任务取消正常退出
                            return;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Message processing failed: {ex.Message}");
                        }
                    }
                }
            }
            finally
            {
                // 清理资源
                _messageAvailable.Dispose();
            }
        }

        // 可选：添加资源清理方法
        public void Dispose()
        {
            _cts.Cancel();
            _messageAvailable.Set(); // 唤醒处理线程
        }

        private async Task SendJsonMessage(BinaryWriter writer, IPCMessage message)
        {
            // 对于其他类型的消息，使用JSON序列化
            string json = JsonSerializer.Serialize(message, message.GetType());
            Console.WriteLine("发送: " + json);
            writer.Write(json);
        }

        private async Task SendWallPaperUpdateMessage(NamedPipeClientStream pipeClient, BinaryWriter writer, WallPaperUpdateMessage message)
        {
            // 写入图片数据（校验码 + 图片数据）
            byte[] image_checksum = IPCEncrypt.CalculateChecksum(message.ImageData);
            byte[] imageWithChecksum = new byte[image_checksum.Length + message.ImageData.Length];

            // 复制校验码到前面
            Buffer.BlockCopy(image_checksum, 0, imageWithChecksum, 0, image_checksum.Length);
            // 复制图片数据到后面
            Buffer.BlockCopy(message.ImageData, 0, imageWithChecksum, image_checksum.Length, message.ImageData.Length);

            // 写入组合数据的总长度和内容
            writer.Write(imageWithChecksum.Length);
            await pipeClient.WriteAsync(imageWithChecksum, 0, imageWithChecksum.Length).ConfigureAwait(false);

            // 写入深度图数据（校验码 + 深度图数据）
            byte[] depth_checksum = IPCEncrypt.CalculateChecksum(message.DepthData);
            byte[] depthWithChecksum = new byte[depth_checksum.Length + message.DepthData.Length];

            // 复制校验码到前面
            Buffer.BlockCopy(depth_checksum, 0, depthWithChecksum, 0, depth_checksum.Length);
            // 复制深度图数据到后面
            Buffer.BlockCopy(message.DepthData, 0, depthWithChecksum, depth_checksum.Length, message.DepthData.Length);

            // 写入组合数据的总长度和内容
            writer.Write(depthWithChecksum.Length);
            await pipeClient.WriteAsync(depthWithChecksum, 0, depthWithChecksum.Length).ConfigureAwait(false);

            Console.WriteLine($"发送壁纸更新消息: 图片大小 {message.ImageData.Length} 字节, 深度图大小 {message.DepthData.Length} 字节");
        }

        private async Task SendAddEffectMessage(NamedPipeClientStream pipeClient, BinaryWriter writer, AddEffectMessage message)
        {
            // 写入ID
            Console.WriteLine(message.ID);
            writer.Write(message.ID);

            // 写入HLSL代码数组

            writer.Write(message.HLSLCode.Length);
            Console.WriteLine(message.HLSLCode.Length);
            foreach (var code in message.HLSLCode)
            {
                Console.WriteLine(code);
                writer.Write(code);
            }

            // 写入参数结构数组
            writer.Write(message.Parameter.Length);
            foreach (var param in message.Parameter)
            {
                string json = JsonSerializer.Serialize(param);
                writer.Write(json);
                Console.WriteLine(json);
            }

            // 写入ImageData
            // 格式：三维数组 byte[effectLayer][frameIndex][data]
            if (message.ImageData == null)
            {
                writer.Write(0); // 层数为0表示没有图像数据
                return;
            }

            // 写入层数
            writer.Write(message.ImageData.Length);
            
            for (int layer = 0; layer < message.ImageData.Length; layer++)
            {
                var frames = message.ImageData[layer];
                if (frames == null)
                {
                    writer.Write(0); // 帧数为0表示该层没有数据
                    continue;
                }

                // 写入当前层的帧数
                writer.Write(frames.Length);

                // 写入每帧数据
                for (int frame = 0; frame < frames.Length; frame++)
                {
                    var imageData = frames[frame];
                    if (imageData == null || imageData.Length == 0)
                    {
                        writer.Write(0); // 数据长度为0表示空帧
                        continue;
                    }

                    // 写入图像数据长度和内容（不加密，直接传输）
                    writer.Write(imageData.Length);
                    await pipeClient.WriteAsync(imageData, 0, imageData.Length).ConfigureAwait(false);
                }
            }

            Console.WriteLine($"发送特效添加消息: ID={message.ID}, 层数={message.ImageData?.Length ?? 0}");
        }

        private void SendUpdateEffectParameter(NamedPipeClientStream pipeClient, BinaryWriter writer, UpdateEffectParameterMessage message)
        {
            // ID
            Console.WriteLine(message.ID);
            writer.Write(message.ID);

            // Parameter
            string json = JsonSerializer.Serialize(message.Parameter);
            writer.Write(json);
        }
    }
}