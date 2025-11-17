using AntdUI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace XCWallPaper
{
    public partial class MainForm : Form
    {
        // Drag Panel窗口拖动
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        // Pages
        private AiPage aiPage;
        private MinePage minePage;
        private AdjustPage adjustPage;
        private EffectPage effectPage;
        private CreatorPage creatorPage;

        private Control currentPage;

        private MediaFloatButton mediaFloatButton;

        public MainForm()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            InitializeNavigation();

            mediaFloatButton = new MediaFloatButton(this);

            ShowPage(aiPage);
        }
        // 在浏览器中打开指定URL
        private void OpenUrlInBrowser(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // 处理打开浏览器时可能出现的异常
                MessageBox.Show($"无法打开链接: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Navigation Panel and Pages
        private void InitializeNavigation()
        {
            // Ai Page
            aiPage = new AiPage();
            AiButton.Click += (sender, e) => ShowPage(aiPage);
            aiPage.UpdateMinePage += async (string resultPath) => {minePage.AIAddImage(resultPath); };

            // Mine Page
            minePage = new MinePage();
            minePage.CreateControl();
            minePage.LoadImagesFromFolder();
            MineButton.Click += (sender, e) => ShowPage(minePage);

            // Adjust Page
            adjustPage = new AdjustPage();
            AdjustButton.Click += (sender, e) => ShowPage(adjustPage);

            // Effect Page
            effectPage = new EffectPage();
            EffectButton.Click += (sender, e) =>
            {
                OpenUrlInBrowser("http://47.109.41.86");
            };

            // Creator Page
            creatorPage = new CreatorPage();
            CreatorButton.Click += (sender, e) => ShowPage(creatorPage);

            // Account Page
            AccountButton.Click += (sender, e) =>   
            {
                // 创建遮罩层
                AntdUI.Panel maskPanel = new AntdUI.Panel();
                maskPanel.Size = this.ClientSize;
                maskPanel.Location = new Point(0, 0);
                maskPanel.Back = Color.Transparent; // 半透明黑色
                maskPanel.ForeColor = Color.Transparent;
                maskPanel.BackColor = Color.Transparent;
                
                maskPanel.Name = "AccountMaskPanel";
                maskPanel.Visible = false;

                // 创建一个Panel作为模拟的Drawer
                AntdUI.Panel drawerPanel = new AntdUI.Panel();
                drawerPanel.Size = new Size(420, this.ClientSize.Height);
                drawerPanel.Location = new Point(this.ClientSize.Width, 0);
                drawerPanel.BackColor = Color.FromArgb(240, 240, 240);
                drawerPanel.Name = "AccountDrawerPanel";
                drawerPanel.Shadow = 10;
                drawerPanel.ShadowOpacity = 1;
                UITool.Instance.ArcRegion(drawerPanel, 20);

                // 创建内容Panel并添加到drawerPanel
                AccountPage loginPage = new AccountPage();
                loginPage.Location = new Point(10, 0);
                drawerPanel.Controls.Add(loginPage);

                // 将maskPanel和drawerPanel添加到主窗口
                this.Controls.Add(maskPanel);
                this.Controls.Add(drawerPanel);

                // 设置层级：mask在底层，drawer在上层
                this.Controls.SetChildIndex(maskPanel, 0);
                this.Controls.SetChildIndex(drawerPanel, 1);

                // 显示并设置动画效果
                maskPanel.Visible = true;

                System.Windows.Forms.Timer slideTimer = new System.Windows.Forms.Timer();
                slideTimer.Interval = 10;
                slideTimer.Tick += (timerSender, timerArgs) =>
                {
                    if (drawerPanel.Location.X > this.ClientSize.Width - 400)
                    {
                        drawerPanel.Location = new Point(drawerPanel.Location.X - 20, 0);
                    }
                    else
                    {
                        slideTimer.Stop();
                    }
                };
                slideTimer.Start();

                // 添加关闭按钮功能
                AntdUI.Button closeButton = new AntdUI.Button();
                closeButton.Text = "X";
                closeButton.Size = new Size(25, 25);
                closeButton.Location = new Point(370, 5);
                closeButton.Click += (btnSender, btnArgs) => CloseDrawer();
                drawerPanel.Controls.Add(closeButton);

                // 点击遮罩层也关闭抽屉
                maskPanel.Click += (maskSender, maskArgs) => CloseDrawer();

                // 关闭抽屉的方法
                void CloseDrawer()
                {
                    System.Windows.Forms.Timer closeTimer = new System.Windows.Forms.Timer();
                    closeTimer.Interval = 10;
                    closeTimer.Tick += (closeTimerSender, closeTimerArgs) =>
                    {
                        if (drawerPanel.Location.X < this.ClientSize.Width)
                        {
                            drawerPanel.Location = new Point(drawerPanel.Location.X + 20, 0);
                        }
                        else
                        {
                            closeTimer.Stop();
                            this.Controls.Remove(drawerPanel);
                            this.Controls.Remove(maskPanel);
                        }
                    };
                    closeTimer.Start();
                }
            };
        }


        private void LogoImage_Click(object? sender, EventArgs e)
        {
            mediaFloatButton.ToggleVisibility();
        }

        private void ShowPage(UserControl page)
        {
            if (currentPage == page)
            {
                return;
            }
            currentPage = page;
            PagePanel.Controls.Clear();
            PagePanel.Controls.Add(page);
        }



        // ###Exit Button###
        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // ###Minimum Size Button###
        private void MinimumSizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        // ###Capture Button###
        private void CaptureButton_Click(object sender, EventArgs e)
        {
            // Capture
            Image image = WallpaperCapture.Instance.CaptureTo();

            // 提醒
            AntdUI.Modal.open(new AntdUI.Modal.Config(this, "壁纸效果已经截图保存至桌面！", "", AntdUI.TType.Success)
            {
                OnButtonStyle = (id, btn) =>
                {
                    btn.DefaultBack = Color.FromArgb(240, 240, 240);
                },
                Btns = new AntdUI.Modal.Btn[]
                {
                    new AntdUI.Modal.Btn("look", "看看", AntdUI.TTypeMini.Default)
                },

                OnBtns = (btn) =>
                {
                    if (btn.Name == "look")
                    {
                        AntdUI.Preview.open(new AntdUI.Preview.Config(this, image));
                        return true;
                    }
                    return false;
                },
                LoadingDisableCancel = false,
                OkText = "好的",
                CancelText = null
            });
        }

        // ###Drag Panel###
        private void DragPanel_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
    }
}

