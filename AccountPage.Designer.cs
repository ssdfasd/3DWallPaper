using System.Windows.Forms;

namespace XCWallPaper
{
    partial class AccountPage
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
            LoginPanel = new AntdUI.Panel();
            LoginMailboxLabel = new AntdUI.Label();
            LoginMailboxInput = new AntdUI.Input();
            LoginPasswardLabel = new AntdUI.Label();
            LoginPasswardInput = new AntdUI.Input();
            RememberLabel = new AntdUI.Label();
            RememberCheckBox = new AntdUI.Checkbox();
            ForgetButton = new AntdUI.Button();
            LoginButton = new AntdUI.Button();
            LoginRegisterButton = new AntdUI.Button();

            RegisterPanel = new AntdUI.Panel();
            RegisterMailboxLabel = new AntdUI.Label();
            RegisterMailboxInput = new AntdUI.Input();
            RegisterICLabel = new AntdUI.Label();
            RegisterICPanel = new AntdUI.Panel();
            RegisterIC1 = new AntdUI.Input();
            RegisterIC2 = new AntdUI.Input();
            RegisterIC3 = new AntdUI.Input();
            RegisterIC4 = new AntdUI.Input();
            RegisterIC5 = new AntdUI.Input();
            RegisterIC6 = new AntdUI.Input();
            RegisterICButton = new AntdUI.Button();
            RegisterPasswardLabel = new AntdUI.Label();
            RegisterPasswardInput = new AntdUI.Input();
            RegisterRePasswardLabel = new AntdUI.Label();
            RegisterRePasswardInput = new AntdUI.Input();
            RegisterRegisterButton = new AntdUI.Button();

