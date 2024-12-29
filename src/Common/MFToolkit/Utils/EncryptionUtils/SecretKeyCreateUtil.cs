using System.Security.Cryptography;

namespace MFToolkit.Utils.EncryptionUtils;
/// <summary>
/// 密钥生成
/// </summary>
public class SecretKeyCreateUtil
{
    public static readonly int[] ValidSymmetricKeySizesInBytes = [16, 24, 32];

    /// <summary>
    /// 创建对称密钥
    /// </summary>
    /// <param name="keySizeInBytes">密钥大小（字节）</param>
    /// <returns></returns>
    public static byte[] CreateSymmetricKey(int keySizeInBytes = 32)
    {
        if (!ValidSymmetricKeySizesInBytes.Contains(keySizeInBytes))
        {
            throw new ArgumentException("无效的密钥大小。有效的大小为16、24或32字节。\nInvalid key size. Valid sizes are 16, 24, or 32 bytes.");
        }

        using Aes aes = Aes.Create();
        aes.KeySize = keySizeInBytes * 8; // 指定密钥大小（位）
        aes.GenerateKey(); // 生成一个随机密钥
        return aes.Key;
    }
    /// <summary>
    /// 创建非对称加密密钥
    /// <para>
    /// 可使用本方法生成的公私钥在本类中使用加解密<see cref="RSAEncryption.RSAEncryptionUtil"/>
    /// </para>
    /// </summary>
    /// <param name="keySizeInBits">长度（位）</param>
    /// <returns></returns>
    public static AsymmetricalSecretKey CreateAsymmetricalSecretKey(int keySizeInBits = 2048)
    {
        if (keySizeInBits < 384 || keySizeInBits > 16384 || keySizeInBits % 64 != 0)
        {
            // 如果不合法，抛出一个异常或者使用一个默认值
            throw new ArgumentException("密钥长度必须是384 ~ 16384位之间64的整数倍\nThe key size must be a multiple of 64 between 384 and 16384 bits");
            // 或者
            // keySizeInBits = 384;
        }
        using RSA rsa = RSA.Create(keySizeInBits);
        return new AsymmetricalSecretKey(
            rsa.ExportRSAPrivateKey(),
            rsa.ExportRSAPublicKey()
        );
    }
}


/// <summary>
/// 非对称加密 密钥类
/// </summary>
public sealed record AsymmetricalSecretKey(byte[] PrivateKey, byte[] PublicKey);