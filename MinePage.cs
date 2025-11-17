using AntdUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace XCWallPaper
{
    public partial class MinePage : UserControl
    {
        private List<string> imagePaths = new List<string>();
        private const int ThumbSize = 200; // 缩略图大小（正方形）
        const int PageSize = 20;
        private PictureBox tempImage;
        private string currentImagePath;
        private AntdUI.Panel currentImagePanel;

        public MinePage()
        {
            InitializeComponent();
            
            tempImage = new PictureBox
            {
                Size = new Size(ThumbSize, ThumbSize),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        public async Task LoadImagesFromFolder()
        {
            string folderPath = PathManager.Instance.WallpapersDirectory;

            if (!Directory.Exists(folderPath)) return;

            // 获取支持的图片格式
            var extensions = new[] { ".jpg", ".jpeg", ".png"};

            // 递归遍历所有子文件夹，筛选不以_depth结尾的图片
            imagePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => {
                    string ext = Path.GetExtension(f).ToLower();
                    string fileName = Path.GetFileNameWithoutExtension(f);
                    return extensions.Contains(ext) && !fileName.EndsWith("_depth");
                })
                .ToList();

            imagePaths.Reverse();

            if (imagePaths.Count > 0)
            {
                await UpdatePreviewBoxAsync(imagePaths[0]);
            }

            UpdatePagination();
            RenderImageThumbnails(1);
        }

        private void RenderImageThumbnails(int page)
        {
            ImageFlowPanel.Controls.Clear();

            // 计算当前页的起始和结束索引
            int startIndex = (page - 1) * PageSize;
            int endIndex = Math.Min(startIndex + PageSize, imagePaths.Count);

            // 显示当前页的图片
            for (int i = startIndex; i < endIndex; i++)
            {
                AddImage(imagePaths[i]);
            }

            UpdatePreviewBoxAsync(imagePaths[startIndex]);
        }

        public async void AIAddImage(string path)
        {
            if (imagePaths.Contains(path)) return;
            imagePaths.Insert(0, path);
            UpdatePagination();

            if (ImagePagination.Current == 1)
            {
                AddImage(path, true);
                if (ImageFlowPanel.Controls.Count >= PageSize)
                {
                    ImageFlowPanel.Controls.RemoveAt(PageSize - 1);
                }
            }
        }

        public async void AddImage(string path, bool head = false)
        {
            var boxPanel = new AntdUI.Panel
            {
                Size = new Size(ThumbSize, ThumbSize),
                Location = new Point(915, 0),
                Back = Color.Transparent,
                Shadow = 5,
                ShadowOpacity = 1f,
                ShadowOpacityAnimation = true,
                Radius = 5
            };

            UITool.Instance.ArcRegion(boxPanel, 8);

            var thumbBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                Tag = path,
                Cursor = Cursors.Hand
            };

            thumbBox.MouseEnter += (sender, e) =>
            {
                boxPanel.Shadow = 0;
            };
            thumbBox.MouseLeave += (sender, e) =>
            {
                boxPanel.Shadow = 5;
            };

            // 异步加载图片防止UI冻结
            if (thumbBox.IsHandleCreated)
            {
                // 句柄已创建，可以直接调用Invoke
                await Task.Run(() =>
                {
                    var image = UITool.Instance.LoadImageSquared(path, ThumbSize);
                    thumbBox.Invoke((MethodInvoker)(() => thumbBox.Image = image));
                });
            }
            else
            {
                // 句柄尚未创建，等待句柄创建后再执行
                thumbBox.HandleCreated += (sender, e) =>
                {
                    Task.Run(() =>
                    {
                        var image = UITool.Instance.LoadImageSquared(path, ThumbSize);
                        thumbBox.Invoke((MethodInvoker)(() => thumbBox.Image = image));
                    });
                };
            }

            thumbBox.DoubleClick += ThumbBox_DoubleClick;
            thumbBox.MouseDown += ThumbBox_MouseDown;
            boxPanel.Controls.Add(thumbBox);
            ImageFlowPanel.Controls.Add(boxPanel);
            if (head)
            {
                ImageFlowPanel.Controls.SetChildIndex(boxPanel, 0);
            }
            return;
        }

        private async void ThumbBox_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox box = sender as PictureBox;
            string path = (string)box.Tag;
            if (path != currentImagePath) 
            {
                currentImagePath = path;
                currentImagePanel = (AntdUI.Panel)box.Parent;
                await UpdatePreviewBoxAsync(path);
            }

            if (e.Button == MouseButtons.Right)
            {
                var thumbBox = (PictureBox)sender;
                thumbBox.ContextMenuStrip = ImageContextMenu;
            }
        }


        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentImagePath))
            {
                UpdateWallPaper(currentImagePath);
            }
        }
        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentImagePath)) return;

            AntdUI.Modal.open(new AntdUI.Modal.Config(this.ParentForm, "确认要删除吗？", "", AntdUI.TType.None)
            {
                OnButtonStyle = (id, btn) =>
                {
                    btn.DefaultBack = Color.FromArgb(240, 240, 240);
                },
                OnOk = (btn) =>
                {
                    try
                    {
                        if (File.Exists(currentImagePath))
                        {
                            string folderPath = Path.GetDirectoryName(currentImagePath);
                            Directory.Delete(folderPath, true);

                            imagePaths.Remove(currentImagePath);
                            UpdatePagination();

                            if (imagePaths.Count == 0) 
                            {
                                UpdatePreviewBoxAsync(string.Empty);
                            }
                            else
                            {
                                UpdatePreviewBoxAsync(imagePaths[0]);
                            }

                            this.Invoke(new Action(() =>
                            {
                                ImageFlowPanel.Controls.Remove(currentImagePanel);

                                int substitute = PageSize * ImagePagination.Current - 1;
                                if (imagePaths.Count >= substitute)
                                {
                                    AddImage(imagePaths[substitute]);
                                }
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除文件时出错: {ex.Message}", "错误",
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return true;
                },
                OkText = "确定",
                CancelText = "取消"
            });
        }

        private async void ThumbBox_DoubleClick(object? sender, EventArgs e)
        {
            var path = (string)((PictureBox)sender).Tag;
            await UpdateWallPaper(path);
        }

        private async Task UpdateWallPaper(string path)
        {
            try
            {
                // 检查文件是否存在
                if (!File.Exists(path))
                {
                    Console.WriteLine($"错误：图片文件不存在 - {path}");
                    return;
                }

                // 获取深度图路径（假设深度图在同一目录下，文件名格式为"原文件名_depth.扩展名"）
                string directory = Path.GetDirectoryName(path);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
                string fileExt = Path.GetExtension(path);
                string depthPath = Path.Combine(directory, $"{fileNameWithoutExt}_depth{fileExt}");

                // 异步加载图片
                using (var image = await Task.Run(() => new Bitmap(path)))
                using (var ms = new MemoryStream())
                {
                    // 将图片转换为字节数组
                    image.Save(ms, ImageFormat.Png); // 可以根据需要修改为其他格式
                    byte[] imageData = ms.ToArray();

                    byte[] depthData = null;
                    // 检查深度图是否存在，存在则加载
                    if (File.Exists(depthPath))
                    {
                        using (var depthImage = await Task.Run(() => new Bitmap(depthPath)))
                        using (var depthMs = new MemoryStream())
                        {
                            depthImage.Save(depthMs, ImageFormat.Png);
                            depthData = depthMs.ToArray();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"警告：深度图文件不存在 - {depthPath}");
                    }

                    // 发送消息
                    await IPCSender.Instance.SendMessageAsync(new WallPaperUpdateMessage
                    {
                        ImageData = imageData,
                        DepthData = depthData
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新壁纸时发生错误: {ex.Message}");
            }
        }

        // 打开预览
        private async Task OpenImageAsync(string path)
        {
            try
            {
                // 使用异步方式启动外部程序（需要添加适当的错误处理）
                await Task.Run(() =>
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    };

                    using (var process = Process.Start(processStartInfo))
                    {
                        // 等待程序启动（非阻塞）
                        if (process != null && !process.HasExited)
                        {
                            process.WaitForInputIdle();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开图片时出错: {ex.Message}");
            }
        }

        private async Task UpdatePreviewBoxAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                PreviewBox.Image = null;
                return;
            }
            try
            {
                // 异步加载图片（在后台线程执行）
                using (var bitmap = await Task.Run(() => new Bitmap(path)))
                {
                    PreviewBox.Image = new Bitmap(bitmap);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图片时出错: {ex.Message}");
            }
        }

        private async void PreviewBox_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                AntdUI.Image3D pictureBox = (AntdUI.Image3D)sender;
                System.Drawing.Image image = pictureBox.Image;
                if (image != null) 
                {
                    AntdUI.Preview.open(new AntdUI.Preview.Config(this.ParentForm, image));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开图片时出错: {ex.Message}");
            }
        }

        private void ImagePagination_ValueChanged(object sender, AntdUI.PagePageEventArgs e)
        {
            AntdUI.Pagination pagination = (AntdUI.Pagination)sender;
            RenderImageThumbnails(pagination.Current);
        }

        private void UpdatePagination()
        {
            ImagePagination.Total = (int)MathF.Ceiling(imagePaths.Count / (float)PageSize);
        }
    }
}