using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace XCWallPaper
{
    partial class AiPage
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            DropPanel = new AntdUI.Panel();
            DropLabel = new AntdUI.Label();
            SuspendLayout();
            // 
            // DropPanel
            // 740-180 = 560
            DropPanel.AllowDrop = true;
            DropPanel.Location = new Point(440, 250);
            DropPanel.Name = "DropPanel";
            DropPanel.Size = new Size(600, 300);
            DropPanel.TabIndex = 0;
            DropPanel.Shadow = 10;
            DropPanel.Radius = 20;
            DropPanel.ShadowOpacityAnimation = true;
            DropPanel.BackColor = Color.FromArgb(240, 240, 240);
            DropPanel.Back = Color.FromArgb(250, 250, 250);
            DropPanel.Cursor = Cursors.Hand;
            // 设置拖放事件 
            DropPanel.MouseEnter += DropPanel_MouseEnter; 
            DropPanel.MouseLeave += DropPanel_MouseLeave;
            DropPanel.DragEnter += DropPanel_DragEnter;
            DropPanel.DragLeave += DropPanel_DragLeave;
            DropPanel.DragDrop += DropPanel_DragDrop;
            DropPanel.Click += DropPanel_Click;
            // 设置圆角
            UITool.Instance.ArcRegion(DropPanel, 20);
            // 
            // DropLabel
            // 
            DropLabel.AutoSize = true;
            DropLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            DropLabel.ForeColor = Color.FromArgb(100, 100, 100); 
            DropLabel.Location = new Point(575, 570);
            DropLabel.Name = "DropLabel";
            DropLabel.Size = new Size(390, 31);
            DropLabel.TabIndex = 1;
            DropLabel.Text = "拖拽图片到上方区域或点击选择文件";
            // 
            // AiPage
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            BackColor = Color.Transparent;
            Controls.Add(DropLabel);
            Controls.Add(DropPanel);
            Name = "AiPage";
            Size = new Size(1480, 800);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private AntdUI.Panel DropPanel;
        private AntdUI.Label DropLabel;
        private AntdUI.Button LoadImageButton;
    }
}