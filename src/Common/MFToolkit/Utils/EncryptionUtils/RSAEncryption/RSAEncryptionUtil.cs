using System.Security.Cryptography;
using System.Text;

namespace MFToolkit.Utils.EncryptionUtils.RSAEncryption;

/// <summary>
/// RSA 加解密工具类
/// <para>密钥创建类<see cref="SecretKeyCreateUtil"/></para>
/// </summary>
public class RSAEncryptionUtil
{
    /// <summary>
    /// 加密数据
    /// </summary>
    /// <param name="publicKey">公钥</param>
    /// <param name="plainText">明文字符串</param>
    /// <param name="padding">加密填充模式，默认OaepSHA256</param>
    /// <returns>Base64编码的密文</returns>
    public static string Encrypt(string publicKey, string plainText, RSAEncryptionPadding? padding = null)
    {
        padding ??= RSAEncryptionPadding.OaepSHA256;
        using RSA rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
        byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), padding);
        return Convert.ToBase64String(encryptedData);
    }

    /// <summary>
    /// 解密数据
    /// </summary>
    /// <param name="privateKey">私钥</param>
    /// <param name="cipherTextBase64">Base64编码的密文</param>
    /// <param name="padding">解密填充模式，默认OaepSHA256</param>
    /// <returns>解密后的明文字符串</returns>
    public static string Decrypt(string privateKey, string cipherTextBase64, RSAEncryptionPadding? padding = null)
    {
        padding ??= RSAEncryptionPadding.OaepSHA256;
        using RSA rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
        byte[] cipherText = Convert.FromBase64String(cipherTextBase64);
        byte[] decryptedData = rsa.Decrypt(cipherText, padding);
        return Encoding.UTF8.GetString(decryptedData);
    }

    /// <summary>
    /// 将密钥保存到文件
    /// </summary>
    /// <param name="key">存储密钥（JSON格式）</param>
    /// <param name="filePath">文件路径</param>
    public static async Task SaveKeyToFile(string key, string filePath)
    {
        await File.WriteAllTextAsync(filePath, key);
    }

    /// <summary>
    /// 从文件加载密钥
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>密钥（JSON格式）</returns>
    public static async Task<string> LoadKeyFromFile(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }
}
