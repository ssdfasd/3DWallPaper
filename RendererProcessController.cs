using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace XCWallPaper
{
    internal class RendererProcessController
    {
        #region 单例与配置参数
        // 单例实例
        private static readonly Lazy<RendererProcessController> _instance =
            new Lazy<RendererProcessController>(() => new RendererProcessController());
        public static RendererProcessController Instance => _instance.Value;

        // 配置参数
        private string wallpaperProcessName = "Renderer.exe";
        private int checkInterval = 1000;
        private float overlapThreshold = 0.8f;

        // 状态变量
        private DateTime lastCheckTime = DateTime.MinValue;
        private bool isWallpaperPaused = false;
        private System.Threading.Timer checkTimer;

        // 自动暂停功能开关
        public bool EnableAutoPause { get; set; } = true;
        #endregion


        #region 初始化与进程控制
        /// <summary>
        /// 初始化检查定时器
        /// </summary>
        public void InitializeTimer()
        {
            checkTimer = new System.Threading.Timer(CheckWindowOverlap, null, 0, checkInterval);
        }

        /// <summary>
        /// 启动壁纸进程
        /// </summary>
        public bool StartProcess()
        {
            try
            {
                // 获取当前EXE的完整路径（包含文件名）
                string exeFullPath = Assembly.GetEntryAssembly().Location;

                // 输出结果
                Console.WriteLine("当前EXE完整路径：" + exeFullPath);

                // 如果需要获取EXE所在的目录（不含文件名），可以进一步处理：
                string exeDirectory = Path.GetDirectoryName(exeFullPath);
                Console.WriteLine("当前EXE所在目录：" + exeDirectory);

                string exePath = Path.Combine(exeDirectory, wallpaperProcessName);

                if (!File.Exists(exePath))
                    return false;

                string processName = Path.GetFileNameWithoutExtension(exePath);
                if (!string.IsNullOrEmpty(processName))
                    wallpaperProcessName = processName;

                var existingProcesses = Process.GetProcessesByName(wallpaperProcessName);
                if (existingProcesses.Length > 0)
                    return true;

                Process.Start(new ProcessStartInfo(exePath)
                {
                    UseShellExecute = false,
                    CreateNoWindow = false
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 退出壁纸进程
        /// </summary>
        public void ExitProcess()
        {
            try
            {
                var processes = Process.GetProcessesByName(wallpaperProcessName);
                foreach (var process in processes)
                {
                    if (process.HasExited) continue;

                    if (!process.CloseMainWindow())
                        process.Kill();

                    process.WaitForExit(2000);
                }
            }
            catch (Exception)
            {
                // 忽略异常
            }
        }
        #endregion


        #region 窗口覆盖检查
        // 需要排除的系统窗口类名
        private readonly HashSet<string> _excludedClasses = new HashSet<string>
        {
            "Shell_TrayWnd",    // 任务栏
            "Progman",          // 程序管理器（桌面）
            "WorkerW",          // 桌面Worker窗口
            "IME",              // 输入法窗口
            "Windows.UI.Core.CoreWindow", // UWP系统窗口
            "ApplicationFrameWindow", // UWP应用框架窗口
            "CEF-OSC-WIDGET"
        };

        // 屏幕工作区相关常量
        private const uint SPI_GETWORKAREA = 0x0030; // 获取屏幕工作区
        private const int SW_SHOWNORMAL = 1; // 正常显示状态


        // 新增：获取窗口所属进程ID
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // 系统核心进程列表
        private readonly HashSet<string> _systemProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "explorer", "svchost", "services", "lsass", "winlogon",
    "taskmgr", "dwm", "csrss", "system", "smss", "spoolsv",
    "runtimebroker", "audiodg", "fontdrvhost"
};

        // 检查窗口是否属于系统进程
        private bool IsSystemProcess(IntPtr hWnd)
        {
            try
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                using (Process process = Process.GetProcessById((int)processId))
                {
                    return _systemProcesses.Contains(process.ProcessName);
                }
            }
            catch
            {
                // 获取进程失败时，保守处理为系统进程
                return true;
            }
        }

        private void CheckWindowOverlap(object state)
        {
            // 如果自动暂停功能未启用，则直接返回
            if (!EnableAutoPause)
                return;

            // 记录当前检查时间
            DateTime currentCheckTime = DateTime.Now;

            // 防止过于频繁的检查
            if ((currentCheckTime - lastCheckTime).TotalMilliseconds < checkInterval * 0.8)
                return;

            lastCheckTime = currentCheckTime;

            // 获取屏幕工作区（排除任务栏）
            SystemParametersInfo(SPI_GETWORKAREA, 0, out RECT screenWorkArea, 0);

            // 修正：获取整个屏幕区域（包含任务栏，通过GetSystemMetrics实现）
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);
            RECT screenArea = new RECT
            {
                Left = 0,
                Top = 0,
                Right = screenWidth,
                Bottom = screenHeight
            };

            var visibleWindows = new List<IntPtr>();
            RECT overlayArea = new RECT
            {
                Left = int.MaxValue,
                Top = int.MaxValue,
                Right = int.MinValue,
                Bottom = int.MinValue
            };

            // 枚举所有顶级窗口
            EnumWindows((hWnd, lParam) =>
            {
                // 1. 必须是顶层窗口
                if (GetParent(hWnd) != IntPtr.Zero)
                    return true;

                // 2. 窗口必须可见
                if (!IsWindowVisible(hWnd))
                    return true;

                // 3. 窗口状态必须正常（允许正常显示或最大化）
                WINDOWPLACEMENT placement;
                placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                GetWindowPlacement(hWnd, out placement);

                // 允许：正常显示（SW_SHOWNORMAL=1）或最大化（SW_SHOWMAXIMIZED=3）
                if (placement.showCmd != SW_SHOWNORMAL && placement.showCmd != SW_SHOWMAXIMIZED)
                    return true;

                // 4. 排除已知系统窗口类
                StringBuilder className = new StringBuilder(256);
                GetClassName(hWnd, className, className.Capacity);
                if (_excludedClasses.Contains(className.ToString()))
                    return true;

                // 5. 排除系统进程窗口
                if (IsSystemProcess(hWnd))
                    return true;

                // 6. 窗口必须有实际大小
                if (!GetWindowRect(hWnd, out RECT windowRect))
                    return true;
                int windowWidth = windowRect.Right - windowRect.Left;
                int windowHeight = windowRect.Bottom - windowRect.Top;
                if (windowWidth < 100 || windowHeight < 50)
                    return true;

                // 7. 排除渲染器自身窗口
                StringBuilder title = new StringBuilder(256);
                GetWindowText(hWnd, title, title.Capacity);
                if (title.ToString().Trim() == wallpaperProcessName)
                    return true;

                // 根据窗口状态选择参考区域（最大化窗口用完整屏幕，否则用工作区）
                RECT referenceArea = (placement.showCmd == SW_SHOWMAXIMIZED) ? screenArea : screenWorkArea;

                // 计算与参考区域的重叠区域
                int overlapLeft = Math.Max(referenceArea.Left, windowRect.Left);
                int overlapTop = Math.Max(referenceArea.Top, windowRect.Top);
                int overlapRight = Math.Min(referenceArea.Right, windowRect.Right);
                int overlapBottom = Math.Min(referenceArea.Bottom, windowRect.Bottom);

                // 如果存在重叠区域，更新覆盖区域
                if (overlapLeft < overlapRight && overlapTop < overlapBottom)
                {
                    overlayArea.Left = Math.Min(overlayArea.Left, overlapLeft);
                    overlayArea.Top = Math.Min(overlayArea.Top, overlapTop);
                    overlayArea.Right = Math.Max(overlayArea.Right, overlapRight);
                    overlayArea.Bottom = Math.Max(overlayArea.Bottom, overlapBottom);
                }

                visibleWindows.Add(hWnd);
                return true;
            }, IntPtr.Zero);

            // 计算覆盖区域有效性
            bool hasValidOverlay = (overlayArea.Left < overlayArea.Right) && (overlayArea.Top < overlayArea.Bottom);
            float totalOverlapArea = hasValidOverlay ? (overlayArea.Right - overlayArea.Left) * (overlayArea.Bottom - overlayArea.Top) : 0;
            float wallpaperArea = (screenWorkArea.Right - screenWorkArea.Left) * (screenWorkArea.Bottom - screenWorkArea.Top);
            float overlapRatio = wallpaperArea > 0 ? totalOverlapArea / wallpaperArea : 0;

            // 控制进程状态
            if (overlapRatio >= overlapThreshold && !isWallpaperPaused)
            {
                SuspendProcess();
                isWallpaperPaused = true;
            }
            else if (overlapRatio < overlapThreshold && isWallpaperPaused)
            {
                ResumeProcess();
                isWallpaperPaused = false;
            }
        }

        // 辅助方法：获取窗口所属进程名称
        private string GetProcessName(IntPtr hWnd)
        {
            try
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                using (Process process = Process.GetProcessById((int)processId))
                {
                    return process.ProcessName;
                }
            }
            catch
            {
                return "Unknown";
            }
        }
        #endregion

        #region 进程暂停与恢复
        /// <summary>
        /// 暂停壁纸进程
        /// </summary>
        public void SuspendProcess()
        {
            try
            {
                var processes = Process.GetProcessesByName(wallpaperProcessName);
                foreach (var process in processes)
                {
                    if (process.HasExited) continue;

                    foreach (ProcessThread thread in process.Threads)
                    {
                        var threadHandle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                        if (threadHandle != IntPtr.Zero)
                        {
                            SuspendThread(threadHandle);
                            CloseHandle(threadHandle);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 忽略异常
            }
        }

        /// <summary>
        /// 恢复壁纸进程
        /// </summary>
        public void ResumeProcess()
        {
            try
            {
                var processes = Process.GetProcessesByName(wallpaperProcessName);
                foreach (var process in processes)
                {
                    if (process.HasExited) continue;

                    // 优先恢复主线程
                    ProcessThread mainThread = null;
                    uint minThreadId = uint.MaxValue;
                    foreach (ProcessThread thread in process.Threads)
                    {
                        if ((uint)thread.Id < minThreadId)
                        {
                            minThreadId = (uint)thread.Id;
                            mainThread = thread;
                        }
                    }

                    // 恢复主线程
                    if (mainThread != null)
                    {
                        var threadHandle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)mainThread.Id);
                        if (threadHandle != IntPtr.Zero)
                        {
                            ResumeThread(threadHandle);
                            CloseHandle(threadHandle);
                        }
                    }

                    // 恢复其他线程
                    foreach (ProcessThread thread in process.Threads)
                    {
                        if (thread.Id == mainThread?.Id) continue;

                        var threadHandle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                        if (threadHandle != IntPtr.Zero)
                        {
                            ResumeThread(threadHandle);
                            CloseHandle(threadHandle);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 忽略异常
            }
        }
        #endregion


        #region Windows API 声明（集中管理）

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex); // 获取系统 metrics 信息

        // 枚举窗口委托
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, out RECT pvParam, uint fWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern bool SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);


        // 结构体定义
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }


        // 枚举与常量
        [Flags]
        private enum ThreadAccess : int
        {
            TERMINATE = 0x0001,
            SUSPEND_RESUME = 0x0002,
            GET_CONTEXT = 0x0008,
            SET_CONTEXT = 0x0010,
            SET_INFORMATION = 0x0020,
            QUERY_INFORMATION = 0x0040,
            SET_THREAD_TOKEN = 0x0080,
            IMPERSONATE = 0x0100,
            DIRECT_IMPERSONATION = 0x0200
        }

        private const int SW_SHOWMINIMIZED = 2;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_CHILD = 0x40000000;
        private const int SW_SHOWMAXIMIZED = 3; // 窗口最大化状态
        private const int SM_CXSCREEN = 0;      // 屏幕宽度
        private const int SM_CYSCREEN = 1;      // 屏幕高度
        #endregion
    }
}