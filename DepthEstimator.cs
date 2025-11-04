using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;


// ### 本地深度估计AI ###
namespace XCWallPaper
{
    class DepthEstimator : IDisposable
    {
        private InferenceSession session;
        private readonly int inputSize;
        private readonly float meanR, meanG, meanB;
        private readonly float stdR, stdG, stdB;
        private bool disposed = false;
        private bool isUsingGPU = false;


        /// 获取当前是否使用GPU加速

        public bool IsUsingGPU => isUsingGPU;


        public DepthEstimator(string onnxModelPath, int inputSize = 518,
            float meanR = 0.485f, float meanG = 0.456f, float meanB = 0.406f,
            float stdR = 0.229f, float stdG = 0.224f, float stdB = 0.225f)
        {
            this.inputSize = inputSize;
            this.meanR = meanR;
            this.meanG = meanG;
            this.meanB = meanB;
            this.stdR = stdR;
            this.stdG = stdG;
            this.stdB = stdB;

            // 加载ONNX模型并配置执行提供者
            var sessionOptions = new SessionOptions();

            try
            {
                // 尝试使用DirectML GPU加速
                sessionOptions.AppendExecutionProvider_DML(0); // 指定设备ID为0
                sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_DISABLE_ALL; // 完全禁用图优化

                // 添加DirectML兼容性配置选项
                sessionOptions.AddSessionConfigEntry("ep.dml.enable_dynamic_graph_fusion", "0"); // 禁用动态图融合
                sessionOptions.AddSessionConfigEntry("ep.dml.disable_metacommands", "0"); // 禁用元命令
                sessionOptions.AddSessionConfigEntry("ep.dml.enable_graph_capture", "0"); // 禁用图捕获
                sessionOptions.AddSessionConfigEntry("ep.dml.enable_graph_serialization", "0"); // 禁用图序列化

                Console.WriteLine("尝试使用DirectML GPU加速（兼容模式）...");

                session = new InferenceSession(onnxModelPath, sessionOptions);
                isUsingGPU = true;
                Console.WriteLine("DirectML GPU加速初始化成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DirectML初始化失败: {ex.Message}");
                Console.WriteLine("回退到CPU执行...");

                // 回退到CPU执行
                sessionOptions.Dispose();
                sessionOptions = new SessionOptions();
                sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

                try
                {
                    session = new InferenceSession(onnxModelPath, sessionOptions);
                    isUsingGPU = false;
                    Console.WriteLine("CPU执行初始化成功！");
                }
                catch (Exception cpuEx)
                {
                    throw new Exception($"CPU和GPU初始化都失败了。GPU错误: {ex.Message}, CPU错误: {cpuEx.Message}");
                }
            }

            // 输出模型输入信息
            var inputMeta = session.InputMetadata;
            Console.WriteLine("模型输入信息:");
            foreach (var item in inputMeta)
            {
                Console.WriteLine($"  {item.Key}: {string.Join(", ", item.Value.Dimensions)}");
            }

            // 输出模型输出信息
            var outputMeta = session.OutputMetadata;
            Console.WriteLine("模型输出信息:");
            foreach (var item in outputMeta)
            {
                Console.WriteLine($"  {item.Key}: {string.Join(", ", item.Value.Dimensions)}");
            }
        }

