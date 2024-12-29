using System.Security.Cryptography;
using System.Text;
using MFToolkit.Utils.EncryptionUtils.MD5Encryption;

namespace MFToolkit.Extensions;
public static class PasswordExtensions
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
            return MD5EncryptionUtil.Encrypt(oriPwd + encryptionKey);
        }

        HashAlgorithm hashAlgorithm = encryptionType switch
        {
            EncryptionType.SHA1 => SHA1.Create(),
            EncryptionType.SHA256 => SHA256.Create(),
            EncryptionType.SHA384 => SHA384.Create(),
            EncryptionType.SHA512 => SHA512.Create(),
            _ => throw new ArgumentException("Unsupported encryption type.", nameof(encryptionType)),
        };

        // 计算字节数组的散列值
        byte[] data = Encoding.UTF8.GetBytes(oriPwd + encryptionKey);
        byte[] hash = hashAlgorithm.ComputeHash(data);

        // 将散列值转换为十六进制字符串
        return Convert.ToHexString(hash);
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