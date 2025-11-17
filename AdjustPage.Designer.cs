
using System.Drawing.Printing;

namespace XCWallPaper
{
    partial class AdjustPage
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
            EffectPanel = new AntdUI.Panel();
            SystemPanel = new AntdUI.Panel();
            EffectFlowPanel = new FlowLayoutPanel();
            SystemFlowPanel = new FlowLayoutPanel();
            SuspendLayout();
            //
            // Effect Flow Panel
            //
            EffectFlowPanel.BackColor = Color.FromArgb(200, 200, 200);
            EffectFlowPanel.Dock = DockStyle.Fill;
            EffectFlowPanel.FlowDirection = FlowDirection.TopDown;
            EffectFlowPanel.WrapContents = false;
            EffectFlowPanel.AutoScroll = true;
            EffectFlowPanel.Name = "特效调节";
            //
            // Effect Panel
            //
            EffectPanel.Size = new Size(680, 740);
            EffectPanel.Location = new Point(750, 0);
            EffectPanel.Controls.Add(EffectFlowPanel);
            EffectPanel.Back = Color.Transparent;
            EffectPanel.Shadow = 10;
            EffectPanel.ShadowOpacityAnimation = true;
            UITool.Instance.ArcRegion(EffectFlowPanel, 16);
            //
            // System Flow Panel
            //
            SystemFlowPanel.BackColor = Color.FromArgb(200, 200, 200);
            SystemFlowPanel.Dock = DockStyle.Fill;
            SystemFlowPanel.FlowDirection = FlowDirection.TopDown;
            SystemFlowPanel.WrapContents = false;
            SystemFlowPanel.AutoScroll = true;
            SystemFlowPanel.HorizontalScroll.Enabled = false;
            SystemFlowPanel.Name = "系统调节";
            //
            //
            // System Panel
            //
            SystemPanel.Size = new Size(680, 740);
            SystemPanel.Location = new Point(40, 0);
            SystemPanel.Controls.Add(SystemFlowPanel);
            SystemPanel.Back = Color.Transparent;
            SystemPanel.Shadow = 10;
            SystemPanel.ShadowOpacityAnimation = true;
            UITool.Instance.ArcRegion(SystemFlowPanel, 16);
            // 
            // AdjustPage
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            BackColor = Color.Transparent;
            Controls.Add(EffectPanel);
            Controls.Add(SystemPanel);
            Name = "AdjustPage";
            Size = new Size(1480, 800);
            ResumeLayout(false);
        }

        #endregion
        private AntdUI.Panel EffectPanel;
        private AntdUI.Panel SystemPanel;
        private FlowLayoutPanel EffectFlowPanel;
        private FlowLayoutPanel SystemFlowPanel;
    }
}
