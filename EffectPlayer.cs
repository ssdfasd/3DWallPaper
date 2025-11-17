using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.CodeDom;

namespace XCWallPaper
{
    // 效果播放器类（核心功能）
    class EffectPlayer
    {
        private static readonly Lazy<EffectPlayer> _instance = new Lazy<EffectPlayer>(() => new EffectPlayer());
        public static EffectPlayer Instance => _instance.Value;


        // 通过IPC通信更新壁纸渲染器的特效
        public void AddEffect(EffectPackage effectPackage)
        {
            // HLSL Codes
            string hlslCode = effectPackage.HlslCode;
            List<string> hlslPasses = !string.IsNullOrEmpty(hlslCode)
            ? hlslCode.Split(
                new[] { EffectPackager.Instance.HLSLPassSeparator + Environment.NewLine },
                StringSplitOptions.None
            ).ToList()
            : new List<string>();

            // Images
            byte[][][] images = ConvertImagesTo3DArray(effectPackage.ImageResources);

            // Parameter Json
            List<EffectParameterMessage> parameterMessages = new List<EffectParameterMessage>();
            for (int i = 0; i < effectPackage.Parameters.Count; i++)
            {
                EffectParameter parameter = effectPackage.Parameters[i];
                parameterMessages.Add(new EffectParameterMessage
                {
                    PassID = parameter.PassId,
                    Type = parameter.Type,
                    Name = parameter.Name,
                    Value = parameter.DefaultValue,
                });
            }

            //IPC通信发送信息
            IPCSender.Instance.SendMessageAsync(new AddEffectMessage
            {
                ID = effectPackage.Metadata.ID,
                HLSLCode = hlslPasses.ToArray(),
                Parameter = parameterMessages.ToArray(),
                ImageData = images
            });
        }

        // 更改特效参数消息
        public void UpdateEffectParameters(string effectID, EffectParameterMessage effectParameterMessage)
        {
            IPCSender.Instance.SendMessageAsync(new UpdateEffectParameterMessage { ID = effectID, Parameter = effectParameterMessage });
        }


        // 移除特效消息
        public void RemoveEffect(string effectID)
        {
            IPCSender.Instance.SendMessageAsync(new RemoveEffectMessage { ID = effectID });
        }

        // 清除特效消息
        public void ClearEffects()
        {
            IPCSender.Instance.SendMessageAsync(new ClearEffectsMessage());
        }

        // 特效排序消息
        public void SortEffects(List<string> effectIDs)
        {
            IPCSender.Instance.SendMessageAsync(new SortEffectsMessage { IDs = effectIDs.ToArray() });
        }
        



        // ### 整理图片资源字节数组 ###
        private byte[][][] ConvertImagesTo3DArray(Dictionary<string, byte[]> images)
        {
            if (images == null || images.Count == 0)
                return new byte[0][][];

            // 确定二维数组的最大维度
            int maxX = 0;
            int maxY = 0;

            foreach (var key in images.Keys)
            {
                if (TryParseImageKey(key, out int x, out int y))
                {
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }

            // 创建三维数组（+1 是因为索引从 0 开始）
            byte[][][] imageData = new byte[maxX + 1][][];

            for (int i = 0; i <= maxX; i++)
            {
                imageData[i] = new byte[maxY + 1][];
            }

            // 填充数组
            foreach (var pair in images)
            {
                if (TryParseImageKey(pair.Key, out int x, out int y))
                {
                    imageData[x][y] = pair.Value;
                }
            }

            return imageData;
        }

        private bool TryParseImageKey(string key, out int x, out int y)
        {
            x = -1;
            y = -1;

            if (string.IsNullOrEmpty(key))
                return false;

            // 假设键格式为 "imageX_Y"，例如 "image2_2"
            var parts = key.Split('_');
            if (parts.Length != 2)
                return false;

            // 提取 "image" 后的数字
            string xPart = parts[0].Substring(5); // 从 "image" 后的位置开始

            if (!int.TryParse(xPart, out x) || !int.TryParse(parts[1], out y))
                return false;

            // 调整为 0 基索引
            x--;
            y--;

            return true;
        }
    }
}