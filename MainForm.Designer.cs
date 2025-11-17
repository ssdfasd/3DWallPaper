
namespace XCWallPaper
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            // Ai Page Closed
            aiPage.Closed();

            // Creator Page Closed
            creatorPage.SaveProject();

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ExitButton = new AntdUI.Button();
            MinimumSizeButton = new AntdUI.Button();
            CaptureButton = new AntdUI.Button();
            DragPanel = new Panel();
            NavigationPanel = new AntdUI.Panel();
            LogoImage = new AntdUI.Button();
            MediaToolTip = new AntdUI.TooltipComponent();
            AccountButton = new AntdUI.Button();
            AiButton = new AntdUI.Button();
            AdjustButton = new AntdUI.Button();
            EffectButton = new AntdUI.Button();
            CreatorButton = new AntdUI.Button();
            PagePanel = new Panel();
            MineButton = new AntdUI.Button();
            DragPanel.SuspendLayout();
            NavigationPanel.SuspendLayout();
            SuspendLayout();
            // 
            // ExitButton
            // 
            ExitButton.Name = "ExitButton";
            ExitButton.Shape = AntdUI.TShape.Circle;
            ExitButton.IconSvg = "CloseOutlined";
            ExitButton.IconSize = new Size(20, 20);
            ExitButton.BackHover = Color.FromArgb(255, 100, 100);
            ExitButton.Location = new Point(1540, 8);
            ExitButton.Size = new Size(50, 50);
            ExitButton.Click += ExitButton_Click;
            // 
            // MinimumSizeButton
            // 
            MinimumSizeButton.Name = "MinimumSizeButton";
            MinimumSizeButton.IconSvg = "MinusOutlined";
            MinimumSizeButton.IconSize = new Size(20, 20);
            MinimumSizeButton.BackHover = Color.FromArgb(200, 200, 200);
            MinimumSizeButton.Location = new Point(1490, 8);
            MinimumSizeButton.Size = new Size(50, 50);
            MinimumSizeButton.Click += MinimumSizeButton_Click;
            // 
            // CaptureButton
            // 
            CaptureButton.Name = "CaptureButton";
            CaptureButton.IconSvg = "CameraOutlined";
            CaptureButton.IconSize = new Size(20, 20);
            CaptureButton.BackHover = Color.FromArgb(200, 200, 200);
            CaptureButton.Location = new Point(1440, 8);
            CaptureButton.Size = new Size(50, 50);
            CaptureButton.Click += CaptureButton_Click;
            // 
            // DragPanel
            // 
            DragPanel.BackColor = Color.Transparent;
            DragPanel.Controls.Add(MinimumSizeButton);
            DragPanel.Controls.Add(ExitButton);
            DragPanel.Controls.Add(CaptureButton);
            DragPanel.Name = "DragPanel";
            DragPanel.Size = new Size(1600, 60);
            DragPanel.TabIndex = 2;
            DragPanel.MouseDown += DragPanel_MouseDown;
            // 
            // NavigationPanel
            // 
            NavigationPanel.Dock = DockStyle.Left;
            NavigationPanel.Location = new Point(0, 0);
            NavigationPanel.Name = "NavigationPanel";
            NavigationPanel.Size = new Size(120, 900);
            NavigationPanel.Back = Color.FromArgb(200, 200, 200);
            NavigationPanel.Shadow = 8;
            NavigationPanel.ShadowOpacity = 0.5f;
            NavigationPanel.Controls.Add(LogoImage);
            NavigationPanel.Controls.Add(CreatorButton);
            NavigationPanel.Controls.Add(EffectButton);
            NavigationPanel.Controls.Add(AdjustButton);
            NavigationPanel.Controls.Add(MineButton);
            NavigationPanel.Controls.Add(AiButton);
            NavigationPanel.Controls.Add(AccountButton);
            // 
            // LogoImage
            // 
            LogoImage.Name = "LogoImage";
            LogoImage.Location = new Point(15, 20);
            LogoImage.Size = new Size(90, 90);
            LogoImage.BackHover = XCTheme.Instance.NavigationButtonBackHover;
            LogoImage.DefaultBorderColor = XCTheme.Instance.NavigationButtonDefaultBorderColor;
            LogoImage.BackActive = XCTheme.Instance.NavigationButtonBackActive;
            LogoImage.ForeColor = XCTheme.Instance.NavigationButtonForeColor;
            LogoImage.BorderWidth = 3;
            LogoImage.Click += LogoImage_Click;
            //
            // MediaToolTip
            //
            MediaToolTip.ArrowAlign = AntdUI.TAlign.Right;
            MediaToolTip.Font = new Font("Microsoft YaHei UI", 12F);
            //MediaToolTip.SetTip(LogoImage, "联系我们");
            // 
            // AccountButton
            // 
            AccountButton.Name = "AccountButton";
            AccountButton.Location = new Point(15, 790);
            AccountButton.Size = new Size(90, 90);
            AccountButton.IconSvg = "UserOutlined";
            AccountButton.IconSize = new Size(30, 30);
            AccountButton.BackHover = XCTheme.Instance.NavigationButtonBackHover;
            AccountButton.DefaultBorderColor = XCTheme.Instance.NavigationButtonDefaultBorderColor;
            AccountButton.BackActive = XCTheme.Instance.NavigationButtonBackActive;
            AccountButton.ForeColor = XCTheme.Instance.NavigationButtonForeColor;
            AccountButton.BorderWidth = 3;
            // 
            // AiButton
            // 
            AiButton.Name = "AiButton";
            AiButton.Location = new Point(15, 160);
            AiButton.Size = new Size(90, 90);
            AiButton.IconSvg = "HomeOutlined";
            AiButton.IconSize = new Size(30, 30);
            AiButton.BackHover = XCTheme.Instance.NavigationButtonBackHover;
            AiButton.DefaultBorderColor = XCTheme.Instance.NavigationButtonDefaultBorderColor;
            AiButton.BackActive = XCTheme.Instance.NavigationButtonBackActive;
            AiButton.ForeColor = XCTheme.Instance.NavigationButtonForeColor;
            AiButton.BorderWidth = 3;
            // 
            // MineButton
            // 
            MineButton.Name = "MineButton";
            MineButton.Location = new Point(15, 250);
            MineButton.Size = new Size(90, 90);
            MineButton.IconSvg = "PictureOutlined";
            MineButton.IconSize = new Size(30, 30);
            MineButton.BackHover = XCTheme.Instance.NavigationButtonBackHover;
            MineButton.DefaultBorderColor = XCTheme.Instance.NavigationButtonDefaultBorderColor;
            MineButton.BackActive = XCTheme.Instance.NavigationButtonBackActive;
            MineButton.ForeColor = XCTheme.Instance.NavigationButtonForeColor;
            MineButton.BorderWidth = 3;
            // 
            // AdjustButton
            // 
            AdjustButton.Name = "AdjustButton";
            AdjustButton.Location = new Point(15, 340);
            AdjustButton.Size = new Size(90, 90);
            AdjustButton.IconSvg = "ControlOutlined";
            AdjustButton.IconSize = new Size(30, 30);
            AdjustButton.BackHover = XCTheme.Instance.NavigationButtonBackHover;
            AdjustButton.DefaultBorderColor = XCTheme.Instance.NavigationButtonDefaultBorderColor;
            AdjustButton.BackActive = XCTheme.Instance.NavigationButtonBackActive;
            AdjustButton.ForeColor = XCTheme.Instance.NavigationButtonForeColor;
            AdjustButton.BorderWidth = 3;
            // 
            // EffectButton
            // 
            EffectButton.Name = "EffectButton";
            EffectButton.Location = new Point(15, 430);
            EffectButton.Size = new Size(90, 90);
            EffectButton.IconSvg = "ShopOutlined";
            EffectButton.IconSize = new Size(30, 30);
            EffectButton.BackHover = XCTheme.Instance.NavigationButtonBackHover;
            EffectButton.DefaultBorderColor = XCTheme.Instance.NavigationButtonDefaultBorderColor;
            EffectButton.BackActive = XCTheme.Instance.NavigationButtonBackActive;
            EffectButton.ForeColor = XCTheme.Instance.NavigationButtonForeColor;
            EffectButton.BorderWidth = 3;
            // 
            // CreatorButton
            // 
            CreatorButton.Name = "CreatorButton";
            CreatorButton.Location = new Point(15, 520);
            CreatorButton.Size = new Size(90, 90);
            CreatorButton.IconSvg = "CodeOutlined";
            CreatorButton.IconSize = new Size(30, 30);
            CreatorButton.BackHover = XCTheme.Instance.NavigationButtonBackHover;
            CreatorButton.DefaultBorderColor = XCTheme.Instance.NavigationButtonDefaultBorderColor;
            CreatorButton.BackActive = XCTheme.Instance.NavigationButtonBackActive;
            CreatorButton.ForeColor = XCTheme.Instance.NavigationButtonForeColor;
            CreatorButton.BorderWidth = 3;
            // 
            // 
            // PagePanel
            // 
            PagePanel.BackColor = Color.Transparent;
            PagePanel.Location = new Point(120, 100);
            PagePanel.Name = "PagePanel";
            PagePanel.Size = new Size(1480, 800);
            PagePanel.TabIndex = 8;
            // 
            // MainForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.FromArgb(240, 240, 240);
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1600, 900);
            ControlBox = false;
            Controls.Add(NavigationPanel);
            Controls.Add(PagePanel);
            Controls.Add(DragPanel);
            DoubleBuffered = true;
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            ShowIcon = false;
            DragPanel.ResumeLayout(false);
            NavigationPanel.ResumeLayout(false);
            ResumeLayout(false);
            // 设置圆角
            UITool.Instance.ArcRegion(this, 16);
        }
        #endregion

        private AntdUI.Button ExitButton;
        private AntdUI.Button MinimumSizeButton;
        private AntdUI.Button CaptureButton;
        private Panel DragPanel;

        private AntdUI.Panel NavigationPanel;
        private AntdUI.Button AccountButton;
        private AntdUI.Button LogoImage;
        private AntdUI.Button AiButton;
        private AntdUI.Button MineButton;
        private AntdUI.Button AdjustButton;
        private AntdUI.Button EffectButton;
        private AntdUI.Button CreatorButton;
        private Panel PagePanel;

        private AntdUI.FormFloatButton MediaFloatButton;
        private AntdUI.TooltipComponent MediaToolTip;
    }
}