using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text.Json;
using static XCWallPaper.EffectPackager;

/*
### HEAD ###
FILE_SIGNATURE      4
CURRENT_VERSION     4
哈希值              32  (只计算40后的数据)
文件长度            8   (占位)
元数据长度          4
元数据              元数据长度

HSL位置             8   (占位)
图片位置            8   (占位)
填充                HEADER_SIZE - 之前已有的长度

### DATA ###
PackageBlockType    1
参数列表数据长度    4
参数列表数据        参数列表数据长度
PackageBlockType    1
HLSL数据长度        8
HLSL数据            HLSL数据长度
PackageBlockType    1
图片数据长度        8
图片数据            图片数据长度
PackageBlockType    1
 */

namespace XCWallPaper
{
    /// <summary>
    /// 特效文件封装管理器 - 负责HLSL代码和图片资源的自定义格式封装与解封装
    /// </summary>
    public class EffectPackager
    {
        // 单例实现
        private static readonly Lazy<EffectPackager> _instance = new Lazy<EffectPackager>(() => new EffectPackager());
        public static EffectPackager Instance => _instance.Value;

        // 文件格式常量定义
        private const string FILE_SIGNATURE = "XCWP";        // 文件签名，用于标识文件类型
        private const int CURRENT_VERSION = 1;              // 升级版本号以支持新格式
        private const int HEADER_SIZE = 1024;               // 文件头固定大小
        private const int HASH_START_POSITION = 48;         // 哈希校验开始位置
        private const string PACKAGE_EXTENSION = ".xcpe";   // 自定义包文件扩展名

        public string HLSLPassSeparator = "//=====PASS_SEPARATOR=====";

        // 私有构造函数
        private EffectPackager() { }

        public string GetPackagePath(string name)
        {
            string tempFolderPath = PathManager.Instance.TempDirectory;
            return Path.Combine(tempFolderPath, name + PACKAGE_EXTENSION);
        }


        #region 封装功能 - 将HLSL和图片资源打包为自定义格式
        /// <summary>
        /// 封装特效文件为自定义格式
        /// </summary>
        /// <param name="hlslCode">HLSL源代码</param>
        /// <param name="imagePaths">图片资源路径列表</param>
        /// <param name="metadata">特效元数据</param>
        /// <param name="parameters">参数配置</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="encryptKey">加密密钥（可选）</param>
        /// <returns>封装是否成功</returns>
        public bool PackageEffect(string hlslCode, List<string> imagePaths, EffectMetadata metadata, List<EffectParameter> parameters, string outputPath, string encryptKey = null)
        {
            if (string.IsNullOrEmpty(outputPath))
                outputPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),
                    $"{metadata.EffectName}_{DateTime.Now:yyyyMMdd_HHmmss}{PACKAGE_EXTENSION}");

