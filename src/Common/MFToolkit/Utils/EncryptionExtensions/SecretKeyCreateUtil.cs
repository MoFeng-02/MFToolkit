using System.Security.Cryptography;

namespace MFToolkit.Utils.EncryptionExtensions;
/// <summary>
/// 密钥生成
/// </summary>
public class SecretKeyCreateUtil
{
    /// <summary>
    /// 创建对称密钥
    /// </summary>
    /// <param name="length">长度（位）</param>
    /// <returns></returns>
    public static string CreateSymmetricKey(int length = 32)
    {
        if (length % 8 != 0) throw new ArgumentException("Invalid key length. The length should be a multiple of 8.");
        using Aes aes = Aes.Create();
        aes.KeySize = length * 8;   // 指定密钥大小（位）
        aes.GenerateKey();      // 生成一个随机密钥
        string symmetricKey = Convert.ToBase64String(aes.Key); // 将密钥转换为Base64字符串
        return symmetricKey;
    }
    /// <summary>
    /// 非对称加密 密钥类
    /// </summary>
    public class AsymmetricalSecretKey
    {
        public string PrivateKey { get; set; } = null!;
        public string PublicKey { get; set; } = null!;
    }
    /// <summary>
    /// 创建非对称加密密钥
    /// </summary>
    /// <param name="length">长度（位）</param>
    /// <returns></returns>
    public static AsymmetricalSecretKey CreateAsymmetricalSecretKey(int length = 2048)
    {
        if (length < 384 || length > 16384 || length % 8 != 0)
        {
            // 如果不合法，抛出一个异常或者使用一个默认值
            throw new ArgumentException("The length is not a multiple of 8 between 384 and 16384");
            // 或者
            // length = 384;
        }
        using RSA rsa = RSA.Create(length);
        RSAParameters rsaKeyInfo = rsa.ExportParameters(true); // 导出密钥信息
        string publicKey = Convert.ToBase64String(rsaKeyInfo.Modulus); // 将公钥转换为Base64字符串
        string privateKey = Convert.ToBase64String(rsaKeyInfo.D); // 将私钥转换为Base64字符串
        var result = new AsymmetricalSecretKey
        {
            PrivateKey = privateKey,
            PublicKey = publicKey,
        };
        return result;
    }
}
