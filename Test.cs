using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XCWallPaper
{
    /// <summary>
    /// 测试单例
    /// </summary>
    public class Test
    {
        // 单例实现
        private static readonly Lazy<Test> _instance = new Lazy<Test>(() => new Test());
        public static Test Instance => _instance.Value;


        // ### Print EffectParameters ###
        public void PrintEffectParameters(List<EffectParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                Console.WriteLine("参数列表为空");
                return;
            }

            Console.WriteLine("特效参数列表：");
            Console.WriteLine("====================================================================");
            Console.WriteLine("{0,-8} | {1,-15} | {2,-10} | {3,-20} | {4,-15} | {5,-15} | {6}",
                              "PassID", "名称", "类型", "默认值", "最小值", "最大值", "描述");
            Console.WriteLine("--------------------------------------------------------------------");

            foreach (var param in parameters)
            {
                string defaultValue = FormatValue(param.DefaultValue);
                string minValue = FormatValue(param.MinValue);
                string maxValue = FormatValue(param.MaxValue);

                Console.WriteLine("{0,-8} | {1,-15} | {2,-10} | {3,-20} | {4,-15} | {5,-15} | {6}",
                                  param.PassId,
                                  param.Name,
                                  param.Type,
                                  defaultValue,
                                  minValue,
                                  maxValue,
                                  param.Description);
            }

            Console.WriteLine("====================================================================");
        }
        private string FormatValue(object value)
        {
            if (value == null)
                return "null";

            if (value is Array arrayValue)
            {
                return "[" + string.Join(", ", arrayValue.Cast<object>()) + "]";
            }

            if (value is string[] stringArray)
            {
                return "[" + string.Join(", ", stringArray) + "]";
            }

            return value.ToString();
        }







        // ### Show Effect Package ###
        /// <summary>
        /// 显示特效包的所有信息并保存图片资源
        /// </summary>
        /// <param name="package">解封装后的特效包</param>
        /// <param name="imagePath">图片保存路径（应为目录）</param>
        public void ShowEffectPackage(EffectPackage package, string imagePath)
        {
            if (package == null)
            {
                Console.WriteLine("特效包为空，无法显示信息");
                return;
            }

            try
            {
                // 确保图片保存路径是一个有效的目录
                if (!Directory.Exists(imagePath))
                {
                    Directory.CreateDirectory(imagePath);
                    Console.WriteLine($"已创建图片保存目录: {imagePath}");
                }

                // 打印特效元数据
                Console.WriteLine("===== 特效元数据 =====");
                Console.WriteLine($"特效名称: {package.Metadata.EffectName}");
                Console.WriteLine($"作者: {package.Metadata.Author}");
                Console.WriteLine($"描述: {package.Metadata.Description}");
                Console.WriteLine($"创建时间: {package.Metadata.CreateTime}");
                Console.WriteLine($"版本号: {package.Metadata.Version}");
                Console.WriteLine("标签:");
                foreach (var tag in package.Metadata.Tags)
                {
                    Console.WriteLine($"  - {tag.Key}: {tag.Value}");
                }
                Console.WriteLine();

                // 打印HLSL代码信息
                Console.WriteLine("===== HLSL代码信息 =====");
                Console.WriteLine($"代码长度: {package.HlslCode.Length} 字符");
                Console.WriteLine("代码前100个字符:");
                string previewCode = package.HlslCode.Length > 100
                    ? package.HlslCode.Substring(0, 100) + "..."
                    : package.HlslCode;
                Console.WriteLine(previewCode);
                Console.WriteLine();

                // 打印参数信息
                Console.WriteLine("===== 特效参数信息 =====");
                if (package.Parameters == null || package.Parameters.Count == 0)
                {
                    Console.WriteLine("没有参数");
                }
                else
                {
                    foreach (var param in package.Parameters)
                    {
                        Console.WriteLine($"参数名称: {param.Name}");
                        Console.WriteLine($"所属Pass: {param.PassId}");
                        Console.WriteLine($"参数类型: {param.Type}");
                        Console.WriteLine($"默认值: {param.DefaultValue}");
                        Console.WriteLine($"最小值: {param.MinValue}");
                        Console.WriteLine($"最大值: {param.MaxValue}");
                        Console.WriteLine($"描述: {param.Description}");
                        Console.WriteLine("---------------------");
                    }
                }
                Console.WriteLine();

                // 保存并打印图片资源信息
                Console.WriteLine("===== 图片资源信息 =====");
                if (package.ImageResources == null || package.ImageResources.Count == 0)
                {
                    Console.WriteLine("没有图片资源");
                }
                else
                {
                    int savedCount = 0;
                    foreach (var image in package.ImageResources)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(image.Key);
                            if (string.IsNullOrEmpty(fileName))
                            {
                                fileName = $"image_{savedCount}{GetDefaultImageExtension()}";
                            }

                            string fullPath = Path.Combine(imagePath, fileName);
                            File.WriteAllBytes(fullPath, image.Value);
                            savedCount++;
                            Console.WriteLine($"已保存图片: {fileName} (大小: {image.Value.Length} 字节)");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"保存图片 {image.Key} 时出错: {ex.Message}");
                        }
                    }
                    Console.WriteLine($"共保存 {savedCount} 张图片到: {imagePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"显示特效包信息时出错: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// 获取默认图片扩展名（当文件名无扩展名时使用）
        /// </summary>
        private string GetDefaultImageExtension()
        {
            // 这里假设默认图片格式为PNG，可根据实际情况修改
            return ".png";
        }

        /// <summary>
        /// 显示特效包的所有信息并保存图片资源（带进度显示）
        /// </summary>
        public void ShowEffectPackageWithProgress(EffectPackage package, string imagePath, IProgress<string> progress = null)
        {
            if (package == null)
            {
                ReportProgress(progress, "特效包为空，无法显示信息");
                return;
            }

            try
            {
                if (!Directory.Exists(imagePath))
                {
                    Directory.CreateDirectory(imagePath);
                    ReportProgress(progress, $"已创建图片保存目录: {imagePath}");
                }

                ReportProgress(progress, "===== 开始显示特效包信息 =====");
                ReportProgress(progress, "===== 特效元数据 =====");
                ReportProgress(progress, $"特效名称: {package.Metadata.EffectName}");
                ReportProgress(progress, $"作者: {package.Metadata.Author}");
                ReportProgress(progress, $"描述: {package.Metadata.Description}");
                ReportProgress(progress, $"创建时间: {package.Metadata.CreateTime}");
                ReportProgress(progress, $"版本号: {package.Metadata.Version}");
                ReportProgress(progress, "标签:");
                foreach (var tag in package.Metadata.Tags)
                {
                    ReportProgress(progress, $"  - {tag.Key}: {tag.Value}");
                }

                ReportProgress(progress, "\n===== HLSL代码信息 =====");
                ReportProgress(progress, $"代码长度: {package.HlslCode.Length} 字符");
                string previewCode = package.HlslCode.Length > 100
                    ? package.HlslCode.Substring(0, 100) + "..."
                    : package.HlslCode;
                ReportProgress(progress, "代码前100个字符:\n" + previewCode);

                ReportProgress(progress, "\n===== 特效参数信息 =====");
                if (package.Parameters == null || package.Parameters.Count == 0)
                {
                    ReportProgress(progress, "没有参数");
                }
                else
                {
                    foreach (var param in package.Parameters)
                    {
                        ReportProgress(progress, $"参数名称: {param.Name}");
                        ReportProgress(progress, $"所属Pass: {param.PassId}");
                        ReportProgress(progress, $"参数类型: {param.Type}");
                        ReportProgress(progress, $"默认值: {param.DefaultValue}");
                        ReportProgress(progress, $"最小值: {param.MinValue}");
                        ReportProgress(progress, $"最大值: {param.MaxValue}");
                        ReportProgress(progress, $"描述: {param.Description}");
                        ReportProgress(progress, "---------------------");
                    }
                }

                ReportProgress(progress, "\n===== 图片资源信息 =====");
                if (package.ImageResources == null || package.ImageResources.Count == 0)
                {
                    ReportProgress(progress, "没有图片资源");
                }
                else
                {
                    int totalImages = package.ImageResources.Count;
                    int savedCount = 0;

                    foreach (var image in package.ImageResources)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(image.Key);
                            if (string.IsNullOrEmpty(fileName))
                            {
                                fileName = $"image_{savedCount}{GetDefaultImageExtension()}";
                            }

                            string fullPath = Path.Combine(imagePath, fileName);
                            File.WriteAllBytes(fullPath, image.Value);
                            savedCount++;
                            ReportProgress(progress, $"已保存图片 {savedCount}/{totalImages}: {fileName} (大小: {image.Value.Length} 字节)");
                        }
                        catch (Exception ex)
                        {
                            ReportProgress(progress, $"保存图片 {image.Key} 时出错: {ex.Message}");
                        }
                    }
                    ReportProgress(progress, $"共保存 {savedCount} 张图片到: {imagePath}");
                }

                ReportProgress(progress, "===== 特效包信息显示完成 =====");
            }
            catch (Exception ex)
            {
                ReportProgress(progress, $"显示特效包信息时出错: {ex.Message}");
                ReportProgress(progress, ex.StackTrace);
            }
        }

        /// <summary>
        /// 报告进度（安全调用）
        /// </summary>
        private void ReportProgress(IProgress<string> progress, string message)
        {
            progress?.Report(message);
            Console.WriteLine(message);
        }
    }
}
