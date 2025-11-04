using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace XCWallPaper
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr AllocConsole();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // for test
            //AllocConsole();

            // Renderer Start and Detect Pause
            RendererProcessController.Instance.StartProcess();
            RendererProcessController.Instance.InitializeTimer();

            // Load Settings
            PathManager.Instance.LoadSettings();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());

            // Renderer Exit
            RendererProcessController.Instance.ExitProcess();
        }
    }
}
