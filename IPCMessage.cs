using System;
using System.Collections;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text.Json.Serialization;
using XCWallPaper;


//### System Message ###

/// <summary>
/// IPC消息基类
/// </summary>
[Serializable]
public abstract class IPCMessage
{
    public string MessageType { get; set; }
    public long Timestamp { get; set; }

    protected IPCMessage(string messageType)
    {
        MessageType = messageType;
        Timestamp = DateTime.Now.Ticks;
    }
}

/// <summary>
/// 心跳消息
/// </summary>
[Serializable]
public class HeartBeatMessage : IPCMessage
{
    public HeartBeatMessage() : base(nameof(HeartBeatMessage)) { }
}

/// <summary>
/// 启动关闭消息
/// </summary>
[Serializable]
public class ActiveMessage : IPCMessage
{
    public bool Active { get; set; } = true;
    public ActiveMessage() : base(nameof(ActiveMessage)) { }
}

/// <summary>
/// 壁纸切换消息
/// </summary>
[Serializable]
public class WallPaperUpdateMessage : IPCMessage
{
    public byte[] ImageData { get; set; }  // 图片数据
    public byte[] DepthData { get; set; }  // 深度图数据    

    public WallPaperUpdateMessage() : base(nameof(WallPaperUpdateMessage)) { }
}

/// <summary>
/// 显示设置消息
/// </summary>
[Serializable]
public class DisplaySettingsMessage : IPCMessage
{
    public int FrameRate { get; set; }
    public float ResolutionWidth { get; set; }
    public float ResolutionHeight { get; set; }
    public bool EdgeSoftEnabled { get; set; }
    public bool EdgeHighlight {  get; set; }
    public bool Bloom {  get; set; }
    public bool BlurEffectEnabled { get; set; }
    public float BlurIntensity { get; set; }

    public DisplaySettingsMessage() : base(nameof(DisplaySettingsMessage))
    {
        FrameRate = PathManager.Instance.GetSettinggs().FrameRate;
        ResolutionWidth = PathManager.Instance.GetSettinggs().ResolutionWidth;
        ResolutionHeight = PathManager.Instance.GetSettinggs().ResolutionHeight;
        EdgeSoftEnabled = PathManager.Instance.GetSettinggs().EdgeSoftEnabled;
        EdgeHighlight = PathManager.Instance.GetSettinggs().EdgeHighlight;
        Bloom = PathManager.Instance.GetSettinggs().Bloom;
        BlurEffectEnabled = PathManager.Instance.GetSettinggs().BlurEffectEnabled;
        BlurIntensity = PathManager.Instance.GetSettinggs().BlurIntensity;
    }
}

/// <summary>
/// 系统设置消息
/// </summary>
[Serializable]
public class SystemSettingsMessage : IPCMessage
{
    public bool OverAutoPause { get; set; }      //覆盖自动暂停
    public bool StartupWithSystem { get; set; }   // 开机自启动
    public bool NightModeAutoStart { get; set; } // 夜间模式自启动

    public SystemSettingsMessage() : base(nameof(SystemSettingsMessage))
    {
        OverAutoPause = PathManager.Instance.GetSettinggs().OverAutoPause;
        StartupWithSystem = PathManager.Instance.GetSettinggs().StartupWithSystem;
        NightModeAutoStart = PathManager.Instance.GetSettinggs().NightModeAutoStart;
    }
}

/// <summary>
/// 3D设置消息
/// </summary>
[Serializable]
public class TriDSettingsMessage : IPCMessage
{
    public bool MouseFollow3D { get; set; }  // 启用3D鼠标跟随
    public bool FollowReverse { get; set; }    // 反向跟随
    public float MouseFollowIntensity { get; set; }  //跟随强度
    public float ViewMoveIntensity { set; get; }     //视角平移强度
    public float ViewRotateIntensity { get; set; }   //视角旋转强度
    public MouseAnimation3DType MouseAnimation3DType { get; set; }  // 跟随动画

    public TriDSettingsMessage() : base(nameof(TriDSettingsMessage))
    {
        MouseFollow3D = PathManager.Instance.GetSettinggs().MouseFollow3D;
        MouseFollow3D = PathManager.Instance.GetSettinggs().FollowReverse;
        MouseFollowIntensity = PathManager.Instance.GetSettinggs().MouseFollowIntensity;
        ViewMoveIntensity = PathManager.Instance.GetSettinggs().ViewMoveIntensity;
        ViewRotateIntensity = PathManager.Instance.GetSettinggs().ViewRotateIntensity;
        MouseAnimation3DType = PathManager.Instance.GetSettinggs().MouseAnimation3DType;
    }
}

/// <summary>
/// 播放设置消息
/// </summary>
[Serializable]
public class PlaybackSettingsMessage : IPCMessage
{
    public bool IsPlaying { get; set; } = false;    // 不播放
    public PlaybackMode PlaybackMode { get; set; } // 随机切换
    public TransitionType TransitionType { get; set; }  // 淡入淡出
    public int TransitionDuration { get; set; }  // 过渡持续60秒

