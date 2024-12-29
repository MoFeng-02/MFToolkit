using System.Security.Cryptography;
using System.Text;

namespace MFToolkit.Utils.EncryptionUtils.AESEncryption;
public class AesEncryptionUtil
{
    private static readonly int[] ValidSymmetricKeySizesInBytes = { 16, 24, 32 };
    private const int DefaultIterations = 10000;
    private const int SaltSize = 16; // AES 默认的 IV 长度为 16 字节

    /// <summary>
    /// 使用提供的对称密钥加密文本
    /// </summary>
    /// <param name="text">要加密的文本</param>
    /// <param name="skey">密钥字符串</param>
    /// <returns>加密后的Base64字符串（包含IV和密文）</returns>
    public static string? Encrypt(string text, string skey)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(skey)) return null;

        byte[] keyBytes = Encoding.UTF8.GetBytes(skey);
        return Encrypt(text, keyBytes);
    }

    /// <summary>
    /// 使用提供的对称密钥解密文本
    /// </summary>
    /// <param name="cipherText">加密后的Base64字符串（包含IV和密文）</param>
    /// <param name="skey">密钥字符串</param>
    /// <returns>解密后的原始文本</returns>
    public static string Decrypt(string cipherText, string skey)
    {
        if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(skey)) return null!;

        byte[] keyBytes = Encoding.UTF8.GetBytes(skey);
        return Decrypt(cipherText, keyBytes);
    }

    /// <summary>
    /// 使用提供的对称密钥加密文本
    /// </summary>
    /// <param name="text">要加密的文本</param>
    /// <param name="skey">密钥字节数组</param>
    /// <returns>加密后的Base64字符串（包含IV和密文）</returns>
    public static string? Encrypt(string text, byte[] skey)
    {
        if (string.IsNullOrEmpty(text) || skey == null || skey.Length == 0) return null;

        using var aesAlg = Aes.Create();
        aesAlg.Key = AdjustKeySize(skey, 32);

        // 为此次加密操作生成一个新的IV
        aesAlg.GenerateIV();

        using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(text);
        }

        // 将IV和加密后的数据组合在一起
        var combinedIvAndCipherText = new byte[aesAlg.IV.Length + msEncrypt.ToArray().Length];
        Buffer.BlockCopy(aesAlg.IV, 0, combinedIvAndCipherText, 0, aesAlg.IV.Length);
        Buffer.BlockCopy(msEncrypt.ToArray(), 0, combinedIvAndCipherText, aesAlg.IV.Length, msEncrypt.ToArray().Length);

        // 返回包含IV和密文的Base64编码字符串
        return Convert.ToBase64String(combinedIvAndCipherText);
    }

    /// <summary>
    /// 使用提供的对称密钥解密文本
    /// </summary>
    /// <param name="cipherText">加密后的Base64字符串（包含IV和密文）</param>
    /// <param name="skey">密钥字节数组</param>
    /// <returns>解密后的原始文本</returns>
    public static string Decrypt(string cipherText, byte[] skey)
    {
        try
        {
            if (string.IsNullOrEmpty(cipherText) || skey == null || skey.Length == 0) return null!;

            // 将Base64字符串转换回字节数组
            var fullCipher = Convert.FromBase64String(cipherText);

            // 提取IV（前16个字节）
            var iv = new byte[16]; // AES 默认的 IV 长度为 16 字节
                                   // 提取加密后的数据（除去IV的部分）
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using var aesAlg = Aes.Create();
            aesAlg.Key = AdjustKeySize(skey, 32);
            aesAlg.IV = iv;

            // 创建解密器并读取解密后的文本
            using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("无效的Base64字符串格式", nameof(cipherText), ex);
        }
        catch (CryptographicException ex)
        {
            throw new ArgumentException("解密过程中发生错误，请检查密钥或输入数据", ex);
        }
    }

    /// <summary>
    /// 加密方法，使用提供的对称密钥加密字节数组（例如文件内容）
    /// </summary>
    /// <param name="bytes">源文件 字节数组</param>
    /// <param name="skey">密钥字符串</param>
    /// <param name="keySizeInBytes">密钥大小（字节）</param>
    /// <returns>加密后的字节数组（包含IV和密文）</returns>
    /// <exception cref="ArgumentNullException" />
    public static byte[] Encrypt(byte[] bytes, string skey, int keySizeInBytes = 32)
    {
        if (bytes == null || bytes.Length == 0 || string.IsNullOrEmpty(skey))
            throw new ArgumentNullException(nameof(bytes) + " or " + nameof(skey));

        byte[] keyBytes = Encoding.UTF8.GetBytes(skey);
        return Encrypt(bytes, keyBytes, keySizeInBytes);
    }

    /// <summary>
    /// 解密方法，使用提供的对称密钥解密字节数组
    /// </summary>
    /// <param name="bytes">加密后文件 字节数组（包含IV和密文）</param>
    /// <param name="skey">密钥字符串</param>
    /// <param name="keySizeInBytes">密钥大小（字节）</param>
    /// <returns>解密后的原始字节数组</returns>
    /// <exception cref="ArgumentNullException" />
    public static byte[] Decrypt(byte[] bytes, string skey, int keySizeInBytes = 32)
    {
        if (bytes == null || bytes.Length == 0 || string.IsNullOrEmpty(skey))
            throw new ArgumentNullException(nameof(bytes) + " or " + nameof(skey));

        byte[] keyBytes = Encoding.UTF8.GetBytes(skey);
        return Decrypt(bytes, keyBytes, keySizeInBytes);
    }

    /// <summary>
    /// 加密方法，使用提供的对称密钥加密字节数组（例如文件内容）
    /// </summary>
    /// <param name="bytes">源文件 字节数组</param>
    /// <param name="skey">密钥字节数组</param>
    /// <param name="keySizeInBytes">密钥大小（字节）</param>
    /// <returns>加密后的字节数组（包含IV和密文）</returns>
    /// <exception cref="ArgumentNullException" />
    public static byte[] Encrypt(byte[] bytes, byte[] skey, int keySizeInBytes = 32)
    {
        if (bytes == null || bytes.Length == 0 || skey == null || skey.Length == 0)
            throw new ArgumentNullException(nameof(bytes) + " or " + nameof(skey));

        ValidateKeySize(keySizeInBytes);

        using var aesAlg = Aes.Create();
        aesAlg.KeySize = keySizeInBytes * 8; // 设置密钥大小（位）

        // 生成新的IV
        aesAlg.GenerateIV();

        // 调整密钥大小以匹配指定长度
        aesAlg.Key = AdjustKeySize(skey, keySizeInBytes);

        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Write))
        {
            cryptoStream.Write(bytes, 0, bytes.Length); // 写入要加密的数据
            cryptoStream.FlushFinalBlock(); // 完成加密操作
        }

        // 将IV和加密后的数据组合在一起
        var combinedIvAndCipherText = new byte[aesAlg.IV.Length + memoryStream.ToArray().Length];
        Buffer.BlockCopy(aesAlg.IV, 0, combinedIvAndCipherText, 0, aesAlg.IV.Length);
        Buffer.BlockCopy(memoryStream.ToArray(), 0, combinedIvAndCipherText, aesAlg.IV.Length, memoryStream.ToArray().Length);

        return combinedIvAndCipherText; // 返回包含IV和密文的字节数组
    }

    /// <summary>
    /// 解密方法，使用提供的对称密钥解密字节数组
    /// </summary>
    /// <param name="bytes">加密后文件 字节数组（包含IV和密文）</param>
    /// <param name="skey">密钥字节数组</param>
    /// <param name="keySizeInBytes">密钥大小（字节）</param>
    /// <returns>解密后的原始字节数组</returns>
    public static byte[] Decrypt(byte[] bytes, byte[] skey, int keySizeInBytes = 32)
    {
        if (bytes == null || bytes.Length == 0 || skey == null || skey.Length == 0)
            throw new ArgumentNullException(nameof(bytes) + " or " + nameof(skey));

        ValidateKeySize(keySizeInBytes);

        // 提取IV（前16个字节）
        byte[] iv = new byte[16]; // AES 默认的 IV 长度为 16 字节
                                  // 提取加密后的数据（除去IV的部分）
        byte[] cipher = new byte[bytes.Length - iv.Length];

        Buffer.BlockCopy(bytes, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(bytes, iv.Length, cipher, 0, cipher.Length);

        // 调整密钥大小以匹配指定长度
        byte[] keyBytes = AdjustKeySize(skey, keySizeInBytes);

        using var aesAlg = Aes.Create();
        aesAlg.KeySize = keySizeInBytes * 8; // 设置密钥大小（位）
        aesAlg.Key = keyBytes;
        aesAlg.IV = iv;

        using var memoryStream = new MemoryStream(cipher);
        using var cryptoStream = new CryptoStream(memoryStream, aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Read);
        using var originalStream = new MemoryStream();

        var buffer = new byte[1024];
        int readBytes;

        // 读取解密后的数据并写入到原始流中
        while ((readBytes = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            originalStream.Write(buffer, 0, readBytes);
        }

        return originalStream.ToArray(); // 返回解密后的原始字节数组
    }

    #region 可加盐
    /// <summary>
    /// 使用提供的密码加密文本（带盐）
    /// </summary>
    /// <param name="text">要加密的文本</param>
    /// <param name="password">密码字符串</param>
    /// <returns>加密后的Base64字符串（包含盐、IV和密文）</returns>
    public static string? EncryptWithPassword(string text, string password)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(password)) return null;

        byte[] salt = GenerateSalt();
        byte[] key = DeriveKey(password, salt, 32, DefaultIterations);

        using var aesAlg = Aes.Create();
        aesAlg.Key = key;

        // 为此次加密操作生成一个新的IV
        aesAlg.GenerateIV();

        using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(text);
        }

        // 将盐、IV和加密后的数据组合在一起
        var combinedSaltIvAndCipherText = Combine(salt, aesAlg.IV, msEncrypt.ToArray());

        // 返回包含盐、IV和密文的Base64编码字符串
        return Convert.ToBase64String(combinedSaltIvAndCipherText);
    }

    /// <summary>
    /// 使用提供的密码解密文本（带盐）
    /// </summary>
    /// <param name="cipherText">加密后的Base64字符串（包含盐、IV和密文）</param>
    /// <param name="password">密码字符串</param>
    /// <returns>解密后的原始文本</returns>
    public static string? DecryptWithPassword(string cipherText, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(password)) return null;

            // 将Base64字符串转换回字节数组
            var fullCipher = Convert.FromBase64String(cipherText);

            // 提取盐（前16个字节）
            byte[] salt = ExtractPart(fullCipher, 0, SaltSize);

            // 提取IV（接下来的16个字节）
            byte[] iv = ExtractPart(fullCipher, SaltSize, SaltSize);

            // 提取加密后的数据（除去盐和IV的部分）
            byte[] cipher = ExtractPart(fullCipher, SaltSize * 2, fullCipher.Length - SaltSize * 2);

            byte[] key = DeriveKey(password, salt, 32, DefaultIterations);

            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            // 创建解密器并读取解密后的文本
            using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("无效的Base64字符串格式", nameof(cipherText), ex);
        }
        catch (CryptographicException ex)
        {
            throw new ArgumentException("解密过程中发生错误，请检查密码或输入数据", ex);
        }
    }

    /// <summary>
    /// 使用 PBKDF2 从给定的密码和盐生成指定长度的密钥。
    /// </summary>
    /// <param name="password">用于派生密钥的密码字符串</param>
    /// <param name="salt">用于增加随机性的盐字节数组</param>
    /// <param name="keySizeInBytes">期望的密钥大小（字节）</param>
    /// <param name="iterations">迭代次数，提高计算成本以抵御暴力破解</param>
    /// <returns>派生出的密钥字节数组</returns>
    private static byte[] DeriveKey(string password, byte[] salt, int keySizeInBytes = 32, int iterations = DefaultIterations)
    {
        using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return rfc2898DeriveBytes.GetBytes(keySizeInBytes);
    }

    /// <summary>
    /// 生成一个新的随机盐。
    /// </summary>
    /// <param name="size">盐的大小（字节）</param>
    /// <returns>随机生成的盐字节数组</returns>
    private static byte[] GenerateSalt(int size = SaltSize)
    {
        var salt = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    /// <summary>
    /// 将多个字节数组合并为一个数组。
    /// </summary>
    /// <param name="arrays">要合并的字节数组</param>
    /// <returns>合并后的字节数组</returns>
    private static byte[] Combine(params byte[][] arrays)
    {
        int totalLength = arrays.Sum(array => array.Length);
        byte[] result = new byte[totalLength];
        int offset = 0;

        foreach (byte[] array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }

    /// <summary>
    /// 从源数组中提取一部分字节数组。
    /// </summary>
    /// <param name="source">源字节数组</param>
    /// <param name="startIndex">起始索引</param>
    /// <param name="length">提取长度</param>
    /// <returns>提取的字节数组</returns>
    private static byte[] ExtractPart(byte[] source, int startIndex, int length)
    {
        byte[] part = new byte[length];
        Buffer.BlockCopy(source, startIndex, part, 0, length);
        return part;
    }
    #endregion

    private static void ValidateKeySize(int keySizeInBytes)
    {
        if (!ValidSymmetricKeySizesInBytes.Contains(keySizeInBytes))
        {
            throw new ArgumentException("无效的密钥大小。有效的大小为16、24或32字节。\nInvalid key size. Valid sizes are 16, 24, or 32 bytes.");
        }
    }

    private static byte[] AdjustKeySize(byte[] key, int targetSizeInBytes)
    {
        if (key.Length > targetSizeInBytes) Array.Resize(ref key, targetSizeInBytes); // 如果密钥过长，截断至指定长度
        else if (key.Length < targetSizeInBytes)
        {
            // 如果密钥过短，填充至指定长度
            // 注意：这里简单的填充可能会导致安全性问题，建议使用更安全的填充方式，例如通过哈希函数生成额外的密钥材料。
            Array.Resize(ref key, targetSizeInBytes);
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key); // 使用随机数填充剩余部分
        }

        return key;
    }
}