            ForgetPanel = new AntdUI.Panel();
            ForgetMailboxLabel = new AntdUI.Label();
            ForgetMailboxInput = new AntdUI.Input();
            ForgetICLabel = new AntdUI.Label();
            ForgetICPanel = new AntdUI.Panel();
            ForgetIC1 = new AntdUI.Input();
            ForgetIC2 = new AntdUI.Input();
            ForgetIC3 = new AntdUI.Input();
            ForgetIC4 = new AntdUI.Input();
            ForgetIC5 = new AntdUI.Input();
            ForgetIC6 = new AntdUI.Input();
            ForgetICButton = new AntdUI.Button();
            ForgetPasswardLabel = new AntdUI.Label();
            ForgetPasswardInput = new AntdUI.Input();
            ForgetRePasswardLabel = new AntdUI.Label();
            ForgetRePasswardInput = new AntdUI.Input();
            ForgetRegisterButton = new AntdUI.Button();
            SuspendLayout();
            //
            // Login Panel
            //
            LoginPanel.Size = new Size(400, 900);
            LoginPanel.Location = new Point(0, 0);
            LoginPanel.Back = Color.FromArgb(240, 240, 240);
            Controls.Add(RegisterPanel);////////////////////////
            //
            // Login Panel
            //
            RegisterPanel.Size = new Size(400, 900);
            RegisterPanel.Location = new Point(0, 0);
            RegisterPanel.Back = Color.FromArgb(240, 240, 240);
            //
            // Forget Panel
            //
            ForgetPanel.Size = new Size(400, 900);
            ForgetPanel.Location = new Point(0, 0);
            ForgetPanel.Back = Color.FromArgb(240, 240, 240);
            //
            // Login Mailbox Label
            //
            LoginMailboxLabel = new AntdUI.Label();
            LoginMailboxLabel.Size = new Size(100, 40);
            LoginMailboxLabel.Location = new Point(40, 240);
            LoginMailboxLabel.BackColor = Color.Transparent;
            LoginMailboxLabel.Text = "邮箱";
            LoginMailboxLabel.TextAlign = ContentAlignment.MiddleLeft;
            LoginMailboxLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            LoginMailboxLabel.ForeColor = Color.FromArgb(100, 100, 100);
            //
            // Login Mailbox Input 
            //
            LoginMailboxInput.Location = new Point(30, 280);
            LoginMailboxInput.Size = new Size(320, 60);
            LoginMailboxInput.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            LoginMailboxInput.PlaceholderText = "your@emil.com";
            LoginPanel.Controls.Add(LoginMailboxInput);
            LoginPanel.Controls.Add(LoginMailboxLabel);
            //
            // Login Passward Label
            //
            LoginPasswardLabel = new AntdUI.Label();
            LoginPasswardLabel.Size = new Size(100, 40);
            LoginPasswardLabel.Location = new Point(40, 350);
            LoginPasswardLabel.BackColor = Color.Transparent;
            LoginPasswardLabel.Text = "密码";
            LoginPasswardLabel.TextAlign = ContentAlignment.MiddleLeft;
            LoginPasswardLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            LoginPasswardLabel.ForeColor = Color.FromArgb(100, 100, 100);
            //
            // Login Mailbox Input 
            //
            LoginPasswardInput.Location = new Point(30, 390);
            LoginPasswardInput.Size = new Size(320, 60);
            LoginPasswardInput.PasswordChar = '·';
            LoginPasswardInput.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            LoginPasswardInput.PlaceholderText = "········";
            LoginPanel.Controls.Add(LoginPasswardLabel);
            LoginPanel.Controls.Add(LoginPasswardInput);
            //
            // Remember Label
            //
            RememberLabel = new AntdUI.Label();
            RememberLabel.Size = new Size(100, 40);
            RememberLabel.Location = new Point(70, 460);
            RememberLabel.BackColor = Color.Transparent;
            RememberLabel.Text = "记住我";
            RememberLabel.TextAlign = ContentAlignment.MiddleLeft;
            RememberLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            RememberLabel.ForeColor = Color.FromArgb(100, 100, 100);
            LoginPanel.Controls.Add(RememberLabel);
            //
            // Remember CheckBox
            //
            RememberCheckBox.Size = new Size(40, 40);
            RememberCheckBox.Location = new Point(30, 460);
            RememberCheckBox.Fill = Color.FromArgb(100, 100, 100);
            RememberCheckBox.BackColor = Color.Transparent;
            LoginPanel.Controls.Add(RememberCheckBox);
            //
            // Forget Button
            //
            ForgetButton.Text = "忘记密码？";
            ForgetButton.TextAlign = ContentAlignment.MiddleLeft;
            ForgetButton.DefaultBack = Color.Transparent;
            ForgetButton.Size = new Size(100, 40);
            ForgetButton.Location = new Point(250, 460);
            ForgetButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            ForgetButton.ForeColor = Color.FromArgb(100, 100, 100);
            LoginPanel.Controls.Add(ForgetButton);
            //
            // Login Button
            //
            LoginButton.Text = "登录";
            LoginButton.TextAlign = ContentAlignment.MiddleCenter;
            LoginButton.Size = new Size(320, 60);
            LoginButton.Location = new Point(30, 510);
            LoginButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            LoginButton.ForeColor = Color.FromArgb(100, 100, 100);
            LoginButton.DefaultBack = Color.FromArgb(200, 200, 200);
            LoginPanel.Controls.Add(LoginButton);
            //
            // Register Button
            //
            LoginRegisterButton.Text = "注册";
            LoginRegisterButton.TextAlign = ContentAlignment.MiddleCenter;
            LoginRegisterButton.Size = new Size(320, 60);
            LoginRegisterButton.Location = new Point(30, 580);
            LoginRegisterButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            LoginRegisterButton.ForeColor = Color.FromArgb(100, 100, 100);
            LoginRegisterButton.DefaultBack = Color.FromArgb(200, 200, 200);
            LoginPanel.Controls.Add(LoginRegisterButton);
            //
            // Register Mailbox Label
            //
            RegisterMailboxLabel = new AntdUI.Label();
            RegisterMailboxLabel.Size = new Size(100, 40);
            RegisterMailboxLabel.Location = new Point(40, 200);
            RegisterMailboxLabel.BackColor = Color.Transparent;
            RegisterMailboxLabel.Text = "邮箱";
            RegisterMailboxLabel.TextAlign = ContentAlignment.MiddleLeft;
            RegisterMailboxLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            RegisterMailboxLabel.ForeColor = Color.FromArgb(100, 100, 100);
            //
            // Register Mailbox Input 
            //
            RegisterMailboxInput.Location = new Point(30, 240);
            RegisterMailboxInput.Size = new Size(320, 60);
            RegisterMailboxInput.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            RegisterMailboxInput.PlaceholderText = "your@emil.com";
            RegisterPanel.Controls.Add(RegisterMailboxInput);
            RegisterPanel.Controls.Add(RegisterMailboxLabel);
            //
            // Register IC Label
            //
            RegisterICLabel = new AntdUI.Label();
            RegisterICLabel.Size = new Size(100, 40);
            RegisterICLabel.Location = new Point(40, 300);
            RegisterICLabel.BackColor = Color.Transparent;
            RegisterICLabel.Text = "验证码";
            RegisterICLabel.TextAlign = ContentAlignment.MiddleLeft;
            RegisterICLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            RegisterICLabel.ForeColor = Color.FromArgb(100, 100, 100);
            RegisterPanel.Controls.Add(RegisterICLabel);
            //
            // Register IC Panel
            //
            RegisterICPanel.Size = new Size(320, 60);
            RegisterICPanel.Location = new Point(30, 340);
            RegisterICPanel.BackColor = Color.Transparent;
            RegisterICPanel.Back = Color.Transparent;
            RegisterPanel.Controls.Add(RegisterICPanel);
            RegisterICPanel.Controls.Add(RegisterIC1);
            RegisterICPanel.Controls.Add(RegisterIC2);
            RegisterICPanel.Controls.Add(RegisterIC3);
            RegisterICPanel.Controls.Add(RegisterIC4);
            RegisterICPanel.Controls.Add(RegisterIC5);
            RegisterICPanel.Controls.Add(RegisterIC6);
            RegisterICPanel.Visible = false;
            //
            // Register IC Button
            //
            RegisterICButton.Text = "发送";
            RegisterICButton.TextAlign = ContentAlignment.MiddleCenter;
            RegisterICButton.Size = new Size(320, 60);
            RegisterICButton.Location = new Point(30, 340);
            RegisterICButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            RegisterICButton.ForeColor = Color.FromArgb(100, 100, 100);
            RegisterICButton.DefaultBack = Color.FromArgb(200, 200, 200);
            RegisterPanel.Controls.Add(RegisterICButton);
            //
            // Register IC 1
            //
            RegisterIC1.Size = new Size(50, 60);
            RegisterIC1.Location = new Point(0, 0);
            RegisterIC1.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            //
            // Register IC 2
            //
            RegisterIC2.Size = new Size(50, 60);
            RegisterIC2.Location = new Point(54, 0);
            RegisterIC2.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            //
            // Register IC 3
            //
            RegisterIC3.Size = new Size(50, 60);
            RegisterIC3.Location = new Point(108, 0);
            RegisterIC3.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            //
            // Register IC 4
            //
            RegisterIC4.Size = new Size(50, 60);
            RegisterIC4.Location = new Point(162, 0);
            RegisterIC4.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            //
            // Register IC 5
            //
            RegisterIC5.Size = new Size(50, 60);
            RegisterIC5.Location = new Point(216, 0);
            RegisterIC5.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            //
            // Register IC 6
            //
            RegisterIC6.Size = new Size(50, 60);
            RegisterIC6.Location = new Point(270, 0);
            RegisterIC6.Font = new Font("Segoe UI", 14, FontStyle.Bold);