        /// <summary>
        /// 批量处理图片并实时回调进度（无顺序保证，处理完成立即回调）
        /// </summary>
        public async Task EstimateDepthAsync(List<Bitmap> inputImages,
            Action<int, Bitmap, float> resultCallback,
            int maxParallelism = -1,
            CancellationToken cancellationToken = default)
        {
            if (inputImages == null || inputImages.Count == 0)
                throw new ArgumentException("输入图片数组不能为空", nameof(inputImages));

            if (resultCallback == null)
                throw new ArgumentException("结果回调函数不能为空", nameof(resultCallback));

            int totalTasks = inputImages.Count;
            int completedTasks = 0;

            // 确定最佳并行度（默认使用CPU核心数，不超过任务数）
            if (maxParallelism == -1)
                maxParallelism = Math.Min(Environment.ProcessorCount, totalTasks);
            else
                maxParallelism = Math.Min(maxParallelism, totalTasks);

            // 使用SemaphoreSlim控制并发任务数
            var semaphore = new SemaphoreSlim(maxParallelism);
            var tasks = new List<Task>();

            for (int i = 0; i < totalTasks; i++)
            {
                int id = i;
                Bitmap image = inputImages[i];

                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync(cancellationToken);

                    try
                    {
                        // 图像处理流程
                        Tensor<float> inputTensor = PreprocessImage(image);
                        string inputName = session.InputMetadata.Keys.First();
                        var inputs = new List<NamedOnnxValue>
                        {
                            NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
                        };

                        using var results = session.Run(inputs);
                        var outputTensor = results.First().AsTensor<float>();

                        Bitmap depthMap = PostprocessOutput(outputTensor, image.Width, image.Height);

                        // 计算实时进度（原子操作保证线程安全）
                        int localCompleted = Interlocked.Increment(ref completedTasks);
                        float progress = (float)localCompleted / totalTasks;

                        // 直接调用回调函数（实时触发）
                        resultCallback(id, depthMap, progress);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"处理图片时发生错误: {ex.Message}");
                    }
                    finally
                    {
                        // 释放信号量，允许下一个任务开始
                        semaphore.Release();
                    }
                }, cancellationToken));
            }

            // 等待所有任务完成
            await Task.WhenAll(tasks);
        }


        /// <summary>
        /// 图像预处理：调整大小、归一化
        /// </summary>
        private Tensor<float> PreprocessImage(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            int inputHeight = inputSize;
            int inputWidth = (int)(inputSize * (float)width / height);
            inputHeight = (int)Math.Ceiling((double)inputHeight / 14) * 14;
            inputWidth = (int)Math.Ceiling((double)inputWidth / 14) * 14;

            // 调整图像大小到模型输入尺寸
            using Bitmap resizedImage = new Bitmap(inputWidth, inputHeight);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, inputWidth, inputHeight);
            }

            // 将图像转换为float数组 [B, C, H, W]
            float[] inputArray = new float[1 * 3 * inputHeight * inputWidth];
            int index = 0;

            for (int y = 0; y < inputHeight; y++)
            {
                for (int x = 0; x < inputWidth; x++)
                {
                    Color pixel = resizedImage.GetPixel(x, y);

                    // 直接使用RGB顺序（无需BGR转换）
                    float r = (pixel.R / 255.0f - meanR) / stdR;
                    float g = (pixel.G / 255.0f - meanG) / stdG;
                    float b = (pixel.B / 255.0f - meanB) / stdB;

                    // 按RGB顺序填充
                    inputArray[index + 0 * inputHeight * inputWidth] = r; // B
                    inputArray[index + 1 * inputHeight * inputWidth] = g; // G
                    inputArray[index + 2 * inputHeight * inputWidth] = b; // R

                    index++;
                }
            }

            // 创建ONNX Runtime张量
            var tensor = new DenseTensor<float>(inputArray, new int[] { 1, 3, inputHeight, inputWidth });
            return tensor;
        }

        /// <summary>
        /// 模型输出后处理：将深度值转换为可视图像
        /// </summary>
        private Bitmap PostprocessOutput(Tensor<float> outputTensor, int originalWidth, int originalHeight)
        {
            // 获取输出张量的实际维度
            int dimensionCount = outputTensor.Dimensions.Length;
            Console.WriteLine($"输出张量维度数量: {dimensionCount}");

            // 确定输出的高度和宽度
            int outputHeight, outputWidth;

            // 处理不同维度的输出张量
            if (dimensionCount == 4)
            {
                // 格式为 [batch, channel, height, width]
                int batchSize = outputTensor.Dimensions[0];
                int channelCount = outputTensor.Dimensions[1];
                outputHeight = outputTensor.Dimensions[2];
                outputWidth = outputTensor.Dimensions[3];

                // 确保batchSize符合预期
                if (batchSize != 1 || channelCount != 1)
                {
                    throw new Exception($"意外的输出张量格式: 预期[1,1,H,W]，实际[{batchSize},{channelCount},{outputHeight},{outputWidth}]");
                }
            }
            else if (dimensionCount == 3)
            {
                // 格式为 [channel, height, width]
                int channelCount = outputTensor.Dimensions[0];
                outputHeight = outputTensor.Dimensions[1];
                outputWidth = outputTensor.Dimensions[2];

                // 确保channelCount符合预期
                if (channelCount != 1)
                {
                    throw new Exception($"意外的输出张量格式: 预期[1,H,W]，实际[{channelCount},{outputHeight},{outputWidth}]");
                }
            }
            else if (dimensionCount == 2)
            {
                // 格式为 [height, width]
                outputHeight = outputTensor.Dimensions[0];
                outputWidth = outputTensor.Dimensions[1];
            }
            else
            {
                throw new Exception($"不支持的输出张量维度数量: {dimensionCount}");
            }

            // 找到深度值的范围用于可视化
            float minDepth = float.MaxValue;
            float maxDepth = float.MinValue;

            // 根据不同维度访问张量元素
            for (int y = 0; y < outputHeight; y++)
            {
                for (int x = 0; x < outputWidth; x++)
                {
                    float depth;

                    if (dimensionCount == 4)
                        depth = outputTensor[0, 0, y, x];
                    else if (dimensionCount == 3)
                        depth = outputTensor[0, y, x];
                    else // dimensionCount == 2
                        depth = outputTensor[y, x];

                    minDepth = Math.Min(minDepth, depth);
                    maxDepth = Math.Max(maxDepth, depth);
                }
            }

            // 防止除零错误
            if (maxDepth == minDepth) maxDepth = minDepth + 1e-8f;

            // 创建深度图
            Bitmap depthMap = new Bitmap(outputWidth, outputHeight, PixelFormat.Format24bppRgb);
            // 使用多维索引直接访问张量元素
            for (int y = 0; y < outputHeight; y++)
            {
                for (int x = 0; x < outputWidth; x++)
                {
                    // 根据不同维度访问张量元素
                    float depth;

                    if (dimensionCount == 4)
                        depth = outputTensor[0, 0, y, x];
                    else if (dimensionCount == 3)
                        depth = outputTensor[0, y, x];
                    else // dimensionCount == 2
                        depth = outputTensor[y, x];

                    int grayValue = (int)Math.Clamp(255 * (depth - minDepth) / (maxDepth - minDepth), 0, 255);
                    Color color = Color.FromArgb(grayValue, grayValue, grayValue);
                    depthMap.SetPixel(x, y, color);
                }
            }

            // 调整回原始尺寸
            if (outputWidth != originalWidth || outputHeight != originalHeight)
            {
                Bitmap resizedDepthMap = new Bitmap(originalWidth, originalHeight);
                using (Graphics g = Graphics.FromImage(resizedDepthMap))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(depthMap, 0, 0, originalWidth, originalHeight);
                }
                depthMap.Dispose();
                depthMap = resizedDepthMap;
            }

            return depthMap;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    session?.Dispose();
                }

                disposed = true;
            }
        }

        ~DepthEstimator()
        {
            Dispose(false);
        }
    }
}