using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace XCWallPaper
{
    public partial class AiPage : UserControl
    {
        // 用于跟踪正在处理的临时文件夹
        private HashSet<string> processingFolders = new HashSet<string>();

        // 取消令牌源，用于取消正在进行的处理
        private CancellationTokenSource cancellationTokenSource;

        // 更新Mine Page
        public Action<string> UpdateMinePage;

        private bool isProcessing = false;

        public AiPage()
        {
            DoubleBuffered = true;
            InitializeComponent();
        }

        public void Closed()
        {
            // 取消正在进行的处理
            CancelOngoingProcessing();

            // 清理临时文件夹
            CleanupTemporaryFolders();
        }

        // ### 图片未处理完，软件退出 ###
        private void CancelOngoingProcessing()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        private void CleanupTemporaryFolders()
        {
            if (processingFolders.Count == 0)
                return;

            try
            {
                foreach (string folder in processingFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        PathManager.Instance.SafeDeleteFolder(folder);
                    }
                }
                processingFolders.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理临时文件夹时出错: {ex.Message}");
            }
        }


        // ### 图片导入区域的事件处理 ###
        private void DropPanel_MouseLeave(object sender, EventArgs e)
        {
            UITool.Instance.MouseLeaveColor(DropPanel);
        }

        private void DropPanel_MouseEnter(object sender, EventArgs e)
        {
            UITool.Instance.MouseEnterColor(DropPanel, Color.FromArgb(220, 220, 220));
        }
        private void DropPanel_DragLeave(object sender, EventArgs e)
        {
            UITool.Instance.MouseLeaveColor(DropPanel);
        }

        private void DropPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string ext = System.IO.Path.GetExtension(files[0]).ToLower();

                // 支持的图片格式
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp")
                {
                    e.Effect = DragDropEffects.Copy;
                    UITool.Instance.MouseEnterColor(DropPanel, Color.FromArgb(220, 220, 220));
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void DropPanel_DragDrop(object sender, DragEventArgs e)
        {
            DropPanel.BackColor = Color.FromArgb(240, 240, 240);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // 过滤出支持的图片文件
                var imageFiles = files.Where(file =>
                {
                    string ext = Path.GetExtension(file).ToLower();
                    return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp";
                }).ToArray();

                if (imageFiles.Length > 0)
                {
                    ProcessImages(imageFiles);
                }
            }
        }

        private void DropPanel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Multiselect = true;  // 启用多选
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ProcessImages(openFileDialog.FileNames);
                }
            }
        }

        // ### 核心图片处理逻辑 ###
        
        private async Task ProcessImages(string[] imagePaths)
        {
            if (imagePaths == null || imagePaths.Length == 0)
            {
                AntdUI.Message.info(this.ParentForm, "没有选择要处理的图像");
                return;
            }

            int successCount = 0;
            isProcessing = true;
            AntdUI.Message.loading(this.ParentForm, "处理中...", async (config) =>
            {
                int lastSuccessCount = 0;
                while (isProcessing) 
                {
                    Thread.Sleep(100);
                    if (successCount != lastSuccessCount)
                    {
                        config.Text = AntdUI.Localization.Get("Loading", "处理中...") + $" （{successCount}/{imagePaths.Length}）";
                        config.Refresh();
                        lastSuccessCount = successCount;
                    }
                    continue; 
                }
                config.OK("转换成功，已经添加至壁纸库！");
            }, new System.Drawing.Font("Segoe UI", 12F));

            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            string modelPath = "depth_anything_v2_vitb_dynamic.onnx";
            int imageCount = imagePaths.Length;

            if (imageCount == 0) { return; }

            try
            {
                // 清空之前的处理文件夹记录
                processingFolders.Clear();

                using (DepthEstimator estimator = new DepthEstimator(modelPath, (int)(518)))
                {
                    // 准备图片数组和输出路径
                    var depthPaths = new List<string>();
                    var images = new List<Bitmap>();
                    ConcurrentBag<string> resultPaths = new ConcurrentBag<string>();
                    foreach (string imagePath in imagePaths)
                    {
                        // 检查是否已取消
                        if (token.IsCancellationRequested) break;

                        // 生成唯一文件夹名
                        string tempID = PathManager.Instance.GenerateUniqueFolderName();
                        string outputFolder = PathManager.Instance.CreateWallpaperFolder(tempID);

                        // 记录正在处理的文件夹
                        processingFolders.Add(outputFolder);

                        string fileName = Path.GetFileName(imagePath);
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);

                        string targetImagePath = Path.Combine(outputFolder, fileName);
                        string depthMapPath = Path.Combine(outputFolder, $"{fileNameWithoutExt}_depth{extension}");
                        depthPaths.Add(depthMapPath);

                        // 复制图片到输出目录（同步操作，确保后续处理可用）
                        File.Copy(imagePath, targetImagePath, true);
                        resultPaths.Add(targetImagePath);

                        // 加载图片并保存任务参数
                        using (Bitmap inputImage = new Bitmap(targetImagePath))
                        {
                            images.Add(
                                new Bitmap(inputImage) // 复制Bitmap避免资源冲突
                            );
                        }
                    }

                    // 调用批量处理方法
                    await estimator.EstimateDepthAsync(
                        images,
                        (id, depthMap, progress) =>
                        {
                            // 检查是否已取消
                            if (token.IsCancellationRequested) return;

                            // 保存深度图
                            depthMap.Save(depthPaths[id]);

                            // 从处理列表中移除已完成的文件夹
                            string folder = Path.GetDirectoryName(depthPaths[id]);
                            if (processingFolders.Contains(folder))
                            {
                                processingFolders.Remove(folder);
                            }

                            // 更新进度（线程安全的UI更新）
                            Interlocked.Increment(ref successCount);
                            
                        },
                        2,      // 并行线程数量
                        token   // 取消令牌
                    );

                    // 处理完成后通知
                    if (successCount > 0)
                    {
                        CompleteProcessing(resultPaths);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 处理被用户取消，正常情况
                MessageBox.Show("图片处理已取消", "操作取消", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化处理引擎时出错: {ex.Message}", "严重错误",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 清理可能残留的处理标记
                processingFolders.Clear();
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                }
            }
        }


        //### 处理进度管理 ###

        private void CompleteProcessing(ConcurrentBag<string> resultPaths)
        {
            isProcessing = false;

            // 处理完成后清理处理记录
            processingFolders.Clear();

            // Update Mine Page
            if (UpdateMinePage != null)
            {
                foreach (var path in resultPaths) 
                {
                    UpdateMinePage(path);
                }
            }
        }
    }
}