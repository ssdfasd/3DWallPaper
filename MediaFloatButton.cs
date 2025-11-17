using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AntdUI;

namespace XCWallPaper
{
    public class MediaFloatButton
    {
        private Form parentForm;
        private AntdUI.FormFloatButton floatButton;
        private System.Timers.Timer floatButtonTimer;
        private List<AntdUI.FloatButton.ConfigBtn> buttonsConfig;
        private Action<string> onButtonClick;
        private int autoCloseDelay = 5000; // 默认5秒后自动关闭

        // 用于存储平台名称和对应URL的字典
        private Dictionary<string, string> platformUrls = new Dictionary<string, string>
        {
            { "Bili", "https://www.bilibili.com" },
            { "TikTok", "https://www.tiktok.com" },
            { "X", "https://twitter.com" },
            { "Google", "https://www.google.com" },
            { "QQ", "https://im.qq.com" },
            { "WeChat", "https://weixin.qq.com" }
        };

        public MediaFloatButton(Form parent)
        {
            this.parentForm = parent;
            InitializeDefaultButtons();
        }

        // 初始化默认的浮动按钮配置
        private void InitializeDefaultButtons()
        {
            buttonsConfig = new List<AntdUI.FloatButton.ConfigBtn>
            {
                new AntdUI.FloatButton.ConfigBtn("Bili", "BilibiliOutlined", true)
                {
                    IconSize = new Size(40, 40)
                },
                new AntdUI.FloatButton.ConfigBtn("TikTok", "TikTokOutlined", true)
                {
                    IconSize = new Size(40, 40)
                },
                new AntdUI.FloatButton.ConfigBtn("X", "XOutlined", true)
                {
                    IconSize = new Size(40, 40)
                },
                new AntdUI.FloatButton.ConfigBtn("Google", "GoogleOutlined", true)
                {
                    IconSize = new Size(40, 40)
                },
                new AntdUI.FloatButton.ConfigBtn("QQ", "QqOutlined", true)
                {
                    IconSize = new Size(40, 40)
                },
                new AntdUI.FloatButton.ConfigBtn("WeChat", "WechatOutlined", true)
                {
                    IconSize = new Size(40, 40)
                }
            };

            // 设置默认的按钮点击处理
            onButtonClick = (buttonName) =>
            {
                if (platformUrls.TryGetValue(buttonName, out string url))
                {
                    OpenUrlInBrowser(url);
                }
            };
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

        // 创建配置的深拷贝
        private List<AntdUI.FloatButton.ConfigBtn> CloneButtonsConfig()
        {
            var clone = new List<AntdUI.FloatButton.ConfigBtn>();

            foreach (var btn in buttonsConfig)
            {
                clone.Add(new AntdUI.FloatButton.ConfigBtn(btn.Name, btn.IconSvg, true)
                {
                    IconSize = btn.IconSize
                });
            }

            return clone;
        }

        // 显示浮动按钮
        public void ShowFloatButtons()
        {
            if (floatButton != null)
            {
                return; // 已经显示，不重复创建
            }

            // 创建浮动按钮
            floatButton = AntdUI.FloatButton.open(new AntdUI.FloatButton.Config(this.parentForm,
                CloneButtonsConfig().ToArray(),
                btn => onButtonClick?.Invoke(btn.Name)));

            // 启动自动关闭定时器
            StartAutoCloseTimer();
        }

        // 隐藏浮动按钮
        public void HideFloatButtons()
        {
            if (floatButton == null)
            {
                return; // 已经隐藏，无需操作
            }

            floatButton.Close();
            floatButton = null;

            // 停止定时器
            StopAutoCloseTimer();
        }

        // 启动自动关闭定时器
        private void StartAutoCloseTimer()
        {
            if (floatButtonTimer != null)
            {
                floatButtonTimer.Stop();
                floatButtonTimer.Dispose();
            }

            floatButtonTimer = new System.Timers.Timer(autoCloseDelay);
            floatButtonTimer.Elapsed += (s, args) =>
            {
                // 确保在 UI 线程上执行关闭操作
                parentForm.Invoke(new Action(() =>
                {
                    HideFloatButtons();
                }));
            };
            floatButtonTimer.Start();
        }

        // 停止自动关闭定时器
        private void StopAutoCloseTimer()
        {
            if (floatButtonTimer != null)
            {
                floatButtonTimer.Stop();
                floatButtonTimer.Dispose();
                floatButtonTimer = null;
            }
        }

        // 设置自动关闭延迟时间（毫秒）
        public void SetAutoCloseDelay(int milliseconds)
        {
            autoCloseDelay = milliseconds;
        }

        // 添加自定义按钮
        public void AddCustomButton(string name, string icon, bool visible, Size iconSize)
        {
            buttonsConfig.Add(new AntdUI.FloatButton.ConfigBtn(name, icon, visible)
            {
                IconSize = iconSize
            });
        }

        // 设置自定义按钮点击处理函数
        public void SetButtonClickHandler(Action<string> handler)
        {
            onButtonClick = handler;
        }

        // 设置自定义平台URL
        public void SetPlatformUrl(string platformName, string url)
        {
            if (platformUrls.ContainsKey(platformName))
            {
                platformUrls[platformName] = url;
            }
            else
            {
                platformUrls.Add(platformName, url);
            }
        }

        // 检查浮动按钮是否正在显示
        public bool IsVisible()
        {
            return floatButton != null;
        }

        // 切换浮动按钮的显示/隐藏状态
        public void ToggleVisibility()
        {
            if (IsVisible())
            {
                HideFloatButtons();
            }
            else
            {
                ShowFloatButtons();
            }
        }
    }
}