using System;
using System.Numerics;

namespace XCWallPaper
{
    /// <summary>
    /// 壁纸配置
    /// </summary>
    [Serializable]
    public class Settings
    {
        // 一、显示设置
        public int FrameRate { get; set; } = 60;           // 帧率
        public float ResolutionWidth { get; set; } = 1920;  // 分辨率
        public float ResolutionHeight { get; set; } = 1080;  // 分辨率
        public bool EdgeSoftEnabled {  get; set; } = false;     // 边缘柔和
        public bool EdgeHighlight {  get; set; } = false;     // 描边
        public bool Bloom {  get; set; } = false;     // 泛光
        public bool BlurEffectEnabled { get; set; } = false;  // 模糊
        public float BlurIntensity { get; set; } = 0.3f;       // 模糊强度

        // 二、系统设置
        public bool OverAutoPause { get; set; } = true;         //覆盖自动暂停
        public bool StartupWithSystem { get; set; } = true;  // 默认开机自启动
        public bool NightModeAutoStart { get; set; } = true; // 默认夜间模式自启动

        // 三、3D设置
        public bool MouseFollow3D { get; set; } = true;     // 默认启用3D鼠标跟随
        public bool FollowReverse { get; set; } = false;    // 反向跟随
        public float MouseFollowIntensity { get; set; } = 0.5f;  //跟随强度
        public float ViewMoveIntensity { get; set; } = 0.6f;  //视角平移强度
        public float ViewRotateIntensity { get; set; } = 0.6f;  //视角旋转强度
        public MouseAnimation3DType MouseAnimation3DType { get; set; } = MouseAnimation3DType.None;  // 跟随动画

        // 四、播放设置
        public bool IsPlaying { get; set; } = false;        // 默认不播放
        public PlaybackMode PlaybackMode { get; set; } = PlaybackMode.Random; // 默认随机切换
        public TransitionType TransitionType { get; set; } = TransitionType.Fade; // 默认淡入淡出
        public int TransitionDuration { get; set; } = 60;   // 默认过渡持续60秒

        // 五、夜间模式
        public bool NightModeEnabled { get; set; } = false;  // 默认启用夜间模式
        public float AmbientLightIntensity { get; set; } = 0.5f;  // 默认环境光亮度50%
        public float LightSourceIntensity { get; set; } = 0.5f; // 默认光源亮度50%
        public bool LightSourceFollowsMouse { get; set; } = false; // 光源是否跟随鼠标
        public float LightSourcePositionX { get; set; }    // 光源位置
        public float LightSourcePositionY { get; set; }
        public float LightSourcePositionZ { get; set; }


        // 六、粒子特效
        public bool ParticleEffectEnabled { get; set; } = true; // 默认启用粒子特效
        public int ParticleCount { get; set; } = 1000;          // 默认粒子数量1000

        // 默认配置
        public static Settings GetDefault()
        {
            return new Settings();
        }
    }
}