            try
            {
                using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    // 1. 写入文件头（不包含哈希值，先占位）
                    WriteFileHeader(fs, metadata, parameters);

                    // 2. 写入参数区域
                    WriteParameters(fs, parameters);

                    // 3. 写入HLSL代码
                    WriteHlslCode(fs, hlslCode, encryptKey);

                    // 4. 写入图片资源
                    WriteImageResources(fs, imagePaths, encryptKey);

                    // 5. 计算哈希值并更新文件头
                    using (BinaryWriter bw = new BinaryWriter(fs, Encoding.UTF8, true))
                    {
                        // 标记文件结束
                        bw.Write((byte)PackageBlockType.EndOfFile);
                    }

                    // 6. 计算哈希值并更新文件头
                    EndUpdateFileHash(fs);

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"封装特效文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 异步封装特效文件（适用于大文件或UI线程）
        /// </summary>
        public async Task<bool> PackageEffectAsync(string hlslCode, List<string> imagePaths, EffectMetadata metadata, List<EffectParameter> parameters, string outputPath = null, string encryptKey = null)
        {
            return await Task.Run(() => PackageEffect(hlslCode, imagePaths, metadata, parameters, outputPath, encryptKey));
        }

        /// <summary>
        /// 写入文件头信息（不包含哈希值）
        /// </summary>
        private void WriteFileHeader(FileStream fs, EffectMetadata metadata, List<EffectParameter> parameters)
        {
            // 创建二进制写入器
            using (BinaryWriter bw = new BinaryWriter(fs, Encoding.UTF8, true))
            {
                // 写入文件签名
                bw.Write(Encoding.UTF8.GetBytes(FILE_SIGNATURE));

                // 写入版本号
                bw.Write(CURRENT_VERSION);

                // 为哈希值预留32字节
                bw.Write(new byte[32]);

                // 为文件长度预留8字节
                bw.Write(new byte[8]);

                // 写入元数据长度和内容
                byte[] metadataBytes = JsonSerializer.SerializeToUtf8Bytes(metadata);
                bw.Write(metadataBytes.Length);
                bw.Write(metadataBytes);

                // 写入HSL位置（预留8字节）
                bw.Write(new byte[8]);

                // 写入图片位置（预留8字节）
                bw.Write(new byte[8]);

                // 填充剩余空间到文件头内容大小(40字节)
                int headerContentSize = 4 + 4 + 32 + 8 + 4; // 签名+版本+哈希占位+文件长度占位+元数据长度
                headerContentSize += metadataBytes.Length + 8 + 8; // 元数据+HSL位置+图片位置

                // 填充到完整文件头大小
                int paddingSize = HEADER_SIZE - headerContentSize;
                if (paddingSize > 0)
                {
                    bw.Write(new byte[paddingSize]);
                }
                else if (paddingSize < 0)
                {
                    MessageBox.Show($"头文件内容长度超出限制！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 计算哈希值并更新文件头
        /// </summary>
        private void EndUpdateFileHash(FileStream fs)
        {
            long currentPosition = fs.Position;

            // 计算从40字节开始的数据的哈希
            fs.Seek(HASH_START_POSITION, SeekOrigin.Begin);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(fs);

                // 写入哈希值到文件头
                fs.Seek(8, SeekOrigin.Begin); // 签名(4) + 版本(4)
                using (BinaryWriter bw = new BinaryWriter(fs, Encoding.UTF8, true))
                {
                    bw.Write(hash);

                    // 写入文件总长度
                    long fileLength = fs.Length;
                    bw.Write(fileLength);
                }
            }

            // 恢复到原来的位置
            fs.Seek(currentPosition, SeekOrigin.Begin);
        }

        /// <summary>
        /// 写入参数区域
        /// </summary>
        private void WriteParameters(FileStream fs, List<EffectParameter> parameters)
        {
            using (BinaryWriter bw = new BinaryWriter(fs, Encoding.UTF8, true))
            {
                // 标记参数区域
                bw.Write((byte)PackageBlockType.Parameters);

                // 序列化并写入参数
                byte[] paramBytes = JsonSerializer.SerializeToUtf8Bytes(parameters);
                bw.Write(paramBytes.Length);
                bw.Write(paramBytes);
            }
        }

        /// <summary>
        /// 写入HLSL代码
        /// </summary>
        private void WriteHlslCode(FileStream fs, string hlslCode, string encryptKey)
        {
            using (BinaryWriter bw = new BinaryWriter(fs, Encoding.UTF8, true))
            {
                // 标记HLSL数据块
                bw.Write((byte)PackageBlockType.HlslCode);

                // 处理HLSL代码
                byte[] codeBytes = Encoding.UTF8.GetBytes(hlslCode);

                // 如需加密
                if (!string.IsNullOrEmpty(encryptKey))
                {
                    codeBytes = EncryptData(codeBytes, encryptKey);
                }

                // 写入数据长度和内容
                bw.Write(codeBytes.Length);
                bw.Write(codeBytes);
            }
        }

        /// <summary>
        /// 写入图片资源
        /// </summary>
        private void WriteImageResources(FileStream fs, List<string> imagePaths, string encryptKey)
        {
            using (BinaryWriter bw = new BinaryWriter(fs, Encoding.UTF8, true))
            {
                // 标记图片资源块
                bw.Write((byte)PackageBlockType.ImageResources);

                // 写入图片数量
                bw.Write(imagePaths.Count);

                // 处理每张图片
                foreach (string imagePath in imagePaths)
                {
                    if (!File.Exists(imagePath))
                    {
                        bw.Write(0);
                        bw.Write(0);
                        continue;
                    }

                    try
                    {
                        // 读取图片文件
                        using (FileStream imgFs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                        {
                            byte[] imgBytes = new byte[imgFs.Length];
                            imgFs.Read(imgBytes, 0, imgBytes.Length);

                            // 如需加密
                            if (!string.IsNullOrEmpty(encryptKey))
                            {
                                imgBytes = EncryptData(imgBytes, encryptKey);
                            }

                            // 写入图片文件名（仅文件名，不含路径）
                            string fileName = Path.GetFileName(imagePath);
                            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                            bw.Write(fileNameBytes.Length);
                            bw.Write(fileNameBytes);

                            // 写入图片数据长度和内容
                            bw.Write(imgBytes.Length);
                            bw.Write(imgBytes);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"处理图片 {imagePath} 时出错: {ex.Message}");
                    }
                }
            }
        }
        #endregion

        #region 解封装功能 - 从自定义格式提取HLSL和图片资源
        /// <summary>
        /// 从自定义格式解封装特效文件
        /// </summary>
        /// <param name="packagePath">封装文件路径</param>
        /// <param name="decryptKey">解密密钥（可选）</param>
        /// <returns>解封装后的特效数据</returns>
        public EffectPackage UnpackageEffect(string packagePath, string decryptKey = null)
        {
            if (!File.Exists(packagePath))
                throw new FileNotFoundException("特效封装文件不存在", packagePath);

            try
            {
                using (FileStream fs = new FileStream(packagePath, FileMode.Open, FileAccess.Read))
                {
                    // 1. 验证文件头
                    if (!VerifyFileHeader(fs))
                        throw new InvalidDataException("文件头验证失败，可能不是有效的特效封装文件");

                    // 2. 验证文件哈希
                    if (!VerifyFileHash(fs, packagePath))
                        throw new InvalidDataException("文件校验失败，可能已被篡改");

                    // 3. 读取元数据
                    EffectMetadata metadata = ReadMetadata(fs);

                    // 4. 读取参数区域
                    List<EffectParameter> parameters = ReadParameters(fs);

                    // 5. 读取HLSL代码
                    string hlslCode = ReadHlslCode(fs, decryptKey);

                    // 6. 读取图片资源
                    Dictionary<string, byte[]> imageResources = ReadImageResources(fs, decryptKey);

                    return new EffectPackage(metadata, hlslCode, imageResources, parameters);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解封装特效文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 读取参数区域
        /// </summary>
        public List<EffectParameter> ReadParameters(string packagePath)
        {
            if (!File.Exists(packagePath))
                throw new FileNotFoundException("特效封装文件不存在", packagePath);

            try
            {
                using (FileStream fs = new FileStream(packagePath, FileMode.Open, FileAccess.Read))
                {
                    // 1. 验证文件头
                    if (!VerifyFileHeader(fs))
                        throw new InvalidDataException("文件头验证失败，可能不是有效的特效封装文件");

                    // 2. 跳过元数据区域
                    SkipMetadata(fs);

                    // 3. 读取参数区域
                    return ReadParameters(fs);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取参数区域时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 验证文件头
        /// </summary>
        private bool VerifyFileHeader(FileStream fs)
        {
            using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8, true))
            {
                // 读取文件签名
                byte[] signatureBytes = br.ReadBytes(4);
                string signature = Encoding.UTF8.GetString(signatureBytes);

                // 读取版本号
                int version = br.ReadInt32();

                // 验证签名和版本
                return signature == FILE_SIGNATURE && version >= 1;
            }
        }

        /// <summary>
        /// 读取元数据
        /// </summary>
        private EffectMetadata ReadMetadata(FileStream fs)
        {
            using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8, true))
            {
                // 定位到元数据长度
                fs.Seek(4 + 4 + 32 + 8, SeekOrigin.Begin); // 签名(4) + 版本(4) + 哈希(32) + 文件长度(8)

                // 读取元数据长度
                int metadataLength = br.ReadInt32();

                // 读取元数据字节数组
                byte[] metadataBytes = br.ReadBytes(metadataLength);

                // 反序列化为元数据对象
                return JsonSerializer.Deserialize<EffectMetadata>(metadataBytes);
            }
        }

        /// <summary>
        /// 跳过元数据区域
        /// </summary>
        private void SkipMetadata(FileStream fs)
        {
            using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8, true))
            {
                // 定位到元数据长度
                fs.Seek(4 + 4 + 32 + 8, SeekOrigin.Begin); // 签名(4) + 版本(4) + 哈希(32) + 文件长度(8)

                // 读取元数据长度
                int metadataLength = br.ReadInt32();

                // 跳过元数据
                fs.Seek(metadataLength + 8 + 8, SeekOrigin.Current); // 元数据 + HSL位置 + 图片位置
            }
        }

        /// <summary>
        /// 读取参数区域
        /// </summary>
        private List<EffectParameter> ReadParameters(FileStream fs)
        {
            using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8, true))
            {
                // 定位到参数区域
                fs.Seek(HEADER_SIZE, SeekOrigin.Begin);

                // 读取参数块类型
                PackageBlockType blockType = (PackageBlockType)br.ReadByte();
                if (blockType != PackageBlockType.Parameters)
                    throw new InvalidDataException("未找到参数区域");

                // 读取参数数据长度
                int dataLength = br.ReadInt32();

                // 读取参数数据
                byte[] dataBytes = br.ReadBytes(dataLength);

                // 反序列化为参数列表
                return JsonSerializer.Deserialize<List<EffectParameter>>(dataBytes);
            }
        }

        /// <summary>
        /// 读取HLSL代码
        /// </summary>
        private string ReadHlslCode(FileStream fs, string decryptKey)
        {
            using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8, true))
            {
                // 读取块类型
                PackageBlockType blockType = (PackageBlockType)br.ReadByte();
                if (blockType != PackageBlockType.HlslCode)
                    throw new InvalidDataException("未找到HLSL代码块");

                // 读取数据长度
                int dataLength = br.ReadInt32();

                // 读取数据
                byte[] dataBytes = br.ReadBytes(dataLength);

                // 如需解密
                if (!string.IsNullOrEmpty(decryptKey))
                {
                    dataBytes = DecryptData(dataBytes, decryptKey);
                }

                // 转换为字符串
                return Encoding.UTF8.GetString(dataBytes);
            }
        }

        /// <summary>
        /// 读取图片资源
        /// </summary>
        private Dictionary<string, byte[]> ReadImageResources(FileStream fs, string decryptKey)
        {
            Dictionary<string, byte[]> imageResources = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8, true))
            {
                // 读取块类型
                PackageBlockType blockType = (PackageBlockType)br.ReadByte();

                if (blockType == PackageBlockType.ImageResources)
                {
                    // 读取图片数量
                    int imageCount = br.ReadInt32();

                    // 读取每张图片
                    for (int i = 0; i < imageCount; i++)
                    {
                        // 读取文件名长度和内容
                        int fileNameLength = br.ReadInt32();
                        string fileName = Encoding.UTF8.GetString(br.ReadBytes(fileNameLength));

                        // 读取图片数据长度和内容
                        int imageLength = br.ReadInt32();
                        byte[] imageBytes = br.ReadBytes(imageLength);

                        // 如需解密
                        if (!string.IsNullOrEmpty(decryptKey))
                        {
                            imageBytes = DecryptData(imageBytes, decryptKey);
                        }

                        imageResources[fileName] = imageBytes;
                    }
                }
                return imageResources;
            }
        }

        /// <summary>
        /// 验证文件哈希
        /// </summary>
        private bool VerifyFileHash(FileStream fs, string packagePath)
        {
            using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8, true))
            {
                // 读取存储的哈希值
                byte[] storedHash = br.ReadBytes(32);

                // 重新计算从40字节开始的数据的哈希
                using (SHA256 sha256 = SHA256.Create())
                {
                    using (FileStream hashFs = new FileStream(packagePath, FileMode.Open, FileAccess.Read))
                    {
                        hashFs.Seek(HASH_START_POSITION, SeekOrigin.Begin);
                        byte[] newHash = sha256.ComputeHash(hashFs);
                        return storedHash.SequenceEqual(newHash);
                    }
                }
            }
        }
        #endregion

        #region 加解密辅助功能
        /// <summary>
        /// 数据加密
        /// </summary>
        private byte[] EncryptData(byte[] data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                // 从密钥生成密钥和IV
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                    aes.Key = keyBytes.Take(32).ToArray(); // 取前32字节作为AES-256密钥
                    aes.IV = keyBytes.Skip(32).Take(16).Concat(Enumerable.Repeat((byte)0, 16)).Take(16).ToArray(); // 生成IV
                }

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 数据解密
        /// </summary>
        private byte[] DecryptData(byte[] data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                // 从密钥生成密钥和IV
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                    aes.Key = keyBytes.Take(32).ToArray();
                    aes.IV = keyBytes.Skip(32).Take(16).Concat(Enumerable.Repeat((byte)0, 16)).Take(16).ToArray();
                }

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        try
                        {
                            cs.FlushFinalBlock();
                        }
                        catch (CryptographicException)
                        {
                            // 解密失败，可能是密钥错误
                            return null;
                        }
                    }
                    return ms.ToArray();
                }
            }
        }
        #endregion

        #region 辅助类和枚举定义
        /// <summary>
        /// 封装文件块类型
        /// </summary>
        private enum PackageBlockType
        {
            Parameters = 0,     // 新增参数区域类型
            HlslCode = 1,
            ImageResources = 2,
            EndOfFile = 255
        }


        #endregion
    }

    /// <summary>
    /// 特效元数据类
    /// </summary>
    public class EffectMetadata
    {
        public string ID {  get; set; }
        public string EffectName { get; set; }         // 特效名称
        public string Author { get; set; }             // 作者
        public string Description { get; set; }        // 描述
        public DateTime CreateTime { get; set; }       // 创建时间
        public string Version { get; set; }            // 版本号
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>(); // 标签
    }

    /// <summary>
    /// 解封装后的特效包数据
    /// </summary>
    public class EffectPackage
    {
        public EffectMetadata Metadata { get; }
        public string HlslCode { get; }
        public Dictionary<string, byte[]> ImageResources { get; }
        public List<EffectParameter> Parameters { get; }

        public EffectPackage(EffectMetadata metadata, string hlslCode, Dictionary<string, byte[]> imageResources, List<EffectParameter> parameters)
        {
            Metadata = metadata;
            HlslCode = hlslCode;
            ImageResources = imageResources;
            Parameters = parameters;
        }
    }

    /// <summary>
    /// 特效参数类 - 存储（类型名，默认值，最小值，最大值）
    /// </summary>
    public class EffectParameter
    {
        public int PassId { get; set; }            // 所属于的Pass
        public string Name { get; set; }           // 参数名称
        public string Type { get; set; }           // 参数类型（float, int, bool, color等）
        public object DefaultValue { get; set; }   // 默认值
        public object MinValue { get; set; }       // 最小值
        public object MaxValue { get; set; }       // 最大值
        public string Description { get; set; }    // 参数描述
    }
}

