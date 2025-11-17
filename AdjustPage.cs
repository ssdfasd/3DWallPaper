using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace XCWallPaper
{
    public partial class AdjustPage : UserControl
    {
        public AdjustPage()
        {
            DoubleBuffered = true;
            InitializeComponent();
            InitializeSystemAdjustUIs();
        }

        // 初始化系统调控组件
        private void InitializeSystemAdjustUIs()
        {
            #region 一、显示设置
            AdjustUI.Instance.AddCategory("一、显示设置");

            // 帧率设置（ComboBox）
            AdjustUI.Instance.AddColorPicker(
                "FrameRate",
                "帧率",
                Color.Red,
                value =>
                {

                }
            );

            // 帧率设置（ComboBox）
            AdjustUI.Instance.AddComboBox(
                "FrameRate",
                "帧率",
                new[] { "30", "60", "90", "120" },
                PathManager.Instance.GetSettinggs().FrameRate.ToString(),
                value =>
                {
                    Console.WriteLine($"帧率变更为: {value}fps");
                    int frameRate = int.Parse(value);
                    PathManager.Instance.GetSettinggs().FrameRate = frameRate;
                    IPCSender.Instance.SendMessageAsync(new DisplaySettingsMessage { FrameRate = frameRate });
                }
            );

            // 分辨率（ComboBox）
            // 获取并添加用户的全屏尺寸
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            string fullscreenResolution = $"{screenBounds.Width}*{screenBounds.Height}";
            string[] resolutions = { "2560*1440", "1920*1080", "1600*900" };
            bool resolutionExists = false;

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i] == fullscreenResolution)
                {
                    resolutionExists = true;
                    break;
                }
            }
            if (!resolutionExists)
            {
                List<string> resolutionList = new List<string>(resolutions);
                resolutionList.Insert(0, fullscreenResolution);
                resolutions = resolutionList.ToArray();
            }

            AdjustUI.Instance.AddComboBox(
                "Resolution",
                "分辨率",
                resolutions,
                "1920*1080",
                value =>
                {
                    if (string.IsNullOrEmpty(value)) return;

                    string[] parts = value.Split('*');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int width) &&
                        int.TryParse(parts[1], out int height))
                    {
                        Vector2 resolution = new Vector2(width, height);
                        PathManager.Instance.GetSettinggs().ResolutionWidth = width;
                        PathManager.Instance.GetSettinggs().ResolutionHeight = height;
                        IPCSender.Instance.SendMessageAsync(new DisplaySettingsMessage { ResolutionWidth = width, ResolutionHeight = height });
                    }
                }
            );

            // 边缘柔和（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "EdgeSoftEnabled",
                "边缘柔和",
                PathManager.Instance.GetSettinggs().EdgeSoftEnabled,
                isChecked =>
                {
                    Console.WriteLine($"边缘柔和: {isChecked}");
                    PathManager.Instance.GetSettinggs().EdgeSoftEnabled = isChecked;
                    IPCSender.Instance.SendMessageAsync(new DisplaySettingsMessage { EdgeSoftEnabled = isChecked });
                }
            );

            // 描边（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "EdgeHighlight",
                "描边",
                PathManager.Instance.GetSettinggs().EdgeHighlight,
                isChecked =>
                {
                    Console.WriteLine($"描边: {isChecked}");
                    PathManager.Instance.GetSettinggs().EdgeHighlight = isChecked;
                    IPCSender.Instance.SendMessageAsync(new DisplaySettingsMessage { EdgeHighlight = isChecked });
                }
            );

            // 泛光（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "Bloom",
                "泛光",
                PathManager.Instance.GetSettinggs().Bloom,
                isChecked =>
                {
                    Console.WriteLine($"泛光: {isChecked}");
                    PathManager.Instance.GetSettinggs().Bloom = isChecked;
                    IPCSender.Instance.SendMessageAsync(new DisplaySettingsMessage { Bloom = isChecked });
                }
            );

            // 模糊效果（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "EnableBlur",
                "模糊",
                PathManager.Instance.GetSettinggs().BlurEffectEnabled,
                isChecked =>
                {
                    Console.WriteLine($"模糊效果: {isChecked}");
                    PathManager.Instance.GetSettinggs().BlurEffectEnabled = isChecked;
                    IPCSender.Instance.SendMessageAsync(new DisplaySettingsMessage { BlurEffectEnabled = isChecked });
                }
            );

            // 模糊强度调节（Slider）
            AdjustUI.Instance.AddSlider(
                "BlurIntensity",
                "模糊强度调节",
                0,
                1,
                PathManager.Instance.GetSettinggs().BlurIntensity,
                value =>
                {
                    Console.WriteLine($"模糊强度变更为: {value}");
                    PathManager.Instance.GetSettinggs().BlurIntensity = value;
                    IPCSender.Instance.SendMessageAsync(new DisplaySettingsMessage { BlurIntensity = value });
                }
            );
            #endregion

            #region 二、系统设置
            AdjustUI.Instance.AddCategory("二、系统设置");

            // 覆盖自动暂停（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "OverAutoPause",
                "覆盖自动暂停",
                PathManager.Instance.GetSettinggs().OverAutoPause,
                isChecked =>
                {
                    Console.WriteLine($"覆盖自动暂停: {isChecked}");
                    PathManager.Instance.GetSettinggs().OverAutoPause = isChecked;
                    RendererProcessController.Instance.EnableAutoPause = isChecked;
                }
            );

            // 开机自启动（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "AutoStartOnBoot",
                "开机自启动",
                PathManager.Instance.GetSettinggs().StartupWithSystem,
                isChecked =>
                {
                    Console.WriteLine($"开机自启动: {isChecked}");
                    PathManager.Instance.GetSettinggs().StartupWithSystem = isChecked;
                }
            );

            // 夜间模式自启动（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "NightModeAutoStart",
                "夜间模式自启动",
                PathManager.Instance.GetSettinggs().NightModeAutoStart,
                isChecked =>
                {
                    Console.WriteLine($"夜间模式自启动: {isChecked}");
                    PathManager.Instance.GetSettinggs().NightModeAutoStart = isChecked;
                }
            );
            #endregion

            #region 三、3D设置
            AdjustUI.Instance.AddCategory("三、3D设置");

            // 3D跟随鼠标（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "Enable3DFollow",
                "3D跟随鼠标",
                PathManager.Instance.GetSettinggs().MouseFollow3D,
                isChecked =>
                {
                    Console.WriteLine($"3D跟随鼠标: {isChecked}");
                    PathManager.Instance.GetSettinggs().MouseFollow3D = isChecked;
                    IPCSender.Instance.SendMessageAsync(new TriDSettingsMessage { MouseFollow3D = isChecked });
                }
            );

            // 反向跟随（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "FollowReverse",
                "反向跟随",
                PathManager.Instance.GetSettinggs().FollowReverse,
                isChecked =>
                {
                    Console.WriteLine($"反向跟随: {isChecked}");
                    PathManager.Instance.GetSettinggs().FollowReverse = isChecked;
                    IPCSender.Instance.SendMessageAsync(new TriDSettingsMessage { FollowReverse = isChecked });
                }
            );

            // 跟随强度（Slider）
            AdjustUI.Instance.AddSlider(
                "MouseFollowIntensity",
                "跟随强度调节",
                0,
                1,
                PathManager.Instance.GetSettinggs().MouseFollowIntensity,
                value =>
                {
                    Console.WriteLine($"跟随强度变更为: {value}");
                    PathManager.Instance.GetSettinggs().MouseFollowIntensity = value;
                    IPCSender.Instance.SendMessageAsync(new TriDSettingsMessage { MouseFollowIntensity = value });
                }
            );

            // 视角旋转强度（Slider）
            AdjustUI.Instance.AddSlider(
                "ViewMoveIntensity",
                "视角旋转强度",
                0,
                1,
                PathManager.Instance.GetSettinggs().ViewMoveIntensity,
                value =>
                {
                    Console.WriteLine($"视角旋转强度变更为: {value}");
                    PathManager.Instance.GetSettinggs().ViewMoveIntensity = value;
                    IPCSender.Instance.SendMessageAsync(new TriDSettingsMessage { ViewMoveIntensity = value });
                }
            );

            // 视角平移强度（Slider）
            AdjustUI.Instance.AddSlider(
                "ViewRotateIntensity",
                "视角平移强度",
                0,
                1,
                PathManager.Instance.GetSettinggs().ViewRotateIntensity,
                value =>
                {
                    Console.WriteLine($"视角平移强度变更为: {value}");
                    PathManager.Instance.GetSettinggs().ViewRotateIntensity = value;
                    IPCSender.Instance.SendMessageAsync(new TriDSettingsMessage { ViewRotateIntensity = value });
                }
            );


            // 跟随动画模式（ComboBox）
            AdjustUI.Instance.AddComboBox(
                "MouseAnimation3DType",
                "跟随动画模式",
                new[] { "无", "圆圈", "反弹", "矩形", "蜜蜂" },
                "无",
                value =>
                {
                    Console.WriteLine($"播放方式变更为: {value}");

                    // 将字符串映射到枚举
                    MouseAnimation3DType mode = value switch
                    {
                        "无" => MouseAnimation3DType.None,
                        "圆圈" => MouseAnimation3DType.Circle,
                        "反弹" => MouseAnimation3DType.Rebound,
                        "矩形" => MouseAnimation3DType.Rectangle,
                        "蜜蜂" => MouseAnimation3DType.FigureEight,
                        _ => MouseAnimation3DType.None // 默认值
                    };
                    PathManager.Instance.GetSettinggs().MouseAnimation3DType = mode;
                    IPCSender.Instance.SendMessageAsync(new TriDSettingsMessage { MouseAnimation3DType = mode });
                }
            );
            #endregion

            #region 四、播放设置
            AdjustUI.Instance.AddCategory("四、播放设置");

            // 播放（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "IsPlaying",
                "播放",
                PathManager.Instance.GetSettinggs().IsPlaying,
                isChecked =>
                {
                    Console.WriteLine($"播放状态: {isChecked}");
                    PathManager.Instance.GetSettinggs().IsPlaying = isChecked;
                    IPCSender.Instance.SendMessageAsync(new PlaybackSettingsMessage { IsPlaying = isChecked });
                }
            );

            // 播放方式（ComboBox）
            AdjustUI.Instance.AddComboBox(
                "PlayMode",
                "播放方式",
                new[] { "随机切换", "顺序切换" },
                "随机切换",
                value =>
                {
                    Console.WriteLine($"播放方式变更为: {value}");

                    // 将字符串映射到枚举
                    PlaybackMode mode = value switch
                    {
                        "随机切换" => PlaybackMode.Random,
                        "顺序切换" => PlaybackMode.Sequential,
                        _ => PlaybackMode.Random // 默认值
                    };
                    PathManager.Instance.GetSettinggs().PlaybackMode = mode;
                    IPCSender.Instance.SendMessageAsync(new PlaybackSettingsMessage { PlaybackMode = mode });
                }
            );

            // 壁纸过渡方式（ComboBox）
            AdjustUI.Instance.AddComboBox(
                "TransitionMode",
                "壁纸过渡方式",
                new[] { "淡入淡出", "滑动", "缩放" },
                "淡入淡出",
                value =>
                {
                    Console.WriteLine($"壁纸过渡方式变更为: {value}");
                    TransitionType type = value switch
                    {
                        "淡入淡出" => TransitionType.Fade,
                        "滑动" => TransitionType.Slide,
                        "缩放" => TransitionType.Zoom,
                        _ => TransitionType.Fade // 默认值
                    };
                    PathManager.Instance.GetSettinggs().TransitionType = type;
                    IPCSender.Instance.SendMessageAsync(new PlaybackSettingsMessage { TransitionType = type });
                }
            );

            // 持续时间（Slider）
            AdjustUI.Instance.AddComboBox(
                "Duration",
                "持续时间(秒)",
                new[] { "30", "60", "300", "600" },
                PathManager.Instance.GetSettinggs().TransitionDuration.ToString(),
                value =>
                {
                    Console.WriteLine($"持续时间变更为: {value}秒");
                    int duration = int.Parse(value);
                    PathManager.Instance.GetSettinggs().TransitionDuration = duration;
                    IPCSender.Instance.SendMessageAsync(new PlaybackSettingsMessage { TransitionDuration = duration });
                }
            );
            #endregion

            #region 五、夜间模式
            AdjustUI.Instance.AddCategory("五、夜间模式");

            // 夜间模式（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "EnableNightMode",
                "夜间模式",
                PathManager.Instance.GetSettinggs().NightModeEnabled,
                isChecked =>
                {
                    Console.WriteLine($"夜间模式: {isChecked}");
                    PathManager.Instance.GetSettinggs().NightModeEnabled = isChecked;
                    IPCSender.Instance.SendMessageAsync(new NightModeSettingsMessage { NightModeEnabled = isChecked });
                }
            );

            // 环境光亮度（Slider）
            AdjustUI.Instance.AddSlider(
                "AmbientLightIntensity",
                "环境光亮度",
                0,
                1,
                PathManager.Instance.GetSettinggs().AmbientLightIntensity,
                value =>
                {
                    Console.WriteLine($"环境光亮度变更为: {value}");
                    PathManager.Instance.GetSettinggs().AmbientLightIntensity = value;
                    IPCSender.Instance.SendMessageAsync(new NightModeSettingsMessage { AmbientLightIntensity = value });
                }
            );

            // 光源亮度（Slider）
            AdjustUI.Instance.AddSlider(
                "LightSourceIntensity",
                "光源亮度",
                0,
                1,
                PathManager.Instance.GetSettinggs().LightSourceIntensity,
                value =>
                {
                    Console.WriteLine($"光源亮度变更为: {value}");
                    PathManager.Instance.GetSettinggs().LightSourceIntensity = value;
                    IPCSender.Instance.SendMessageAsync(new NightModeSettingsMessage { LightSourceIntensity = value });
                }
            );

            // 光源位置（MultiSlider - Vector3）
            AdjustUI.Instance.AddMultiSlider(
                "LightSourcePosition",
                "光源位置",
                -1,
                1,
                new float[]
                {
                    PathManager.Instance.GetSettinggs().LightSourcePositionX,
                    PathManager.Instance.GetSettinggs().LightSourcePositionY,
                    PathManager.Instance.GetSettinggs().LightSourcePositionZ
                },
                new string[] { "X", "Y", "Z" },
                (componentIndex, value) =>
                {
                    Console.WriteLine($"光源位置分量[{componentIndex}]变更为: {value}");
                    switch (componentIndex)
                    {
                        case 0:
                            PathManager.Instance.GetSettinggs().LightSourcePositionX = value;
                            IPCSender.Instance.SendMessageAsync(new NightModeSettingsMessage { LightSourcePositionX = value });
                            break;
                        case 1:
                            PathManager.Instance.GetSettinggs().LightSourcePositionY = value;
                            IPCSender.Instance.SendMessageAsync(new NightModeSettingsMessage { LightSourcePositionY = value });
                            break;
                        case 2:
                            PathManager.Instance.GetSettinggs().LightSourcePositionZ = value;
                            IPCSender.Instance.SendMessageAsync(new NightModeSettingsMessage { LightSourcePositionZ = value });
                            break;
                    }
                }
            );

            // 光源跟随鼠标（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "LightSourceFollowMouse",
                "光源跟随鼠标",
                PathManager.Instance.GetSettinggs().LightSourceFollowsMouse,
                isChecked =>
                {
                    Console.WriteLine($"光源跟随鼠标: {isChecked}");
                    PathManager.Instance.GetSettinggs().LightSourceFollowsMouse = isChecked;
                    IPCSender.Instance.SendMessageAsync(new NightModeSettingsMessage { LightSourceFollowsMouse = isChecked });
                }
            );
            #endregion

            #region 六、粒子特效
            AdjustUI.Instance.AddCategory("六、粒子特效");

            // 粒子特效（CheckBox）
            AdjustUI.Instance.AddCheckBox(
                "EnableParticleEffect",
                "粒子特效",
                PathManager.Instance.GetSettinggs().ParticleEffectEnabled,
                isChecked =>
                {
                    Console.WriteLine($"粒子特效: {isChecked}");
                    PathManager.Instance.GetSettinggs().ParticleEffectEnabled = isChecked;
                    IPCSender.Instance.SendMessageAsync(new ParticleEffectMessage { ParticleEffectEnabled = isChecked });
                }
            );

            // 粒子数量（NumericUpDown）
            AdjustUI.Instance.AddNumericUpDown(
                "ParticleCount",
                "粒子数量",
                100,
                10000,
                100,
                PathManager.Instance.GetSettinggs().ParticleCount,
                value =>
                {
                    Console.WriteLine($"粒子数量变更为: {value}");
                    PathManager.Instance.GetSettinggs().ParticleCount = value;
                    IPCSender.Instance.SendMessageAsync(new ParticleEffectMessage { ParticleCount = value });
                }
            );
            #endregion

            // 刷新系统面板显示
            AdjustUI.Instance.RefreshDynamicControls(SystemFlowPanel);
        }
    }
}