    public PlaybackSettingsMessage() : base(nameof(PlaybackSettingsMessage))
    {
        IsPlaying = PathManager.Instance.GetSettinggs().IsPlaying;
        PlaybackMode = PathManager.Instance.GetSettinggs().PlaybackMode;
        TransitionType = PathManager.Instance.GetSettinggs().TransitionType;
        TransitionDuration = PathManager.Instance.GetSettinggs().TransitionDuration;
    }
}

/// <summary>
/// 夜间模式设置消息
/// </summary>
[Serializable]
public class NightModeSettingsMessage : IPCMessage
{
    public bool NightModeEnabled { get; set; }  // 启用夜间模式
    public float AmbientLightIntensity { get; set; }         // 环境光亮度
    public float LightSourceIntensity { get; set; }  // 光源亮度
    public float LightSourcePositionX { get; set; }    // 光源位置
    public float LightSourcePositionY { get; set; }    // 光源位置
    public float LightSourcePositionZ { get; set; }    // 光源位置
    public bool LightSourceFollowsMouse { get; set; } = false;   // 光源是否跟随鼠标

    public NightModeSettingsMessage() : base(nameof(NightModeSettingsMessage))
    {
        NightModeEnabled = PathManager.Instance.GetSettinggs().NightModeEnabled;
        AmbientLightIntensity = PathManager.Instance.GetSettinggs().AmbientLightIntensity;
        LightSourceIntensity = PathManager.Instance.GetSettinggs().LightSourceIntensity;
        LightSourcePositionX = PathManager.Instance.GetSettinggs().LightSourcePositionX;
        LightSourcePositionY = PathManager.Instance.GetSettinggs().LightSourcePositionY;
        LightSourcePositionZ = PathManager.Instance.GetSettinggs().LightSourcePositionZ;
        LightSourceFollowsMouse = PathManager.Instance.GetSettinggs().LightSourceFollowsMouse;
    }
}

/// <summary>
/// 粒子特效设置消息
/// </summary>
[Serializable]
public class ParticleEffectMessage : IPCMessage
{
    public bool ParticleEffectEnabled { get; set; } = true; // 默认启用粒子特效
    public int ParticleCount { get; set; } = 1000;          // 默认粒子数量1000

    public ParticleEffectMessage() : base(nameof(ParticleEffectMessage))
    {
        ParticleEffectEnabled = PathManager.Instance.GetSettinggs().ParticleEffectEnabled;
        ParticleCount = PathManager.Instance.GetSettinggs().ParticleCount;
    }
}

/// <summary>
/// 播放模式枚举
/// </summary>
[Serializable]
public enum PlaybackMode
{
    Random,  // 随机切换
    Sequential // 顺序切换
}

/// <summary>
/// 播放模式枚举
/// </summary>
[Serializable]
public enum MouseAnimation3DType
{
    None,
    Circle,
    Rebound,
    Rectangle,
    FigureEight
}

/// <summary>
/// 过渡方式枚举
/// </summary>
[Serializable]
public enum TransitionType
{
    Fade,    // 淡入淡出
    Slide,   // 滑动
    Zoom     // 缩放
}


// ### Effect Message ###
/// <summary>
/// 特效参数消息
/// </summary>
[Serializable]
public struct EffectParameterMessage
{
    public int PassID { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public object Value { get; set; }
}

/// <summary>
/// 添加特效消息
/// </summary>
[Serializable]
public class AddEffectMessage : IPCMessage
{
    public string ID { get; set; }
    public string[] HLSLCode { get; set; }
    public EffectParameterMessage[] Parameter { get; set; }
    public byte[][][] ImageData { get; set; }
    public AddEffectMessage() : base(nameof(AddEffectMessage)) { }
}

/// <summary>
/// 移除特效消息
/// </summary>
[Serializable]
public class RemoveEffectMessage : IPCMessage
{
    public string ID { get; set; }
    public RemoveEffectMessage() : base(nameof(RemoveEffectMessage)) { }
}

/// <summary>
/// 清除特效消息
/// </summary>
[Serializable]
public class ClearEffectsMessage : IPCMessage
{
    public ClearEffectsMessage() : base(nameof(ClearEffectsMessage)) { }
}


/// <summary>
/// 特效排序消息
/// </summary>
[Serializable]
public class SortEffectsMessage : IPCMessage
{
    public string[] IDs { get; set; }
    public SortEffectsMessage() : base(nameof(SortEffectsMessage)) { }
}

/// <summary>
/// 更改特效参数消息
/// </summary>
[Serializable]
public class UpdateEffectParameterMessage : IPCMessage
{
    public string ID { get; set; }
    public EffectParameterMessage Parameter { get; set; }
    public UpdateEffectParameterMessage() : base(nameof(UpdateEffectParameterMessage)) { }
}

