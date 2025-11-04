using System;
using System.Security.Cryptography;
using System.Text;

namespace MFToolkit.Minecraft.Helpers;

/// <summary>
/// 加密工具类
/// </summary>
public static class CryptoHelper
{
    /// <summary>
    /// 生成SHA1哈希
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>SHA1哈希的十六进制字符串</returns>
    public static string GenerateSha1Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        
        using var sha1 = SHA1.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha1.ComputeHash(inputBytes);
        
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
    
    /// <summary>
    /// 生成MD5哈希
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>MD5哈希的十六进制字符串</returns>
    public static string GenerateMd5Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
    
    /// <summary>
    /// 生成SHA256哈希
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>SHA256哈希的十六进制字符串</returns>
    public static string GenerateSha256Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(inputBytes);
        
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
    
    /// <summary>
    /// 生成UUID（版本3）
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="namespaceId">命名空间ID</param>
    /// <returns>生成的UUID</returns>
    public static Guid GenerateUuidV3(string name, Guid namespaceId)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        
        // 组合命名空间和名称
        var namespaceBytes = namespaceId.ToByteArray();
        var nameBytes = Encoding.UTF8.GetBytes(name);
        
        // 创建MD5哈希
        using var md5 = MD5.Create();
        md5.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
        md5.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
        var hashBytes = md5.Hash;
        
        if (hashBytes == null)
            throw new InvalidOperationException("Failed to generate MD5 hash.");
        
        // 设置UUID版本和变体
        hashBytes[6] = (byte)((hashBytes[6] & 0x0F) | 0x30); // 版本3
        hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80); // RFC4122变体
        
        return new Guid(hashBytes);
    }
    
    /// <summary>
    /// 生成UUID（版本5）
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="namespaceId">命名空间ID</param>
    /// <returns>生成的UUID</returns>
    public static Guid GenerateUuidV5(string name, Guid namespaceId)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        
        // 组合命名空间和名称
        var namespaceBytes = namespaceId.ToByteArray();
        var nameBytes = Encoding.UTF8.GetBytes(name);
        
        // 创建SHA1哈希
        using var sha1 = SHA1.Create();
        sha1.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
        sha1.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
        var hashBytes = sha1.Hash;
        
        if (hashBytes == null || hashBytes.Length < 16)
            throw new InvalidOperationException("Failed to generate SHA1 hash.");
        
        // 取前16字节
        var uuidBytes = new byte[16];
        Array.Copy(hashBytes, uuidBytes, 16);
        
        // 设置UUID版本和变体
        uuidBytes[6] = (byte)((uuidBytes[6] & 0x0F) | 0x50); // 版本5
        uuidBytes[8] = (byte)((uuidBytes[8] & 0x3F) | 0x80); // RFC4122变体
        
        return new Guid(uuidBytes);
    }
    
    /// <summary>
    /// 生成随机UUID（版本4）
    /// </summary>
    /// <returns>生成的UUID</returns>
    public static Guid GenerateUuidV4()
    {
        return Guid.NewGuid();
    }
    
    /// <summary>
    /// 生成离线模式UUID
    /// </summary>
    /// <param name="playerName">玩家名称</param>
    /// <returns>生成的UUID</returns>
    public static Guid GenerateOfflineUuid(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
            throw new ArgumentException("Player name cannot be null or empty.", nameof(playerName));
        
        // Minecraft离线模式UUID使用版本3，命名空间为"OfflinePlayer:"
        var namespaceId = new Guid("00000000-0000-0000-0000-000000000000");
        return GenerateUuidV3($"OfflinePlayer:{playerName}", namespaceId);
    }
}