            //
            // Register Passward Label
            //
            RegisterPasswardLabel = new AntdUI.Label();
            RegisterPasswardLabel.Size = new Size(100, 40);
            RegisterPasswardLabel.Location = new Point(40, 400);
            RegisterPasswardLabel.BackColor = Color.Transparent;
            RegisterPasswardLabel.Text = "密码";
            RegisterPasswardLabel.TextAlign = ContentAlignment.MiddleLeft;
            RegisterPasswardLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            RegisterPasswardLabel.ForeColor = Color.FromArgb(100, 100, 100);
            //
            // Register Mailbox Input 
            //
            RegisterPasswardInput.Location = new Point(30, 440);
            RegisterPasswardInput.Size = new Size(320, 60);
            RegisterPasswardInput.PasswordChar = '·';
            RegisterPasswardInput.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            RegisterPasswardInput.PlaceholderText = "········";
            RegisterPanel.Controls.Add(RegisterPasswardLabel);
            RegisterPanel.Controls.Add(RegisterPasswardInput);
            //
            // ReRegister Passward Label
            //
            RegisterRePasswardLabel = new AntdUI.Label();
            RegisterRePasswardLabel.Size = new Size(100, 40);
            RegisterRePasswardLabel.Location = new Point(40, 500);
            RegisterRePasswardLabel.BackColor = Color.Transparent;
            RegisterRePasswardLabel.Text = "确认密码";
            RegisterRePasswardLabel.TextAlign = ContentAlignment.MiddleLeft;
            RegisterRePasswardLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            RegisterRePasswardLabel.ForeColor = Color.FromArgb(100, 100, 100);
            //
            // ReRegister Mailbox Input 
            //
            RegisterRePasswardInput.Location = new Point(30, 540);
            RegisterRePasswardInput.Size = new Size(320, 60);
            RegisterRePasswardInput.PasswordChar = '·';
            RegisterRePasswardInput.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            RegisterRePasswardInput.PlaceholderText = "········";
            RegisterPanel.Controls.Add(RegisterRePasswardLabel);
            RegisterPanel.Controls.Add(RegisterRePasswardInput);
            //
            // Register Button
            //
            RegisterRegisterButton.Text = "注册";
            RegisterRegisterButton.TextAlign = ContentAlignment.MiddleCenter;
            RegisterRegisterButton.Size = new Size(320, 60);
            RegisterRegisterButton.Location = new Point(30, 610);
            RegisterRegisterButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            RegisterRegisterButton.ForeColor = Color.FromArgb(100, 100, 100);
            RegisterRegisterButton.DefaultBack = Color.FromArgb(200, 200, 200);
            RegisterPanel.Controls.Add(RegisterRegisterButton);
            //
            // LoginPage
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Margin = new Padding(0);
            Name = "LoginPage";
            Size = new Size(400, 900);
            BackColor = Color.FromArgb(220, 220, 220);
            UITool.Instance.ArcRegion(this, 20);
            ResumeLayout(false);
        }

        // # Login Panel #
        private AntdUI.Panel LoginPanel;
        private AntdUI.Label LoginMailboxLabel;
        private AntdUI.Input LoginMailboxInput;
        private AntdUI.Label LoginPasswardLabel;
        private AntdUI.Input LoginPasswardInput;
        private AntdUI.Label RememberLabel;
        private AntdUI.Checkbox RememberCheckBox;
        private AntdUI.Button ForgetButton;

        private AntdUI.Button LoginButton;
        private AntdUI.Button LoginRegisterButton;

        // # Register Panel #
        private AntdUI.Panel RegisterPanel;
        private AntdUI.Label RegisterMailboxLabel;
        private AntdUI.Input RegisterMailboxInput;
        private AntdUI.Label RegisterICLabel;
        private AntdUI.Panel RegisterICPanel;
        private AntdUI.Input RegisterIC1;
        private AntdUI.Input RegisterIC2;
        private AntdUI.Input RegisterIC3;
        private AntdUI.Input RegisterIC4;
        private AntdUI.Input RegisterIC5;
        private AntdUI.Input RegisterIC6;
        private AntdUI.Button RegisterICButton;
        private AntdUI.Label RegisterPasswardLabel;
        private AntdUI.Input RegisterPasswardInput;
        private AntdUI.Label RegisterRePasswardLabel;
        private AntdUI.Input RegisterRePasswardInput;
        private AntdUI.Button RegisterRegisterButton;

        // # Forget Panel #
        private AntdUI.Panel ForgetPanel;
        private AntdUI.Label ForgetMailboxLabel;
        private AntdUI.Input ForgetMailboxInput;
        private AntdUI.Label ForgetICLabel;
        private AntdUI.Panel ForgetICPanel;
        private AntdUI.Input ForgetIC1;
        private AntdUI.Input ForgetIC2;
        private AntdUI.Input ForgetIC3;
        private AntdUI.Input ForgetIC4;
        private AntdUI.Input ForgetIC5;
        private AntdUI.Input ForgetIC6;
        private AntdUI.Button ForgetICButton;
        private AntdUI.Label ForgetPasswardLabel;
        private AntdUI.Input ForgetPasswardInput;
        private AntdUI.Label ForgetRePasswardLabel;
        private AntdUI.Input ForgetRePasswardInput;
        private AntdUI.Button ForgetRegisterButton;
        #endregion
    }
}
