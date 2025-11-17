using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace XCWallPaper
{
    public partial class CreatorPage : UserControl
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        private const int WM_SETREDRAW = 11;

        private int currentLine = 1;
        private int currentColumn = 1;
        private Font editorFont = new Font("Microsoft YaHei", 12);

        private bool isAdjustingFont = false;

        private Dictionary<int, string> passHLSLCodes = new Dictionary<int, string>();
        private Dictionary<int, Dictionary<int, PictureBox>> passImages = new Dictionary<int, Dictionary<int, PictureBox>>();
        private Dictionary<int, string> passParameters = new Dictionary<int, string>();
        private int PassID = 0;
        private const int MassPassCount = 8;
        private int PassCount = 0;
        private const int ThumbSize = 180; // 缩略图大小（正方形）
        private PictureBox currentThumbBox;
        private const string hlslInitCode =
@"
[numthreads(8, 8, 1)]
void Main(uint3 id : SV_DispatchThreadID)
{

}
";
        public CreatorPage()
        {
            InitializeComponent();
            HighlightSyntax();
            UpdateStatusBar();
        }

        private void CreatorPage_Load(object sender, EventArgs e)
        {
            // Initialize
            InitializeProject();
        }

        // ### Code Panel ###
        private void HighlightSyntax(bool fullDocument = true, int? specificLine = null)
        {
            // 保存当前位置
            int start = CodeEditor.SelectionStart;
            int length = CodeEditor.SelectionLength;
            Color originalColor = CodeEditor.SelectionColor;

            // 暂停重绘
            SendMessage(CodeEditor.Handle, WM_SETREDRAW, false, 0);

            try
            {
                if (fullDocument)
                {
                    // 重置所有文本为默认样式
                    CodeEditor.SelectAll();
                    CodeEditor.SelectionColor = Color.FromArgb(30, 30, 30);
                    CodeEditor.SelectionFont = editorFont;
                    CodeEditor.SelectionBackColor = CodeEditorBackColor;
                    CodeEditor.DeselectAll();
                }

                // 确定需要高亮的行范围
                int startLine = specificLine ?? 0;
                int endLine = specificLine.HasValue ? startLine : CodeEditor.Lines.Length - 1;

                for (int lineIndex = startLine; lineIndex <= endLine; lineIndex++)
                {
                    // 跳过空行
                    if (lineIndex >= CodeEditor.Lines.Length) continue;

                    int lineStart = CodeEditor.GetFirstCharIndexFromLine(lineIndex);
                    if (lineStart == -1) continue;

                    string lineText = CodeEditor.Lines[lineIndex];
                    int lineLength = lineText.Length;

                    // 重置当前行样式
                    CodeEditor.Select(lineStart, lineLength);
                    CodeEditor.SelectionColor = Color.FromArgb(30, 30, 30);
                    CodeEditor.SelectionFont = editorFont;
                    CodeEditor.SelectionBackColor = CodeEditorBackColor;
                    CodeEditor.DeselectAll();

                    // 应用高亮规则
                    HighlightKeywordsInLine(lineText, lineStart, new[] { "#include", "#define", "#if", "#ifdef", "#ifndef", "#else", "#elif", "#endif", "#undef" }, Color.FromArgb(198, 120, 221));
                    HighlightStringsInLine(lineText, lineStart, "\"", Color.FromArgb(206, 145, 120));
                    HighlightKeywordsInLine(lineText, lineStart, new[] { "cbuffer", "struct", "int4", "int3", "int2", "uint4", "uint3", "uint2",
                "float2x2", "float3x3", "float4x4", "float4", "float3", "float2",
                "Texture2D", "Texture3D", "RWStructuredBuffer", "StructuredBuffer", "SamplerState" }, Color.FromArgb(229, 192, 123));
                    HighlightKeywordsInLine(lineText, lineStart, new[] { "void", "int", "uint", "float" }, Color.FromArgb(198, 120, 221));
                    HighlightKeywordsInLine(lineText, lineStart, new[] { "=", "+", "-", "*", "/", "|", "%" }, Color.FromArgb(97, 175, 239));
                    HighlightPatternInLine(lineText, lineStart, @"\b[a-zA-Z_][a-zA-Z0-9_]*\s*(?=\()", Color.FromArgb(97, 175, 239));
                    HighlightKeywordsInLine(lineText, lineStart, new[] { "if", "else", "for", "while", "return", "break", "continue", "register" }, Color.FromArgb(198, 120, 221));
                    HighlightPatternInLine(lineText, lineStart, @"(?<![a-zA-Z_])[+-]?[0-9]+\.?[0-9]?(([eE][+-]?)?[0-9]+)?", Color.FromArgb(190, 138, 89));
                    HighlightCommentsInLine(lineText, lineStart, "//", Color.FromArgb(144, 176, 97));
                }
            }
            finally
            {
                // 恢复位置
                CodeEditor.Select(start, length);
                CodeEditor.SelectionColor = originalColor;

                // 恢复重绘并刷新
                SendMessage(CodeEditor.Handle, WM_SETREDRAW, true, 0);
                CodeEditor.Invalidate();
            }
        }

        // 修改后的辅助方法 - 只处理单行
        private void HighlightKeywordsInLine(string lineText, int lineStart, string[] keywords, Color color)
        {
            string pattern = @"\b(" + string.Join("|", keywords.Select(Regex.Escape)) + @")\b";
            MatchCollection matches = Regex.Matches(lineText, pattern);

            foreach (Match match in matches)
            {
                CodeEditor.Select(lineStart + match.Index, match.Length);
                CodeEditor.SelectionColor = color;
            }
        }

        private void HighlightStringsInLine(string lineText, int lineStart, string delimiter, Color color)
        {
            string pattern = $@"{Regex.Escape(delimiter)}(\\.|[^\\{delimiter}])*?{Regex.Escape(delimiter)}";
            MatchCollection matches = Regex.Matches(lineText, pattern);

            foreach (Match match in matches)
            {
                CodeEditor.Select(lineStart + match.Index, match.Length);
                CodeEditor.SelectionColor = color;
            }
        }

        private void HighlightCommentsInLine(string lineText, int lineStart, string singleLine, Color color)
        {
            string pattern = $@"{Regex.Escape(singleLine)}.*";
            MatchCollection matches = Regex.Matches(lineText, pattern);

            foreach (Match match in matches)
            {
                CodeEditor.Select(lineStart + match.Index, match.Length);
                CodeEditor.SelectionColor = color;
            }
        }

        private void HighlightPatternInLine(string lineText, int lineStart, string pattern, Color color)
        {
            MatchCollection matches = Regex.Matches(lineText, pattern);
            foreach (Match match in matches)
            {
                CodeEditor.Select(lineStart + match.Index, match.Length);
                CodeEditor.SelectionColor = color;
            }
        }

        private void UpdateStatusBar()
        {
            StatusLabel.Text = $"    行: {currentLine}, 列: {currentColumn} | 长度: {CodeEditor.Text.Length} 字符 | HLSL 模式";
        }

        private void Editor_SelectionChanged(object sender, EventArgs e)
        {
            // 计算当前行和列
            int selectionStart = CodeEditor.SelectionStart;
            currentLine = CodeEditor.GetLineFromCharIndex(selectionStart) + 1;

            int firstChar = CodeEditor.GetFirstCharIndexOfCurrentLine();
            currentColumn = selectionStart - firstChar + 1;

            UpdateStatusBar();
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            // 只有当不是在调整字体时，才执行语法高亮
            if (!isAdjustingFont)
            {
                HighlightSyntax(false, CodeEditor.GetLineFromCharIndex(CodeEditor.SelectionStart));
            }
        }

        private void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            // 自动缩进
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                int currentPos = CodeEditor.SelectionStart;
                int currentLine = CodeEditor.GetLineFromCharIndex(currentPos);
                int lineStart = CodeEditor.GetFirstCharIndexFromLine(currentLine);
                int indentLength = 0;

                // 计算当前行的缩进
                while (lineStart + indentLength < CodeEditor.Text.Length &&
                       CodeEditor.Text[lineStart + indentLength] == ' ')
                {
                    indentLength++;
                }

                // 插入新行和相同的缩进
                CodeEditor.SelectedText = "\n" + new string(' ', indentLength);

                // 如果上一行以 { 结尾，增加缩进
                if (currentLine > 0)
                {
                    string prevLine = CodeEditor.Lines[currentLine - 1].TrimEnd();
                    if (prevLine.EndsWith("{") && !prevLine.TrimStart().StartsWith("//"))
                    {
                        CodeEditor.SelectedText = new string(' ', 4);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            MessageBox.Show("代码已保存！", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CodeEditor.SelectedText))
            {
                CodeEditor.SelectAll();
            }
            Clipboard.SetText(CodeEditor.SelectedText);
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                CodeEditor.Paste();
            }
        }

        private void btnIncreaseFont_Click(object sender, EventArgs e)
        {
            float newSize = CodeEditor.Font.Size + 1;
            if (newSize <= MAX_FONT_SIZE)
            {
                // 暂停重绘
                SendMessage(CodeEditor.Handle, WM_SETREDRAW, false, 0);
                try
                {
                    isAdjustingFont = true; // 设置标志位，表示正在调整字体

                    editorFont = new Font(CodeEditor.Font.FontFamily, newSize);
                    CodeEditor.Font = editorFont;

                    // 仅当有文本时才进行高亮
                    if (CodeEditor.TextLength > 0)
                    {
                        HighlightSyntax();
                    }
                }
                finally
                {
                    isAdjustingFont = false; // 清除标志位
                                             // 恢复重绘
                    SendMessage(CodeEditor.Handle, WM_SETREDRAW, true, 0);
                    CodeEditor.Invalidate();
                }
            }
        }

        private void btnDecreaseFont_Click(object sender, EventArgs e)
        {
            float newSize = CodeEditor.Font.Size - 1;
            if (newSize >= MIN_FONT_SIZE)
            {
                // 暂停重绘
                SendMessage(CodeEditor.Handle, WM_SETREDRAW, false, 0);
                try
                {
                    isAdjustingFont = true; // 设置标志位，表示正在调整字体

                    editorFont = new Font(CodeEditor.Font.FontFamily, newSize);
                    CodeEditor.Font = editorFont;

                    // 仅当有文本时才进行高亮
                    if (CodeEditor.TextLength > 0)
                    {
                        HighlightSyntax();
                    }
                }
                finally
                {
                    isAdjustingFont = false; // 清除标志位
                                             // 恢复重绘
                    SendMessage(CodeEditor.Handle, WM_SETREDRAW, true, 0);
                    CodeEditor.Invalidate();
                }
            }
        }

        // ### Preview or Submit ###

        private (List<string>, List<EffectParameter>, string) PackageProject()
        {
            // Save Project
            SaveProject();

            // Process Image Path
            List<string> imageNames = new List<string>();
            for (int i = 1; i <= passImages.Count; i++)
            {
                for (int j = 1; j <= passImages[i].Count; j++)
                {
                    string fileName = Path.GetFileName((string)passImages[i][j].Tag);
                    imageNames.Add(Path.GetFileNameWithoutExtension(fileName));
                }
            }

            // Process Parameters
            List<EffectParameter> effectParameters = ProcessParameters();

            // Process HLSLCodes
            string hlslCode = string.Join(
                $"{EffectPackager.Instance.HLSLPassSeparator}{Environment.NewLine}",
                passHLSLCodes.OrderBy(kv => kv.Key).Select(kv => kv.Value)
            );

            return (imageNames, effectParameters, hlslCode);
        }
        private void PreviewButton_Click(object sender, EventArgs e)
        {
            // 课程展示，这里暂时设置成了解包按钮
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "特效文件|*.xcpe";
                openFileDialog.Multiselect = false;  // 启用多选
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string packageName = Path.GetFileNameWithoutExtension(openFileDialog.FileNames[0]);
                    string packagePath = EffectPackager.Instance.GetPackagePath(packageName);
                    Console.WriteLine(packagePath);
                    var effectPackage = EffectPackager.Instance.UnpackageEffect(packagePath, "my_secure_key");
                    EffectPlayer.Instance.AddEffect(effectPackage);
                }
            }

            return;
            /*
            // Clear Effects
            EffectPlayer.Instance.ClearEffects();

            // Package Project
            var (imageNames, effectParameters, hlslCode) = PackageProject();

            EffectMetadata metadata = new EffectMetadata
            {
                ID = "00000000",
                EffectName = "TestEffectName",
                Author = "TestAuthor",
                Description = "TestDescription",
                CreateTime = DateTime.Now,
                Version = 0.ToString()
            };

            string packagePath = EffectPackager.Instance.GetPackagePath("test");

            // 临时封装
            EffectPackager.Instance.PackageEffect(hlslCode, imageNames, metadata, effectParameters, packagePath);

            // 解封装
            var effectPackage = EffectPackager.Instance.UnpackageEffect(packagePath);
            
            // Add Effect
            EffectPlayer.Instance.AddEffect(effectPackage);

            //EffectPlayer.Instance.UpdateEffectParameters("00000000", new EffectParameterMessage { PassID = 1, Type = "float3", Name = "dd", Value = new float[] { 1, 0, 0 } });

            //EffectPlayer.Instance.RemoveEffect("00000000");////////////////////////////////////////////////////////////////////////////////////////////////
            */
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "请输入特效信息";
                form.Size = new Size(400, 330);
                form.StartPosition = FormStartPosition.CenterScreen;
                form.AutoScaleMode = AutoScaleMode.None;
                form.ControlBox = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.BackColor = Color.FromArgb(155, 155, 155);
                UITool.Instance.ArcRegion(form, 16);


                // 创建特效名标签和输入框
                var nameLabel = new Label() { Text = "特效名:", Left = 20, Top = 20, Width = 80 };
                var nameTextBox = new TextBox() { Left = 110, Top = 20, Width = 250 };

                // 创建版本号标签和NumericUpDown控件
                var versionLabel = new Label() { Text = "版本号:", Left = 20, Top = 60, Width = 80 };
                var versionNumericUpDown = new NumericUpDown()
                {
                    Left = 110,
                    Top = 60,
                    Width = 80,
                    DecimalPlaces = 1,
                    Increment = 0.1m,
                    Minimum = 0.1m,
                    Maximum = 10.0m,
                    Value = 1
                };

                // 创建效果描述标签和多行文本框
                var descLabel = new Label() { Text = "效果描述:", Left = 20, Top = 100, Width = 80 };
                var descTextBox = new TextBox()
                {
                    Left = 110,
                    Top = 100,
                    Width = 250,
                    Height = 120,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical
                };

                // 创建确认和取消按钮
                var confirmButton = new Button() { Text = "确认", Left = 110, Top = 240, Width = 80 };
                var cancelButton = new Button() { Text = "取消", Left = 280, Top = 240, Width = 80 };

                // 添加控件到窗体
                form.Controls.AddRange(new Control[] {
            nameLabel, nameTextBox,
            versionLabel, versionNumericUpDown,
            descLabel, descTextBox,
            confirmButton, cancelButton
        });

                // 设置对话框结果
                confirmButton.DialogResult = DialogResult.OK;
                cancelButton.DialogResult = DialogResult.Cancel;
                form.AcceptButton = confirmButton;
                form.CancelButton = cancelButton;

                // 显示对话框
                if (form.ShowDialog() == DialogResult.OK)
                {
                    string effectName = nameTextBox.Text;
                    int version = (int)versionNumericUpDown.Value;
                    string effectDescription = descTextBox.Text;

                    SubmitEffect(effectName, effectDescription, version);
                }
                else
                {
                    // 用户取消了操作，可以选择退出函数
                    return;
                }
            }
        }

        private void SubmitEffect(string effectName, string effectDescription, int version)
        {
            // Package Project
            var (imagePaths, effectParameters, hlslCode) = PackageProject();

            Test.Instance.PrintEffectParameters(effectParameters);

            /////////////////////////////////服务器请求获取唯一ID
            EffectMetadata metadata = new EffectMetadata
            {
                ID = "1111111111111111111",
                EffectName = effectName,
                Author = "创作者用户名",////////////////////////////////////////
                Description = effectDescription,
                CreateTime = DateTime.Now,
                Version = version.ToString()
            };

            string packagePath = EffectPackager.Instance.GetPackagePath(effectName);
            //////////////////////////////////服务器请求获取加密key
            EffectPackager.Instance.PackageEffect(hlslCode, imagePaths, metadata, effectParameters, packagePath, "my_secure_key");

            for (int i = 0; i < imagePaths.Count; i++) { 
            Console.WriteLine(imagePaths[i]);
            }

            // 完整解封装
            //var effectPackage = EffectPackager.Instance.UnpackageEffect(packagePath, "my_secure_key");
            //Test.Instance.ShowEffectPackage(effectPackage, PathManager.Instance.TempDirectory);

            ///////////////////////////////////////上传服务器
        }


        // ### Project ###
        private void InitializeProject()
        {
            List<string> passPaths = PathManager.Instance.GetAllProjectPassFolders();
            if(passPaths.Count == 0)
            {
                CreatePassButton_Click(null, null);
                return;
            }
            for (int i = 1; i <= passPaths.Count; i++)
            {
                CreatePassButton_Click(null, null);

                // HLSL
                string hlslCode = PathManager.Instance.LoadProjectHLSL("pass" + i);
                CodeEditor.Text = hlslCode;

                // Parameters
                string parameters = PathManager.Instance.LoadProjectParameters("pass" + i);
                ParameterEditor.Text = parameters;

                // Image
                LoadImages();
            }

            // HLSL
            HighlightSyntax();

            // Parameters
            HighlightParameterSyntax();

            // Image
            UpdateImagePanel();
        }

        public void SaveProject()
        {
            // Save HLSL Code
            passHLSLCodes[PassID] = CodeEditor.Text;
            PathManager.Instance.SaveProjectHLSL("pass" + PassID, passHLSLCodes[PassID]);

            // Save Parameter List
            passParameters[PassID] = ParameterEditor.Text;
            PathManager.Instance.SaveProjectParameters("pass" + PassID, passParameters[PassID]);
        }

        // ### Pass Panel ###
        private void DeletePassButton_Click(object sender, EventArgs e)
        {
            if (PassCount <= 1) { return; }

            LastPassButton_Click(sender, EventArgs.Empty);

            // Remove 
            PathManager.Instance.DeleteProjectFolder("pass" + (PassID + 1));
            passHLSLCodes.Remove(PassID + 1);
            passParameters.Remove(PassID + 1);
            passImages.Remove(PassID + 1);

            // Rank
            for (int i = PassID + 2; i <= PassCount; i++)
            {
                PathManager.Instance.RenameProjectFolder("pass" + i, "pass" + (i - 1));
                
                passHLSLCodes.Add(i - 1, passHLSLCodes[i]);
                passHLSLCodes.Remove(i);

                passParameters.Add(i - 1, passParameters[i]);
                passParameters.Remove(i);

                passImages.Add(i - 1, passImages[i]);
                passImages.Remove(i);
            }

            PassCount--;
        }

        private void CreatePassButton_Click(object sender, EventArgs e)
        {
            if (PassCount >= MassPassCount) { return; }

            // Rank
            for (int i = PassCount; i >= PassID + 1; i--)
            {
                PathManager.Instance.RenameProjectFolder("pass" + i, "pass" + (i + 1));

                passHLSLCodes.Add(i + 1, passHLSLCodes[i]);
                passHLSLCodes.Remove(i);

                passParameters.Add(i + 1, passParameters[i]);
                passParameters.Remove(i);

                passImages.Add(i + 1, passImages[i]);
                passImages.Remove(i);
            }

            // Add
            PathManager.Instance.CreateProjectFolder("pass" + (PassID + 1));
            passHLSLCodes.Add(PassID + 1, hlslInitCode);
            passParameters.Add(PassID + 1, "");
            passImages.Add(PassID + 1, new Dictionary<int, PictureBox>());

            PassCount++;

            NextPassButton_Click(sender, EventArgs.Empty);
        }

        private void LastPassButton_Click(object sender, EventArgs e)
        {
            if (PassID <= 1) { return; }

            // Save HLSL Code
            passHLSLCodes[PassID] = CodeEditor.Text;
            PathManager.Instance.SaveProjectHLSL("pass" + PassID, passHLSLCodes[PassID]);

            // Save Parameters
            passParameters[PassID] = ParameterEditor.Text;
            PathManager.Instance.SaveProjectParameters("pass" + PassID, passParameters[PassID]);

            // Update Label
            PassID--;
            PassIDLabel.Text = "Pass " + PassID;

            // Update HLSL Code
            CodeEditor.Text = passHLSLCodes[PassID];
            HighlightSyntax();

            // Update Parameters
            ParameterEditor.Text = passParameters[PassID];
            HighlightParameterSyntax();

            // Update Image Panel
            UpdateImagePanel();
        }

        private void NextPassButton_Click(object sender, EventArgs e)
        {
            if (PassID >= PassCount) { return; }

            if (PassID != 0) 
            {
                // Save HLSL Code
                passHLSLCodes[PassID] = CodeEditor.Text;
                PathManager.Instance.SaveProjectHLSL("pass" + PassID, passHLSLCodes[PassID]);

                // Save Parameters
                passParameters[PassID] = ParameterEditor.Text;
                PathManager.Instance.SaveProjectParameters("pass" + PassID, passParameters[PassID]);
            }

            PassID++;

            // Update Label
            PassIDLabel.Text = "Pass " + PassID;

            // Update HLSL Code
            CodeEditor.Text = passHLSLCodes[PassID];
            HighlightSyntax();

            // Update Paramaters
            ParameterEditor.Text= passParameters[PassID];
            HighlightParameterSyntax();

            // Update Image Panel
            UpdateImagePanel();
        }


        // ### Image Resource Panel ###
        private void LoadImageButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Multiselect = true;  // 启用多选
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    AddImages(openFileDialog.FileNames);
                }
            }
            LoadImages();
            UpdateImagePanel();
        }

        private void AddImages(string[] imagePaths)
        {
            string passPath = PathManager.Instance.GetProjectPassFolder("pass" + PassID);
            if (!Directory.Exists(passPath)) return;

            for (int i = 1; i <= imagePaths.Length; i++) 
            {
                string imagePath = imagePaths[i - 1];
                int imageID = passImages[PassID].Count + i;
                string imageName = $"image{PassID}_{imageID}.png";
                File.Copy(imagePath, Path.Combine(passPath, imageName), true);
            }
        }

        private List<string> GetPassImages()
        {
            string passPath = PathManager.Instance.GetProjectPassFolder("pass" + PassID);
            if (!Directory.Exists(passPath)) return null;

            var extensions = new[] { ".png" };

            return Directory.GetFiles(passPath, "*.*", SearchOption.AllDirectories)
                .Where(f => {
                    string ext = Path.GetExtension(f).ToLower();
                    string fileName = Path.GetFileNameWithoutExtension(f);
                    return extensions.Contains(ext);
                })
                .ToList();
        }

        private void LoadImages()
        {
            List<string> imagePaths = GetPassImages();

            if (!passImages.ContainsKey(PassID)) 
            {
                passImages.Add(PassID, new Dictionary<int, PictureBox>());
            }
            passImages[PassID].Clear();

            for (int i = 1; i <= imagePaths.Count; i++)
            {
                string path = imagePaths[i - 1];
                PictureBox thumbBox = new PictureBox
                {
                    Size = new Size(ThumbSize, ThumbSize),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Tag = path,
                    Cursor = Cursors.Hand,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(0, 10, 0, 10),
                };

                // 异步加载图片防止UI冻结
                Task.Run(() =>
                {
                    var image = UITool.Instance.LoadImageSquared(path, ThumbSize);
                    thumbBox.Image = image;
                });

                thumbBox.DoubleClick += ThumbBox_DoubleClick;
                thumbBox.MouseDown += ThumbBox_MouseDown;

                passImages[PassID].Add(i, thumbBox);
            }
        }

        private void UpdateImagePanel()
        {
            ImageFlowPanel.Controls.Clear();

            for (int i = 1; i <= passImages[PassID].Count; i++) 
            {
                ImageFlowPanel.Controls.Add(passImages[PassID][i]);
            }

            ImageFlowPanel.Controls.Add(LoadImageButton);
        }

        private async void ThumbBox_MouseDown(object sender, MouseEventArgs e)
        {
            currentThumbBox = ((PictureBox)sender);
            if (e.Button == MouseButtons.Right)
            {
                currentThumbBox.ContextMenuStrip = ImageContextMenu;
            }
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            string currentImagePath = (string)currentThumbBox?.Tag;
            if (!string.IsNullOrEmpty(currentImagePath))
            {
                OpenImageAsync(currentImagePath).ConfigureAwait(false);
            }
        }

        private async void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            string currentImagePath = (string)currentThumbBox?.Tag;
            if (string.IsNullOrEmpty(currentImagePath)) return;

            if (MessageBox.Show($"确定要删除 {Path.GetFileName(currentImagePath)} 吗?",
                              "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes)
            {
                try
                {
                    // Delete Image
                    File.Delete(currentImagePath);

                    // Rename Images
                    RenameImages();

                    // ReLoad
                    LoadImages();

                    // Update Panel
                    UpdateImagePanel();

                    MessageBox.Show("文件已成功删除", "操作完成",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除文件时出错: {ex.Message}", "错误",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void RenameImages()
        {
            string passPath = PathManager.Instance.GetProjectPassFolder("pass" + PassID);
            if (!Directory.Exists(passPath)) return;

            var extensions = new[] { ".png" };

            // 获取所有符合条件的图片文件并按原顺序排序
            var imageFiles = Directory.GetFiles(passPath, "*.*", SearchOption.AllDirectories)
                .Where(f => {
                    string ext = Path.GetExtension(f).ToLower();
                    string fileName = Path.GetFileNameWithoutExtension(f);
                    return extensions.Contains(ext);
                })
                .OrderBy(f => f) // 按原文件名排序
                .ToList();

            // 按顺序重命名图片，保证连续性
            for (int i = 0; i < imageFiles.Count; i++)
            {
                string oldFilePath = imageFiles[i];
                string directory = Path.GetDirectoryName(oldFilePath);
                string newFileName = $"image{PassID}_{i + 1}.png";
                string newFilePath = Path.Combine(directory, newFileName);

                try
                {
                    File.Move(oldFilePath, newFilePath);
                    Console.WriteLine($"已重命名: {oldFilePath} -> {newFilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"重命名失败: {oldFilePath}, 错误: {ex.Message}");
                }
            }
        }
        private async void ThumbBox_DoubleClick(object? sender, EventArgs e)
        {
            var path = (string)((PictureBox)sender).Tag;
            await OpenImageAsync(path);
        }

        private async Task OpenImageAsync(string path)
        {
            try
            {
                // 异步加载图像（在后台线程执行）
                Image image = await Task.Run(() => Image.FromFile(path));

                // 切换到主线程显示图像
                Invoke(new Action(() =>
                {
                    AntdUI.Preview.open(new AntdUI.Preview.Config(this.ParentForm, image));
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开图片时出错: {ex.Message}");
            }
        }

        // ### Parameter Panel ###
        private void ParameterEditor_TextChanged(object sender, EventArgs e)
        {
            int currentLine = ParameterEditor.GetLineFromCharIndex(ParameterEditor.SelectionStart);
            if (currentLine >= 0)
                HighlightParameterSyntax(currentLine);
        }


        /// <summary>
        /// 高亮参数定义语法（重载版本：处理指定行）
        /// </summary>
        /// <param name="lineNumber">行号（从0开始）</param>
        private void HighlightParameterSyntax(int lineNumber)
        {
            // 保存当前位置
            int start = ParameterEditor.SelectionStart;
            int length = ParameterEditor.SelectionLength;
            Color originalColor = ParameterEditor.SelectionColor;

            // 暂停重绘
            SendMessage(ParameterEditor.Handle, WM_SETREDRAW, false, 0);

            try
            {
                if (lineNumber >= 0 && lineNumber < ParameterEditor.Lines.Length)
                {
                    int lineStart = ParameterEditor.GetFirstCharIndexFromLine(lineNumber);
                    if (lineStart == -1) return;

                    string lineText = ParameterEditor.Lines[lineNumber];
                    int lineLength = lineText.Length;

                    // 重置当前行样式
                    ParameterEditor.Select(lineStart, lineLength);
                    ParameterEditor.SelectionColor = Color.FromArgb(30, 30, 30);
                    ParameterEditor.SelectionFont = editorFont;
                    ParameterEditor.SelectionBackColor = ParameterEditor.BackColor;
                    ParameterEditor.DeselectAll();

                    // 应用参数定义高亮规则
                    HighlightParameterTypes(lineText, lineStart, Color.FromArgb(198, 120, 221));        // 类型高亮
                    HighlightParameterVariables(lineText, lineStart, Color.FromArgb(30, 30, 30));       // 变量名高亮
                    HighlightParameterOperators(lineText, lineStart, Color.FromArgb(97, 175, 239));     // 运算符高亮
                    HighlightParameterValues(lineText, lineStart, Color.FromArgb(190, 138, 89));        // 值高亮
                    HighlightParameterRanges(lineText, lineStart, Color.FromArgb(128, 0, 128));         // 范围高亮
                    HighlightParameterComments(lineText, lineStart, Color.FromArgb(144, 176, 97));      // 注释高亮
                }
            }
            finally
            {
                // 恢复位置
                ParameterEditor.Select(start, length);
                ParameterEditor.SelectionColor = originalColor;

                // 恢复重绘并刷新
                SendMessage(ParameterEditor.Handle, WM_SETREDRAW, true, 0);
                ParameterEditor.Invalidate();
            }
        }

        /// <summary>
        /// 高亮参数定义语法（重载版本：处理全文）
        /// </summary>
        /// <param name="fullDocument">是否处理全文</param>
        private void HighlightParameterSyntax(bool fullDocument = true)
        {
            // 保存当前位置
            int start = ParameterEditor.SelectionStart;
            int length = ParameterEditor.SelectionLength;
            Color originalColor = ParameterEditor.SelectionColor;

            // 暂停重绘
            SendMessage(ParameterEditor.Handle, WM_SETREDRAW, false, 0);

            try
            {
                if (fullDocument)
                {
                    // 重置所有文本为默认样式
                    ParameterEditor.SelectAll();
                    ParameterEditor.SelectionColor = Color.FromArgb(30, 30, 30);
                    ParameterEditor.SelectionFont = editorFont;
                    ParameterEditor.SelectionBackColor = ParameterEditor.BackColor;
                    ParameterEditor.DeselectAll();

                    // 处理全文
                    for (int i = 0; i < ParameterEditor.Lines.Length; i++)
                    {
                        HighlightParameterSyntax(i);
                    }
                }
            }
            finally
            {
                // 恢复位置
                ParameterEditor.Select(start, length);
                ParameterEditor.SelectionColor = originalColor;

                // 恢复重绘并刷新
                SendMessage(ParameterEditor.Handle, WM_SETREDRAW, true, 0);
                ParameterEditor.Invalidate();
            }
        }


        // 高亮参数类型（float, bool, enum等）
        private void HighlightParameterTypes(string lineText, int lineStart, Color color)
        {
            string[] types = new[] { "float", "float2", "float3", "float4", "int", "int2", "int3", "int4",
                            "uint", "uint2", "uint3", "uint4", "bool", "color", "enum" };
            string pattern = @"\b(" + string.Join("|", types.Select(Regex.Escape)) + @")\b";
            MatchCollection matches = Regex.Matches(lineText, pattern);

            foreach (Match match in matches)
            {
                ParameterEditor.Select(lineStart + match.Index, match.Length);
                ParameterEditor.SelectionColor = color;
            }
        }

        // 高亮参数变量名
        private void HighlightParameterVariables(string lineText, int lineStart, Color color)
        {
            // 匹配变量名（类型后面的标识符）
            string pattern = @"(?<=\b(float|float2|float3|float4|int|int2|int3|int4|uint|uint2|uint3|uint4|bool|color|enum)\s+)[a-zA-Z_][a-zA-Z0-9_]*";
            MatchCollection matches = Regex.Matches(lineText, pattern);

            foreach (Match match in matches)
            {
                ParameterEditor.Select(lineStart + match.Index, match.Length);
                ParameterEditor.SelectionColor = color;
            }
        }

        // 高亮参数运算符和括号
        private void HighlightParameterOperators(string lineText, int lineStart, Color color)
        {
            string pattern = @"[=()\[\],.]";
            MatchCollection matches = Regex.Matches(lineText, pattern);

            foreach (Match match in matches)
            {
                ParameterEditor.Select(lineStart + match.Index, match.Length);
                ParameterEditor.SelectionColor = color;
            }
        }

        // 高亮参数值
        private void HighlightParameterValues(string lineText, int lineStart, Color color)
        {
            // 匹配数值、布尔值、颜色值和枚举值
            string pattern = @"(?<==\s*)((\b(true|false)\b)|(\bMAT_[A-Z_]+\b)|(\(\s*((\d+\.\d+|\d+)(\s*,\s*(\d+\.\d+|\d+))*)\s*\))|(\d+\.\d+|\d+))";
            MatchCollection matches = Regex.Matches(lineText, pattern);

            foreach (Match match in matches)
            {
                ParameterEditor.Select(lineStart + match.Index, match.Length);
                ParameterEditor.SelectionColor = color;
            }
        }

        // 高亮参数范围
        private void HighlightParameterRanges(string lineText, int lineStart, Color color)
        {
            // 匹配方括号内的范围定义
            string pattern = @"\[[^\]]*\]";
            MatchCollection matches = Regex.Matches(lineText, pattern);

            foreach (Match match in matches)
            {
                ParameterEditor.Select(lineStart + match.Index, match.Length);
                ParameterEditor.SelectionColor = color;
            }
        }

        // 高亮参数注释
        private void HighlightParameterComments(string lineText, int lineStart, Color color)
        {
            // 匹配#后的注释内容
            string pattern = @"#.*";
            MatchCollection matches = Regex.Matches(lineText, pattern);

            foreach (Match match in matches)
            {
                ParameterEditor.Select(lineStart + match.Index, match.Length);
                ParameterEditor.SelectionColor = color;
            }
        }



        // ### 将参数区代码转化为EffectParameter ###
        private List<EffectParameter> ProcessParameters()
        {
            List<EffectParameter> parameters = new List<EffectParameter>();

            foreach (var passPair in passParameters)
            {
                int passId = passPair.Key;
                string parameterCode = passPair.Value;

                string[] lines = parameterCode.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                        continue;

                    try
                    {
                        EffectParameter param = ParseParameterLine(passId, trimmedLine);
                        if (param != null)
                            parameters.Add(param);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"解析参数行失败: {trimmedLine}, 错误: {ex.Message}");
                    }
                }
            }

            return parameters;
        }

        private EffectParameter ParseParameterLine(int passId, string line)
        {
            string[] commentParts = line.Split(new[] { '#' }, 2);
            string paramPart = commentParts[0].Trim();
            string description = commentParts.Length > 1 ? commentParts[1].Trim() : "";

            string[] equalParts = paramPart.Split(new[] { '=' }, 2);
            if (equalParts.Length != 2)
                return null;

            string declaration = equalParts[0].Trim();
            string valuePart = equalParts[1].Trim();

            // 处理enum类型的特殊语法
            string type;
            string name;

            if (declaration.StartsWith("enum "))
            {
                // 提取enum类型名和参数名
                // 例如: "enum MaterialType { ... } materialType"
                int enumEndIndex = declaration.IndexOf('}');
                if (enumEndIndex == -1)
                    return null;

                string enumPart = declaration.Substring(0, enumEndIndex + 1);
                string namePart = declaration.Substring(enumEndIndex + 1).Trim();

                type = enumPart;
                name = namePart;
            }
            else
            {
                // 普通类型
                string[] typeNameParts = declaration.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (typeNameParts.Length != 2)
                    return null;

                type = typeNameParts[0].Trim();
                name = typeNameParts[1].Trim();
            }

            string defaultValueStr = valuePart;
            string minValueStr = null;
            string maxValueStr = null;

            int bracketStart = valuePart.IndexOf('[');
            int bracketEnd = valuePart.IndexOf(']');
            if (bracketStart >= 0 && bracketEnd > bracketStart)
            {
                defaultValueStr = valuePart.Substring(0, bracketStart).Trim();
                string rangeStr = valuePart.Substring(bracketStart + 1, bracketEnd - bracketStart - 1).Trim();
                string[] rangeParts = rangeStr.Split(new[] { ',' }, 2);

                if (rangeParts.Length == 2)
                {
                    minValueStr = rangeParts[0].Trim();
                    maxValueStr = rangeParts[1].Trim();
                }
            }

            EffectParameter param = new EffectParameter
            {
                PassId = passId,
                Name = name,
                Description = description
            };

            try
            {
                if (type.StartsWith("enum "))
                {
                    // 提取枚举类型定义
                    int enumStart = type.IndexOf('{');
                    int enumEnd = type.IndexOf('}');
                    if (enumStart > 0 && enumEnd > enumStart)
                    {
                        string enumTypeName = type.Substring(5, enumStart - 5).Trim(); // "enum " 后面的类型名
                        string enumValuesStr = type.Substring(enumStart + 1, enumEnd - enumStart - 1).Trim();
                        string[] enumValues = enumValuesStr.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        // 验证默认值是否在枚举值列表中
                        if (!enumValues.Contains(defaultValueStr))
                        {
                            Console.WriteLine($"警告: 枚举参数 '{name}' 的默认值 '{defaultValueStr}' 不在定义的枚举值列表中");
                        }

                        // 设置类型为枚举值列表
                        param.Type = $"{{{enumValuesStr}}}";
                        param.DefaultValue = defaultValueStr;
                        param.MinValue = enumValues;
                        param.MaxValue = enumValues;
                    }
                }
                else
                {
                    // 处理color类型为float4
                    if (type.Equals("color", StringComparison.OrdinalIgnoreCase))
                    {
                        type = "float4";
                        param.Type = type;
                    }
                    else
                    {
                        param.Type = type;
                    }

                    param.DefaultValue = ParseValueByType(defaultValueStr, type);
                    param.MinValue = minValueStr != null ? ParseValueByType(minValueStr, type) : GetDefaultMinValue(type);
                    param.MaxValue = maxValueStr != null ? ParseValueByType(maxValueStr, type) : GetDefaultMaxValue(type);

                    if (type.Equals("bool", StringComparison.OrdinalIgnoreCase))
                    {
                        param.Type = "int";
                        param.DefaultValue = Convert.ToBoolean(param.DefaultValue) ? 1 : 0;
                        param.MinValue = 0;
                        param.MaxValue = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析参数值失败: {line}, 错误: {ex.Message}");
                return null;
            }

            return param;
        }

        private object ParseValueByType(string valueStr, string type)
        {
            valueStr = valueStr.Trim('(', ')'); // 移除括号

            // 处理color类型为float4
            if (type.Equals("color", StringComparison.OrdinalIgnoreCase))
            {
                type = "float4";
            }

            if (type.Contains("float"))
            {
                if (type.EndsWith("2"))
                    return ParseFloatArray(valueStr, 2);
                else if (type.EndsWith("3"))
                    return ParseFloatArray(valueStr, 3);
                else if (type.EndsWith("4"))
                    return ParseFloatArray(valueStr, 4);
                else
                    return float.Parse(valueStr);
            }
            else if (type.Contains("int"))
            {
                if (type.EndsWith("2"))
                    return ParseIntArray(valueStr, 2);
                else if (type.EndsWith("3"))
                    return ParseIntArray(valueStr, 3);
                else if (type.EndsWith("4"))
                    return ParseIntArray(valueStr, 4);
                else
                    return int.Parse(valueStr);
            }
            else if (type.Contains("uint"))
            {
                if (type.EndsWith("2"))
                    return ParseUIntArray(valueStr, 2);
                else if (type.EndsWith("3"))
                    return ParseUIntArray(valueStr, 3);
                else if (type.EndsWith("4"))
                    return ParseUIntArray(valueStr, 4);
                else
                    return uint.Parse(valueStr);
            }
            else if (type.Equals("bool", StringComparison.OrdinalIgnoreCase))
            {
                return bool.Parse(valueStr);
            }

            return valueStr;
        }

        private float[] ParseFloatArray(string valueStr, int length)
        {
            string[] parts = valueStr.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float[] result = new float[length];

            for (int i = 0; i < Math.Min(parts.Length, length); i++)
            {
                result[i] = float.Parse(parts[i]);
            }

            return result;
        }

        private int[] ParseIntArray(string valueStr, int length)
        {
            string[] parts = valueStr.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int[] result = new int[length];

            for (int i = 0; i < Math.Min(parts.Length, length); i++)
            {
                result[i] = int.Parse(parts[i]);
            }

            return result;
        }

        private uint[] ParseUIntArray(string valueStr, int length)
        {
            string[] parts = valueStr.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            uint[] result = new uint[length];

            for (int i = 0; i < Math.Min(parts.Length, length); i++)
            {
                result[i] = uint.Parse(parts[i]);
            }

            return result;
        }

        private object GetDefaultMinValue(string type)
        {
            if (type.Contains("float") || type.Contains("color"))
                return 0.0f;
            else if (type.Contains("int"))
                return 0;
            else if (type.Contains("uint"))
                return 0u;
            else if (type.Equals("bool", StringComparison.OrdinalIgnoreCase))
                return 0;

            return null;
        }

        private object GetDefaultMaxValue(string type)
        {
            if (type.Contains("float") || type.Contains("color"))
                return 1.0f;
            else if (type.Contains("int"))
                return 1;
            else if (type.Contains("uint"))
                return 1u;
            else if (type.Equals("bool", StringComparison.OrdinalIgnoreCase))
                return 1;

            return null;
        }



        // Document Button
        private void DocumentButton_Click(object sender, EventArgs e)
        {

        }
    }
}

/*
float exposure = 1.0 [0.1, 5.0] # 曝光强度调节
float3 lightPos = (10.0, 5.0, 8.0) [-100, 100] # 光源位置坐标
bool enableShadow = true [false, true] # 是否启用阴影效果
color mainColor = (0.7, 0.2, 0.1, 1.0) [0.0, 1.0] # 主色调（RGBA）
int qualityLevel = 2 [1, 5] # 渲染质量等级
enum MaterialType { MAT_LAMBERT, MAT_PHONG, MAT_BLINN_PHONG, MAT_PBR } materialType = MAT_PBR  # 材质类型
 */