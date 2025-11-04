using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCWallPaper
{
    /// <summary>
    /// 用于壁纸软件的安全校验码助手类
    /// 提供基于时间的校验码生成和验证功能，防止图像数据被篡改
    /// </summary>
    public static class IPCEncrypt
    {
        // 校验码长度常量（4字节 = 32位）
        public const int ChecksumLength = 4;

        /// <summary>
        /// 计算图片数据的校验码（带时间因子）
        /// </summary>
        public static byte[] CalculateChecksum(byte[] imageBytes)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(imageBytes);
                int currentMinute = System.DateTime.Now.Minute;

                byte[] checksum = new byte[ChecksumLength];
                System.Array.Copy(hashBytes, checksum, ChecksumLength);

                checksum[0] = (byte)((checksum[0] + currentMinute) % 256);
                checksum[1] = (byte)((checksum[1] * (currentMinute % 10 + 1)) % 256);
                checksum[2] = (byte)((checksum[2] ^ (currentMinute << 3)) % 256);
                checksum[3] = (byte)((checksum[3] - currentMinute + 128) % 256);

                for (int i = 0; i < ChecksumLength; i++)
                {
                    checksum[i] = (byte)((checksum[i] << 3) | (checksum[i] >> 5));
                }

                return checksum;
            }
        }

        /// <summary>
        /// 验证图片数据的校验码
        /// 支持验证当前分钟和前一分钟生成的校验码
        /// </summary>
        public static bool VerifyChecksum(byte[] imageBytes, byte[] receivedChecksum)
        {
            int currentMinute = System.DateTime.Now.Minute;

            // 验证当前分钟和前一分钟的校验码
            for (int minuteOffset = 0; minuteOffset <= 1; minuteOffset++)
            {
                int targetMinute = (currentMinute - minuteOffset + 60) % 60;

                using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(imageBytes);
                    byte[] calculatedChecksum = new byte[ChecksumLength];
                    System.Array.Copy(hashBytes, calculatedChecksum, ChecksumLength);

                    calculatedChecksum[0] = (byte)((calculatedChecksum[0] + targetMinute) % 256);
                    calculatedChecksum[1] = (byte)((calculatedChecksum[1] * (targetMinute % 10 + 1)) % 256);
                    calculatedChecksum[2] = (byte)((calculatedChecksum[2] ^ (targetMinute << 3)) % 256);
                    calculatedChecksum[3] = (byte)((calculatedChecksum[3] - targetMinute + 128) % 256);

                    for (int i = 0; i < ChecksumLength; i++)
                    {
                        calculatedChecksum[i] = (byte)((calculatedChecksum[i] << 3) | (calculatedChecksum[i] >> 5));
                    }

                    bool match = true;
                    for (int i = 0; i < ChecksumLength; i++)
                    {
                        if (calculatedChecksum[i] != receivedChecksum[i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                        return true;
                }
            }

            return false;
        }
    }
}

