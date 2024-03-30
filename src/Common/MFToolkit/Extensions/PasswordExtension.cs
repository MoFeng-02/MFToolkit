using System.Security.Cryptography;
using System.Text;
using MFToolkit.Utils.EncryptionUtils.MD5Encryption;

namespace MFToolkit.Extensions;
public static class PasswordExtension
{
    /// <summary>
    /// 加密密钥,自己可以提供
    /// </summary>
    private const string PasswordEncryptionKey = "oV0uT0gZ3hF3mH5tJ7nG5hM3aN8bW5mZ";
    /// <summary>
    /// 加密密码
    /// </summary>
    /// <param name="oriPwd">原密码</param>
    /// <param name="encryptionType">加密方式</param>
    /// <param name="encryptionKey">加密私钥</param>
    /// <returns></returns>
    public static string ToEncryptionPassword(this string oriPwd, EncryptionType encryptionType = EncryptionType.MD5, string encryptionKey = PasswordEncryptionKey)
    {
        if (encryptionType == EncryptionType.MD5)
        {
            return MD5Util.Encrypt(oriPwd + encryptionKey);
        }
        if (encryptionType == EncryptionType.SHA1)
        {
            // 将字符串转换为字节数组
            byte[] data = Encoding.UTF8.GetBytes(oriPwd + encryptionKey);
            // 计算字节数组的散列值
            byte[] hash = SHA1.HashData(data);
            // 将散列值转换为十六进制字符串
            string output = BitConverter.ToString(hash).Replace("-", "");
            return output;
        }
        if(encryptionType == EncryptionType.SHA256)
        {
            // 将字符串转换为字节数组
            byte[] data = Encoding.UTF8.GetBytes(oriPwd + encryptionKey);
            // 计算字节数组的散列值
            byte[] hash = SHA256.HashData(data);
            // 将散列值转换为十六进制字符串
            string output = BitConverter.ToString(hash).Replace("-", "");
            return output;
        }
        if(encryptionType == EncryptionType.SHA384)
        {
            // 将字符串转换为字节数组
            byte[] data = Encoding.UTF8.GetBytes(oriPwd + encryptionKey);
            // 计算字节数组的散列值
            byte[] hash = SHA384.HashData(data);
            // 将散列值转换为十六进制字符串
            string output = BitConverter.ToString(hash).Replace("-", "");
            return output;
        }
        if (encryptionType == EncryptionType.SHA512)
        {
            // 将字符串转换为字节数组
            byte[] data = Encoding.UTF8.GetBytes(oriPwd + encryptionKey);
            // 计算字节数组的散列值
            byte[] hash = SHA512.HashData(data);
            // 将散列值转换为十六进制字符串
            string output = BitConverter.ToString(hash).Replace("-", "");
            return output;
        }
        return "";
    }
}
public enum EncryptionType
{
    MD5,
    SHA1,
    SHA256,
    SHA384,
    SHA512,
}