using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XCWallPaper
{
    class WallpaperCapture
    {
        private static readonly Lazy<WallpaperCapture> _instance = new Lazy<WallpaperCapture>(() => new WallpaperCapture());
        public static WallpaperCapture Instance => _instance.Value;

        // 导入Windows API函数
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // 导入屏幕捕获相关的Windows API
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        // 定义委托
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // Windows API常量
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SRCCOPY = 0x00CC0020; // 源复制模式

        // 存储窗口句柄以便恢复
        private List<IntPtr> visibleWindows = new List<IntPtr>();

        /// <summary>
        /// 设置桌面图标可见性
        /// </summary>
        /// <param name="isVisible">是否可见</param>
        public void SetDesktopIconsVisible(bool isVisible)
        {
            try
            {
                // 找到桌面窗口
                IntPtr progman = FindWindow("Progman", null);
                IntPtr defView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
                IntPtr sysListView = FindWindowEx(defView, IntPtr.Zero, "SysListView32", null);

                if (sysListView != IntPtr.Zero)
                {
                    // 根据参数显示或隐藏桌面图标
                    ShowWindow(sysListView, isVisible ? SW_SHOW : SW_HIDE);
                }
                else
                {
                    throw new Exception("无法找到桌面图标窗口");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置任务栏可见性
        /// </summary>
        /// <param name="isVisible">是否可见</param>
        public void SetTaskbarVisible(bool isVisible)
        {
            try
            {
                // 找到任务栏窗口
                IntPtr taskbar = FindWindow("Shell_TrayWnd", null);

                if (taskbar != IntPtr.Zero)
                {
                    // 根据参数显示或隐藏任务栏
                    ShowWindow(taskbar, isVisible ? SW_SHOW : SW_HIDE);
                }
                else
                {
                    throw new Exception("无法找到任务栏窗口");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置所有窗口可见性
        /// </summary>
        /// <param name="isVisible">是否可见</param>
        public void SetAllWindowsVisible(bool isVisible)
        {
            try
            {
                if (isVisible)
                {
                    // 显示所有之前隐藏的窗口
                    foreach (IntPtr hWnd in visibleWindows)
                    {
                        ShowWindow(hWnd, SW_SHOW);
                    }
                    visibleWindows.Clear();
                }
                else
                {
                    // 清空之前存储的窗口句柄
                    visibleWindows.Clear();

                    // 枚举所有顶级窗口
                    EnumWindows((hWnd, lParam) =>
                    {
                        // 排除桌面和任务栏窗口
                        IntPtr desktop = FindWindow("Progman", null);
                        IntPtr taskbar = FindWindow("Shell_TrayWnd", null);

                        if (hWnd != desktop && hWnd != taskbar && IsWindowVisible(hWnd))
                        {
                            visibleWindows.Add(hWnd);
                            ShowWindow(hWnd, SW_HIDE);
                        }
                        return true; // 继续枚举
                    }, IntPtr.Zero);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 捕获屏幕并保存到指定路径
        /// </summary>
        /// <param name="path">保存路径，为空时默认桌面</param>
        /// <returns>成功保存返回true，失败返回false</returns>
        public Image CaptureTo(string path = null)
        {
            try
            {
                // 调用捕获方法
                Image screenshot = Capture();
                    if (screenshot == null)
                {
                    Console.WriteLine("屏幕捕获失败，返回的图像为空。");
                    return screenshot;
                }

                // 处理保存路径
                string savePath = GetSavePath(path);

                // 创建目录（如果不存在）
                string directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 保存图像
                screenshot.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
                //Console.WriteLine($"屏幕捕获成功！图像已保存到: {savePath}");
                return screenshot;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 生成保存路径
        /// </summary>
        private string GetSavePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                // 默认保存到桌面
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"ScreenCapture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                return Path.Combine(desktopPath, fileName);
            }
            else if (Path.HasExtension(path))
            {
                // 如果路径包含扩展名，直接使用
                return path;
            }
            else
            {
                // 如果路径不包含扩展名，添加默认扩展名
                string fileName = $"ScreenCapture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                return Path.Combine(path, fileName);
            }
        }

        /// <summary>
        /// 捕获屏幕，先隐藏所有元素，捕获后恢复
        /// </summary>
        /// <returns>捕获的屏幕图像</returns>
        public Image Capture()
        {
            Image screenshot = null;

            try
            {
                // 隐藏桌面图标、任务栏和其他窗口
                SetDesktopIconsVisible(false);
                SetTaskbarVisible(false);
                SetAllWindowsVisible(false);

                // 确保UI有时间更新
                System.Threading.Thread.Sleep(100);

                // 捕获屏幕
                screenshot = CaptureScreen();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"捕获屏幕时出错: {ex.Message}");
            }
            finally
            {
                // 恢复所有隐藏的元素，无论捕获是否成功
                SetAllWindowsVisible(true);
                SetTaskbarVisible(true);
                SetDesktopIconsVisible(true);
            }

            return screenshot;
        }

        /// <summary>
        /// 实际执行屏幕捕获的方法
        /// </summary>
        /// <returns>捕获的屏幕图像</returns>
        private Image CaptureScreen()
        {
            // 获取屏幕尺寸
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            // 创建屏幕图像
            Bitmap bitmap = new Bitmap(screenWidth, screenHeight);

            // 获取屏幕DC
            IntPtr screenDC = GetDC(IntPtr.Zero);
            IntPtr memoryDC = CreateCompatibleDC(screenDC);
            IntPtr bitmapHandle = IntPtr.Zero;

            try
            {
                // 创建兼容位图
                bitmapHandle = CreateCompatibleBitmap(screenDC, screenWidth, screenHeight);
                IntPtr oldBitmap = SelectObject(memoryDC, bitmapHandle);

                // 复制屏幕内容到位图
                BitBlt(memoryDC, 0, 0, screenWidth, screenHeight, screenDC, 0, 0, SRCCOPY);

                // 从位图句柄创建Image对象
                using (System.Drawing.Image screenImage = Image.FromHbitmap(bitmapHandle))
                {
                    // 复制图像到结果Bitmap
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawImage(screenImage, 0, 0);
                    }
                }

                // 清理
                SelectObject(memoryDC, oldBitmap);
            }
            finally
            {
                // 释放资源
                if (bitmapHandle != IntPtr.Zero)
                {
                    DeleteObject(bitmapHandle);
                }
                if (memoryDC != IntPtr.Zero)
                {
                    DeleteObject(memoryDC);
                }
                if (screenDC != IntPtr.Zero)
                {
                    ReleaseDC(IntPtr.Zero, screenDC);
                }
            }

            return bitmap;
        }
    }
}