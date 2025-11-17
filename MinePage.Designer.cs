namespace XCWallPaper
{
    partial class MinePage
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            ImagePanel = new AntdUI.Panel();
            ImageFlowPanel = new FlowLayoutPanel();
            ImagePagination = new AntdUI.Pagination();
            PreviewPanel = new AntdUI.Panel();
            PreviewBox = new AntdUI.Image3D();
            ImageContextMenu = new ContextMenuStrip();
            SuspendLayout();
            // 
            // Image Panel
            // 
            ImagePanel.Size = new Size(870, 720);
            ImagePanel.Controls.Add(ImageFlowPanel);
            ImagePanel.Location = new Point(30, 0);
            ImagePanel.Back = Color.Transparent;
            ImagePanel.Shadow = 10;
            ImagePanel.ShadowOpacityAnimation = true;
            // 
            // ImageFlowPanel
            // 
            ImageFlowPanel.Name = "FlowLayoutPanel";
            ImageFlowPanel.Dock = DockStyle.Fill;
            ImageFlowPanel.AutoScroll = true;
            ImageFlowPanel.BackColor = Color.FromArgb(220,220,220);
            // 设置圆角
            UITool.Instance.ArcRegion(ImageFlowPanel, 24);
            // 
            // ImagePagination
            // 
            ImagePagination.Location = new Point(30, 720);
            ImagePagination.Name = "ImagePagination";
            ImagePagination.ShowSizeChanger = true;
            ImagePagination.Size = new Size(900, 50);
            ImagePagination.Total = 1; 
            ImagePagination.PageSize = 1;
            ImagePagination.ShowSizeChanger = false;
            ImagePagination.SizeChangerWidth = 0;
            ImagePagination.ValueChanged += ImagePagination_ValueChanged;
            // 
            // PreviewPanel 
            // 
            PreviewPanel.Size = new Size(540, 300);
            PreviewPanel.Controls.Add(PreviewBox);
            PreviewPanel.Location = new Point(915, 0);
            PreviewPanel.Back = Color.Transparent;
            PreviewPanel.Shadow = 10;
            PreviewPanel.ShadowOpacity = 0.4f;
            // 
            // PreviewBox
            // 
            PreviewBox.Name = "PreviewBox";
            PreviewBox.BackColor = Color.FromArgb(220, 220, 220);
            PreviewBox.Dock = DockStyle.Fill;
            PreviewBox.Shadow = 10;
            PreviewBox.ShadowOpacity = 0.4f;
            PreviewBox.TabStop = false;
            PreviewBox.Cursor = Cursors.Hand;
            PreviewBox.Vertical = true;
            PreviewBox.Duration = 100;
            PreviewBox.DoubleClick += PreviewBox_DoubleClick;
            // 设置圆角
            UITool.Instance.ArcRegion(PreviewBox, 10);
            //
            // ImageContextMenu
            //
            ToolStripMenuItem openMenuItem = new ToolStripMenuItem("打开");
            openMenuItem.Click += OpenMenuItem_Click;

            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("删除");
            deleteMenuItem.Click += DeleteMenuItem_Click;

            ImageContextMenu.Items.Add(openMenuItem);
            ImageContextMenu.Items.Add(deleteMenuItem);
            // 
            // MinePage
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            BackColor = Color.Transparent;
            Controls.Add(PreviewPanel);
            Controls.Add(ImagePagination);
            Controls.Add(ImagePanel);
            Name = "MinePage";
            Size = new Size(1480, 800);
            ResumeLayout(false);
        }


        #endregion
        private AntdUI.Panel ImagePanel;
        private FlowLayoutPanel ImageFlowPanel;
        private AntdUI.Pagination ImagePagination;

        private AntdUI.Panel PreviewPanel;
        private AntdUI.Image3D PreviewBox;
        private ContextMenuStrip ImageContextMenu;
        private Size PreviewBoxMaxSize;
        private Point PreviewBoxOriginLocation;
    }
}