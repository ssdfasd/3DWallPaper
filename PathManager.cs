using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.DirectoryServices.ActiveDirectory;
using System.Text.Json;

// ### 文件&路径管理器 ###
namespace XCWallPaper
{
    public class PathManager
    {
        // 单例实现
        private static readonly Lazy<PathManager> _instance = new Lazy<PathManager>(() => new PathManager());
        public static PathManager Instance => _instance.Value;

        // 应用名称和公司名称（用于路径生成）
        private readonly string _appName = "PaperCache";
        private readonly string _companyName = "Carcave";

        // ### 基础路径 ###
        public string LocalAppDataPath { get; }
        public string DocumentsPath { get; }

        // ### 配置文件路径 ###
        public string ConfigDirectory { get; }
        public string CacheDirectory { get; }
        public string CredentialsFilePath { get; }
        public string SettingsFilePath { get; }

        public Settings settings;

        // ### 用户内容路径 ###
        public string WallpapersDirectory { get; }
        public string EffectsDirectory { get; }
        public string ProjectDirectory { get; }
        public string TempDirectory { get; }

        // Project
        public string ProjectHLSLFileName { get; }
        public string ProjectParameterFileName { get; } 

        // 私有构造函数
        private PathManager()
        {
            try
            {
                // 初始化基础路径
                LocalAppDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    _companyName, _appName);

                DocumentsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    _appName);

                // 初始化配置文件路径
                ConfigDirectory = Path.Combine(LocalAppDataPath, "config");
                CacheDirectory = Path.Combine(LocalAppDataPath, "cache");
                CredentialsFilePath = Path.Combine(ConfigDirectory, "credentials.dat");
                SettingsFilePath = Path.Combine(ConfigDirectory, "settings.json");

                // 初始化用户内容路径
                WallpapersDirectory = Path.Combine(DocumentsPath, "Wallpapers");
                EffectsDirectory = Path.Combine(DocumentsPath, "Effects");
                ProjectDirectory = Path.Combine(DocumentsPath, "Project");
                TempDirectory = Path.Combine(DocumentsPath, "Temp");
                ProjectHLSLFileName = "pass.hlsl";
                ProjectParameterFileName = "pass.par";

                // 创建必要的目录
                CreateRequiredDirectories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化路径管理器时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // 创建所有必要的目录
        private void CreateRequiredDirectories()
        {
            var directories = new[]
            {
                LocalAppDataPath, ConfigDirectory, CacheDirectory,
                DocumentsPath, WallpapersDirectory, EffectsDirectory, ProjectDirectory, TempDirectory
            };

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        #region 配置文件操作

        // 保存设置文件
        public void SaveSettings(string jsonContent)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置文件出错: {ex.Message}");
            }
        }

        // 获取配置数据
        public Settings GetSettinggs()
        {
            return settings;
        }

