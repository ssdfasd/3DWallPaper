
using System.Reflection.Metadata;

namespace XCWallPaper
{
    partial class CreatorPage
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

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreatorPage));
            CodePanel = new AntdUI.Panel();
            LeftEdgePanel = new Panel();
            CodeEditor = new RichTextBox();
            CodeToolStrip = new ToolStrip();
            SaveButton = new ToolStripButton();
            CopyButton = new ToolStripButton();
            PasteButton = new ToolStripButton();
            IncreaseFontButton = new ToolStripButton();
            DecreaseFontButton = new ToolStripButton();
            StatusStrip = new StatusStrip();
            StatusLabel = new ToolStripStatusLabel();

            PreviewButton = new AntdUI.Button();
            SubmitButton = new AntdUI.Button();

            ImagePanel = new AntdUI.Panel();
            ImageFlowPanel = new FlowLayoutPanel();
            LoadImageButton = new AntdUI.Button();
            ImageContextMenu = new ContextMenuStrip();

            ParameterPanel = new AntdUI.Panel();
            ParameterFlowPanel = new FlowLayoutPanel();
            ParameterEditor = new RichTextBox();

            PassPanel = new AntdUI.Panel();
            NextPassButton = new AntdUI.Button();
            LastPassButton = new AntdUI.Button();
            CreatePassButton = new AntdUI.Button();
            DeletePassButton = new AntdUI.Button();
            PassIDLabel = new AntdUI.Label();

            DocumentButton = new AntdUI.Button();

            CodeToolStrip.SuspendLayout();
            StatusStrip.SuspendLayout();
            CodePanel.SuspendLayout();
            SuspendLayout();

            // 
            // CodeEditor
            // 
            CodeEditor.Dock = DockStyle.Fill;
            CodeEditor.Name = "CodeEditor";
            CodeEditor.TabIndex = 0;
            CodeEditor.Text = "";
            CodeEditor.Dock = DockStyle.Fill;
            // 配置代码编辑器
            CodeEditor.Font = editorFont;
            CodeEditor.BackColor = CodeEditorBackColor;
            CodeEditor.ForeColor = Color.Black;
            CodeEditor.BorderStyle = BorderStyle.None;
            CodeEditor.AcceptsTab = true;
            CodeEditor.WordWrap = false;
            CodeEditor.DetectUrls = false;
            CodeEditor.SelectionChanged += Editor_SelectionChanged;
            CodeEditor.TextChanged += Editor_TextChanged;
            CodeEditor.KeyDown += Editor_KeyDown;
            //
            // Left Edge Panel
            //
            LeftEdgePanel.Dock = DockStyle.Left;
            LeftEdgePanel.Size = new Size(20, Height);
            LeftEdgePanel.BackColor = Color.FromArgb(250, 250, 250);
            //
            // ToolStrip
            //
            CodeToolStrip = new ToolStrip();
            CodeToolStrip.BackColor = Color.White;
            CodeToolStrip.Size = new Size(740, 900); 
            UITool.Instance.ArcRegion(CodeToolStrip, 16);
            //
            // Tool Button
            //
            SaveButton.Name = "保存";
            SaveButton.ForeColor = Color.Black;
            SaveButton.Font = new Font("Microsoft YaHei UI", 12F);
            SaveButton.Click += btnSave_Click;

            CopyButton.Name = "复制";
            CopyButton.ForeColor = Color.Black;
            CopyButton.Font = new Font("Microsoft YaHei UI", 12F);
            CopyButton.Click += btnCopy_Click;

            PasteButton.Name = "粘贴";
            PasteButton.ForeColor = Color.Black;
            PasteButton.Font = new Font("Microsoft YaHei UI", 12F);
            PasteButton.Click += btnPaste_Click;

            IncreaseFontButton.Name = "增大字号";
            IncreaseFontButton.ForeColor = Color.Black;
            IncreaseFontButton.Font = new Font("Microsoft YaHei UI", 12F);
            IncreaseFontButton.Click += btnIncreaseFont_Click;

            DecreaseFontButton.Name = "减小字号";
            DecreaseFontButton.ForeColor = Color.Black;
            DecreaseFontButton.Font = new Font("Microsoft YaHei UI", 12F);
            DecreaseFontButton.Click += btnDecreaseFont_Click;
            // 
            // StatusStrip
            // 
            StatusStrip.BackColor = Color.White;
            StatusStrip.Items.AddRange(new ToolStripItem[] { StatusLabel });
            StatusStrip.Location = new Point(700, 760);
            StatusStrip.Name = "StatusStrip";
            StatusStrip.Size = new Size(740, 25);
            UITool.Instance.ArcRegion(StatusStrip, 16);
            // 
            // StatusLabel
            // 
            StatusLabel.ForeColor = Color.Black;
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new Size(167, 20);
            StatusLabel.Text = "行: 1, 列: 1 | 就绪";
            // 
            // CodePanel
            // 
            CodePanel.Name = "CodePanel";
            CodePanel.Location = new Point(700, 20);
            CodePanel.Size = new Size(740, 740);
            CodePanel.Controls.Add(CodeEditor);
            CodePanel.Controls.Add(LeftEdgePanel);
            CodePanel.Controls.Add(StatusStrip);
            CodePanel.Controls.Add(CodeToolStrip);
            CodePanel.Shadow = 10;
            CodePanel.ShadowOpacityAnimation = true;
            
            //
            // Compile Button
            //
            PreviewButton.Size = new Size(120, 80);
            PreviewButton.Location = new Point(530, 670);
            PreviewButton.Name = "CompileButton";
            PreviewButton.Click += PreviewButton_Click;
            UITool.Instance.ArcRegion(PreviewButton, 16);
            //
            // Submit  Button
            //
            SubmitButton.Size = new Size(120, 80);
            SubmitButton.Location = new Point(360, 670);
            SubmitButton.Name = "SubmitButton";
            SubmitButton.Click += SubmitButton_Click;
            UITool.Instance.ArcRegion(SubmitButton, 16);
            //
            // LoadImageButton
            //
            LoadImageButton.Size = new Size(100, 100);
            LoadImageButton.Margin = new Padding(50, 50, 50, 50);
            LoadImageButton.OriginalBackColor = ImageFlowPanel.BackColor;
            LoadImageButton.DefaultBack = Color.FromArgb(250, 250, 250);
            LoadImageButton.BackHover = Color.FromArgb(220, 220, 220);
            LoadImageButton.Shape = AntdUI.TShape.Circle;
            LoadImageButton.IconSvg = "UploadOutlined";
            LoadImageButton.IconSize = new Size(40, 40);
            LoadImageButton.Click += LoadImageButton_Click;
            // 
            // Image Panel
            //
            ImagePanel.Size = new Size(640, 250);
            ImagePanel.Location = new Point(30, 100);
            ImagePanel.Controls.Add(ImageFlowPanel);
            ImagePanel.Shadow = 10;
            ImagePanel.ShadowOpacityAnimation = true;

            // 
            // Image Flow Panel
            //
            ImageFlowPanel.Dock = DockStyle.Fill;
            ImageFlowPanel.BackColor = Color.FromArgb(240, 240, 240);
            ImageFlowPanel.FlowDirection = FlowDirection.LeftToRight;
            ImageFlowPanel.WrapContents = false;
            ImageFlowPanel.AutoScroll = true;
            UITool.Instance.ArcRegion(ImageFlowPanel, 16);
            ImageFlowPanel.Controls.Add(LoadImageButton);
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
            // Parameter Panel
            //
            ParameterPanel.Size = new Size(640, 300);
            ParameterPanel.Location = new Point(30, 360);
            ParameterPanel.Controls.Add(ParameterFlowPanel);
            ParameterPanel.Shadow = 10;
            ParameterPanel.ShadowOpacityAnimation = true;
            // 
            // Parameter Flow Panel
            //
            ParameterFlowPanel.Dock = DockStyle.Fill;
            ParameterFlowPanel.BackColor = Color.FromArgb(250, 250, 250);
            ParameterFlowPanel.AutoScroll = true;
            UITool.Instance.ArcRegion(ParameterFlowPanel, 16);
            ParameterFlowPanel.Controls.Add(ParameterEditor);
            //
            // Parameter Editor
            //
            ParameterEditor.Size = ParameterFlowPanel.Size - new Size(20, 20);
            ParameterEditor.Margin = new Padding(10, 10, 10, 10);
            ParameterEditor.Name = "ParameterEditor";
            ParameterEditor.TabIndex = 0;
            ParameterEditor.Text = "";
            // 配置代码编辑器
            ParameterEditor.Font = editorFont;
            ParameterEditor.BackColor = CodeEditorBackColor;
            ParameterEditor.ForeColor = Color.Black;
            ParameterEditor.BorderStyle = BorderStyle.None;
            ParameterEditor.AcceptsTab = true;
            ParameterEditor.WordWrap = false;
            ParameterEditor.DetectUrls = false;
            ParameterEditor.TextChanged += ParameterEditor_TextChanged;
            //
            // Pass Panel
            //
            PassPanel.Size = new Size(640, 80);
            PassPanel.Location = new Point(30, 20);
            PassPanel.BackColor = Color.FromArgb(240, 240, 240);
            PassPanel.Shadow = 10;
            PassPanel.ShadowOpacityAnimation = true;
            PassPanel.Radius = 40;
            PassPanel.Controls.Add(CreatePassButton);
            PassPanel.Controls.Add(DeletePassButton);
            PassPanel.Controls.Add(NextPassButton);
            PassPanel.Controls.Add(LastPassButton);
            PassPanel.Controls.Add(PassIDLabel);
            //
            // Next Pass Button
            //
            NextPassButton.Size = new Size(80, 80);
            NextPassButton.Dock = DockStyle.Right;
            NextPassButton.OriginalBackColor = PassPanel.BackColor;
            NextPassButton.DefaultBack = Color.FromArgb(250, 250, 250);
            NextPassButton.BackHover = Color.FromArgb(220, 220, 220);
            NextPassButton.Shape = AntdUI.TShape.Circle;
            NextPassButton.IconSvg = "CaretRightOutlined";
            NextPassButton.IconSize = new Size(30, 30);
            NextPassButton.Click += NextPassButton_Click;
            //
            // Last Pass Button
            //
            LastPassButton.Size = new Size(80, 80);
            LastPassButton.Dock = DockStyle.Left;
            LastPassButton.OriginalBackColor = PassPanel.BackColor;
            LastPassButton.DefaultBack = Color.FromArgb(250, 250, 250);
            LastPassButton.BackHover = Color.FromArgb(220, 220, 220);
            LastPassButton.Shape = AntdUI.TShape.Circle;
            LastPassButton.IconSvg = "CaretLeftOutlined";
            LastPassButton.IconSize = new Size(30, 30);
            LastPassButton.Click += LastPassButton_Click;
            //
            // CreatePassButton
            //
            CreatePassButton.Size = new Size(100, 60);
            CreatePassButton.Dock = DockStyle.Right;
            CreatePassButton.OriginalBackColor = PassPanel.BackColor;
            CreatePassButton.DefaultBack = Color.FromArgb(250, 250, 250);
            CreatePassButton.BackHover = Color.FromArgb(220, 220, 220);
            CreatePassButton.IconSvg = "FileAddOutlined";
            CreatePassButton.IconSize = new Size(30, 30);
            CreatePassButton.Click += CreatePassButton_Click;
            //
            // DeletePassButton
            //
            DeletePassButton.Size = new Size(100, 60);
            DeletePassButton.Dock = DockStyle.Left;
            DeletePassButton.OriginalBackColor = PassPanel.BackColor;
            DeletePassButton.DefaultBack = Color.FromArgb(250, 250, 250);
            DeletePassButton.BackHover = Color.FromArgb(220, 220, 220);
            DeletePassButton.IconSvg = "DeleteOutlined";
            DeletePassButton.IconSize = new Size(30, 30);
            DeletePassButton.Click += DeletePassButton_Click;
            //
            // Pass ID Label 
            //
            PassIDLabel.Dock = DockStyle.Fill;
            PassIDLabel.Text = "Pass 1";
            PassIDLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            PassIDLabel.TextAlign = ContentAlignment.MiddleCenter;
            PassIDLabel.ForeColor = Color.FromArgb(160, 160, 160);
            //
            // Document Button
            //
            DocumentButton.Name = "DocumentButton";
            DocumentButton.IconSvg = "ReadOutlined";
            DocumentButton.IconSize = new Size(20, 20);
            DocumentButton.Shape = AntdUI.TShape.Circle;
            DocumentButton.BackHover = Color.FromArgb(200, 200, 200);
            DocumentButton.Location = new Point(40, 670);
            DocumentButton.Size = new Size(60, 60);
            DocumentButton.Click += DocumentButton_Click;
            // 
            // Creator Page
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(CodePanel);
            Controls.Add(PreviewButton);
            Controls.Add(SubmitButton);
            Controls.Add(ImagePanel);
            Controls.Add(ParameterPanel);
            Controls.Add(PassPanel);
            Controls.Add(DocumentButton);
            Name = "CreatorPage";
            Size = new Size(1480, 800);
            CodeToolStrip.ResumeLayout(false);
            CodeToolStrip.PerformLayout();
            StatusStrip.ResumeLayout(false);
            StatusStrip.PerformLayout();
            CodePanel.ResumeLayout(false);
            Load += CreatorPage_Load;
            ResumeLayout(false);
            PerformLayout();

        }


        // ### Code Panel ###
        private AntdUI.Panel CodePanel;
        private RichTextBox CodeEditor;
        private Color CodeEditorBackColor = Color.FromArgb(240, 240, 240);
        private const float MIN_FONT_SIZE = 10;
        private const float MAX_FONT_SIZE = 20;

        private Panel LeftEdgePanel;
        private ToolStrip CodeToolStrip;
        private ToolStripButton SaveButton;
        private ToolStripButton CopyButton;
        private ToolStripButton PasteButton;
        private ToolStripButton DecreaseFontButton;
        private ToolStripButton IncreaseFontButton;
        private StatusStrip StatusStrip;
        private ToolStripStatusLabel StatusLabel;

        // ### Buttons ###
        private AntdUI.Button PreviewButton;
        private AntdUI.Button SubmitButton;

        // ### Image Resources ###
        private AntdUI.Panel ImagePanel;
        private FlowLayoutPanel ImageFlowPanel;
        private ContextMenuStrip ImageContextMenu;
        private AntdUI.Button LoadImageButton;

        // ### Parameters List ###
        private AntdUI.Panel ParameterPanel;
        private FlowLayoutPanel ParameterFlowPanel;
        private RichTextBox ParameterEditor;

        // ### Pass Panel ###
        private AntdUI.Panel PassPanel;
        private AntdUI.Button NextPassButton;
        private AntdUI.Button LastPassButton;
        private AntdUI.Button CreatePassButton;
        private AntdUI.Button DeletePassButton;
        private AntdUI.Label PassIDLabel;

        // ### Document Button ###
        private AntdUI.Button DocumentButton;
    }
}