        // 加载设置文件
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    settings = JsonConvert.DeserializeObject<Settings>(json) ?? Settings.GetDefault();
                }
                else
                {
                    // 使用默认配置
                    settings = Settings.GetDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置文件出错: {ex.Message}");
                // 发生错误时使用默认配置
                settings = Settings.GetDefault();
            }
        }

        // 保存加密的凭据
        public void SaveCredentials(string username, string password)
        {
            try
            {
                var credentials = $"{username}|{password}";
                var encryptedData = ProtectData(Encoding.UTF8.GetBytes(credentials));
                File.WriteAllBytes(CredentialsFilePath, encryptedData);
            }
            catch (Exception ex)
            {
                LogError("保存凭据失败", ex);
                throw;
            }
        }

        // 加载加密的凭据
        public (string username, string password) LoadCredentials()
        {
            try
            {
                if (File.Exists(CredentialsFilePath))
                {
                    var encryptedData = File.ReadAllBytes(CredentialsFilePath);
                    var decryptedData = UnprotectData(encryptedData);
                    var credentials = Encoding.UTF8.GetString(decryptedData);
                    var parts = credentials.Split('|');

                    if (parts.Length == 2)
                    {
                        return (parts[0], parts[1]);
                    }
                }
                return (null, null);
            }
            catch (Exception ex)
            {
                LogError("加载凭据失败", ex);
                return (null, null);
            }
        }

        #endregion

        #region 壁纸管理
        // 生成唯一文件夹名称的辅助方法
        public string GenerateUniqueFolderName()
        {
            // 获取当前时间戳
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // 生成8位随机哈希（使用GUID简化实现）
            string randomHash = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            // 组合成唯一名称
            return $"{timestamp}_{randomHash}";
        }

        // 创建新壁纸文件夹并返回路径
        public string CreateWallpaperFolder(string wallpaperId)
        {
            try
            {
                var folderPath = Path.Combine(WallpapersDirectory, wallpaperId);
                Directory.CreateDirectory(folderPath);
                return folderPath;
            }
            catch (Exception ex)
            {
                LogError($"创建壁纸文件夹失败: {wallpaperId}", ex);
                throw;
            }
        }

        // 安全删除文件夹的方法，处理可能的文件锁定问题
        public  void SafeDeleteFolder(string folderPath, int retryCount = 3)
        {
            if (!Directory.Exists(folderPath))
                return;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    // 先尝试删除文件
                    string[] files = Directory.GetFiles(folderPath);
                    foreach (string file in files)
                    {
                        try
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                            File.Delete(file);
                        }
                        catch (IOException)
                        {
                            // 尝试释放文件句柄
                            System.Threading.Thread.Sleep(100);
                            try
                            {
                                File.Delete(file);
                            }
                            catch { }
                        }
                        catch { }
                    }

                    // 再删除子文件夹
                    string[] subfolders = Directory.GetDirectories(folderPath);
                    foreach (string subfolder in subfolders)
                    {
                        SafeDeleteFolder(subfolder, retryCount);
                    }

                    // 最后删除当前文件夹
                    Directory.Delete(folderPath);
                    return;
                }
                catch (Exception)
                {
                    if (i < retryCount - 1)
                    {
                        // 等待一段时间后重试
                        System.Threading.Thread.Sleep(500);
                    }
                    else
                    {
                        throw; // 所有重试都失败，抛出异常
                    }
                }
            }
        }

        // 保存壁纸文件
        public async Task<string> SaveWallpaperFile(string wallpaperId, Stream imageStream, string fileExtension)
        {
            try
            {
                var folderPath = CreateWallpaperFolder(wallpaperId);
                var fileName = $"wallpaper{fileExtension}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageStream.CopyToAsync(fileStream);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                LogError($"保存壁纸文件失败: {wallpaperId}", ex);
                throw;
            }
        }

        // 保存壁纸配置
        public void SaveWallpaperConfig(string wallpaperId, string jsonConfig)
        {
            try
            {
                var folderPath = Path.Combine(WallpapersDirectory, wallpaperId);
                var configPath = Path.Combine(folderPath, "metadata.json");
                File.WriteAllText(configPath, jsonConfig);
            }
            catch (Exception ex)
            {
                LogError($"保存壁纸配置失败: {wallpaperId}", ex);
                throw;
            }
        }

        // 获取壁纸文件夹子文件夹
        public string GetWallpaperSubfolderPath(string subfolderName)
        {
            return Path.Combine(WallpapersDirectory, subfolderName);
        }

        // 获取所有壁纸文件夹
        public List<string> GetAllWallpaperSubFolders()
        {
            try
            {
                if (Directory.Exists(WallpapersDirectory))
                {
                    return Directory.GetDirectories(WallpapersDirectory).ToList();
                }
                return new List<string>();
            }
            catch (Exception ex)
            {
                LogError("获取壁纸文件夹列表失败", ex);
                return new List<string>();
            }
        }

        // 删除壁纸
        public void DeleteWallpaper(string wallpaperId)
        {
            try
            {
                var folderPath = Path.Combine(WallpapersDirectory, wallpaperId);
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
            }
            catch (Exception ex)
            {
                LogError($"删除壁纸失败: {wallpaperId}", ex);
                throw;
            }
        }

        #endregion

        #region 特效文件管理

        // 创建特效文件夹
        public string CreateEffectFolder(string effectId, string effectType = null)
        {
            try
            {
                var basePath = EffectsDirectory;
                if (!string.IsNullOrEmpty(effectType))
                {
                    basePath = Path.Combine(basePath, effectType);
                    Directory.CreateDirectory(basePath);
                }

                var folderPath = Path.Combine(basePath, effectId);
                Directory.CreateDirectory(folderPath);
                return folderPath;
            }
            catch (Exception ex)
            {
                LogError($"创建特效文件夹失败: {effectId}", ex);
                throw;
            }
        }

        // 保存特效文件
        public async Task<string> SaveEffectFile(string effectId, Stream fileStream, string fileName, string effectType = null)
        {
            try
            {
                var folderPath = CreateEffectFolder(effectId, effectType);
                var filePath = Path.Combine(folderPath, fileName);

                using (var outputStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(outputStream);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                LogError($"保存特效文件失败: {effectId}", ex);
                throw;
            }
        }

        // 保存特效配置
        public void SaveEffectConfig(string effectId, string jsonConfig, string effectType = null)
        {
            try
            {
                var folderPath = Path.Combine(EffectsDirectory, effectType ?? "", effectId);
                var configPath = Path.Combine(folderPath, "config.json");
                File.WriteAllText(configPath, jsonConfig);
            }
            catch (Exception ex)
            {
                LogError($"保存特效配置失败: {effectId}", ex);
                throw;
            }
        }

        #endregion

        #region 项目文件管理
        // 创建项目文件夹
        public string CreateProjectFolder(string passID)
        {
            try
            {
                var folderPath = Path.Combine(ProjectDirectory, passID);
                Directory.CreateDirectory(folderPath);
                return folderPath;
            }
            catch (Exception ex)
            {
                LogError($"创建项目文件夹失败: {passID}", ex);
                throw;
            }
        }

        // 获取项目的所有Pass文件夹
        public List<string> GetAllProjectPassFolders()
        {
            try
            {
                if (Directory.Exists(ProjectDirectory))
                {
                    return Directory.GetDirectories(ProjectDirectory).ToList();
                }
                return new List<string>();
            }
            catch (Exception ex)
            {
                LogError("获取项目文件夹列表失败", ex);
                return new List<string>();
            }
        }

        // 获取Pass文件夹
        public string GetProjectPassFolder(string passID)
        {
            try
            {
                var passPath = Path.Combine(ProjectDirectory, passID);
                if (Directory.Exists(passPath))
                {
                    return passPath;
                }
                return "";
            }
            catch (Exception ex)
            {
                LogError("获取项目文件夹失败", ex);
                return "";
            }
        }

        // 删除项目中的Pass文件夹
        public void DeleteProjectFolder(string passID)
        {
            try
            {
                var folderPath = Path.Combine(ProjectDirectory, passID);
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
            }
            catch (Exception ex)
            {
                LogError($"删除项目文件夹失败: {passID}", ex);
                throw;
            }
        }

        // 重命名项目的Pass文件夹
        public void RenameProjectFolder(string passID, string newPassID)
        {
            try
            {
                var oldFolderPath = Path.Combine(ProjectDirectory, passID);
                var newFolderPath = Path.Combine(ProjectDirectory, newPassID);

                if (!Directory.Exists(oldFolderPath))
                {
                    throw new DirectoryNotFoundException($"原文件夹不存在: {oldFolderPath}");
                }

                if (Directory.Exists(newFolderPath))
                {
                    throw new IOException($"目标文件夹已存在: {newFolderPath}");
                }

                Directory.Move(oldFolderPath, newFolderPath);
            }
            catch (Exception ex)
            {
                LogError($"重命名项目文件夹失败: {passID} 到 {newPassID}", ex);
                throw;
            }
        }

        // HLSL
        // 保存项目文件的HLSL代码
        public void SaveProjectHLSL(string passID, string hlslCode)
        {
            try
            {
                var folderPath = Path.Combine(ProjectDirectory, passID);
                if (Directory.Exists(folderPath))
                {
                    var hlslPath = Path.Combine(folderPath, ProjectHLSLFileName);
                    File.WriteAllText(hlslPath, hlslCode);
                }
            }
            catch (Exception ex)
            {
                LogError($"保存项目HLSL失败: {passID}", ex);
                throw;
            }
        }

        // 读取项目文件的HLSL代码
        public string LoadProjectHLSL(string passID)
        {
            try
            {
                var folderPath = Path.Combine(ProjectDirectory, passID);
                var hlslPath = Path.Combine(folderPath, ProjectHLSLFileName);
                if (File.Exists(hlslPath))
                {
                    return File.ReadAllText(hlslPath);
                }
                return null;
            }
            catch (Exception ex)
            {
                LogError("加载项目HLSL失败", ex);
                throw;
            }
        }

        // Parameters
        // 保存项目文件的HLSL代码
        public void SaveProjectParameters(string passID, string parameters)
        {
            try
            {
                var folderPath = Path.Combine(ProjectDirectory, passID);
                if (Directory.Exists(folderPath))
                {
                    var parameterPath = Path.Combine(folderPath, ProjectParameterFileName);
                    File.WriteAllText(parameterPath, parameters);
                }
            }
            catch (Exception ex)
            {
                LogError($"保存项目HLSL失败: {passID}", ex);
                throw;
            }
        }

        // 读取项目文件的parameters
        public string LoadProjectParameters(string passID)
        {
            try
            {
                var folderPath = Path.Combine(ProjectDirectory, passID);
                var parameterPath = Path.Combine(folderPath, ProjectParameterFileName);
                if (File.Exists(parameterPath))
                {
                    return File.ReadAllText(parameterPath);
                }
                return null;
            }
            catch (Exception ex)
            {
                LogError("加载项目HLSL失败", ex);
                throw;
            }
        }

        // Image
        // 读取项目文件的Image
        public List<string> GetProjectImagePaths(string passID)
        {
            return null;
        }
        #endregion

        #region 加密/解密方法

        // 使用DPAPI加密数据
        private byte[] ProtectData(byte[] data)
        {
            try
            {
                return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            }
            catch
            {
                // 如果DPAPI不可用（例如在非Windows系统上），使用AES加密作为备选
                return EncryptWithAES(data);
            }
        }

        // 使用DPAPI解密数据
        private byte[] UnprotectData(byte[] encryptedData)
        {
            try
            {
                return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            }
            catch
            {
                // 如果DPAPI不可用，尝试使用AES解密
                return DecryptWithAES(encryptedData);
            }
        }

        // AES加密作为备选方案
        private byte[] EncryptWithAES(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = GetEncryptionKey();
                aes.IV = new byte[16]; // 简化实现，实际应用中应使用随机IV

                using (var encryptor = aes.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        // AES解密
        private byte[] DecryptWithAES(byte[] encryptedData)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = GetEncryptionKey();
                aes.IV = new byte[16];

                using (var decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                }
            }
        }

        // 获取加密密钥（实际应用中应使用更安全的密钥管理）
        private byte[] GetEncryptionKey()
        {
            // 这里使用应用名称和机器名生成一个基本密钥
            // 实际应用中应考虑更安全的密钥存储方式
            var keyMaterial = $"{_appName}{Environment.MachineName}{Environment.UserName}";
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(keyMaterial));
            }
        }

        #endregion

        #region 辅助方法

        // 记录错误（实际应用中可以替换为日志系统）
        private void LogError(string message, Exception ex)
        {
            try
            {
                var logPath = Path.Combine(ConfigDirectory, "error.log");
                var logMessage = $"[{DateTime.Now}] {message}: {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // 如果日志记录失败，至少显示错误消息
                MessageBox.Show($"操作失败: {message}\n\n{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 清理临时缓存
        public void CleanupCache()
        {
            try
            {
                if (Directory.Exists(CacheDirectory))
                {
                    foreach (var file in Directory.GetFiles(CacheDirectory))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch { /* 忽略个别文件删除失败 */ }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("清理缓存失败", ex);
            }
        }

        // 清理临时文件夹
        public void CleanupTemp()
        {
            try
            {
                if (Directory.Exists(TempDirectory))
                {
                    foreach (var file in Directory.GetFiles(TempDirectory))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch { /* 忽略个别文件删除失败 */ }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("清理缓存失败", ex);
            }
        }

        // 检查文件是否存在
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        // 获取文件内容
        public string GetFileContent(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }
                return null;
            }
            catch (Exception ex)
            {
                LogError($"读取文件失败: {filePath}", ex);
                return null;
            }
        }

        #endregion
    }
}