using System.IO.Compression;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace MFToolkit.Minecraft.Helpers;

/// <summary>
/// Minecraft 原生库提取工具，仅提取当前系统所需的原生文件（.dll/.so/.dylib 等）
/// </summary>
public class NativeExtractorHelper
{
    // 当前系统对应的原生文件扩展名（仅提取这些类型）
    private static readonly HashSet<string> TargetExtensions;

    static NativeExtractorHelper()
    {
        // 根据当前系统初始化需要提取的文件扩展名
        TargetExtensions = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new HashSet<string> { ".dll" }
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new HashSet<string> { ".so" }
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? new HashSet<string> { ".dylib" }
                    : new HashSet<string>();
    }

    /// <summary>
    /// 从 ZIP 文件中提取当前系统所需的原生文件（仅提取目标扩展名的文件）
    /// </summary>
    /// <param name="zipFilePath">ZIP 文件路径</param>
    /// <param name="extractPath">解压目标路径（所有文件直接放在该目录下，不保留 ZIP 内部目录结构）</param>
    /// <param name="overwrite">是否覆盖已存在的文件</param>
    /// <param name="cancellationToken">取消令牌</param>
    public static async Task ExtractNativesAsync(string zipFilePath, string extractPath, bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(zipFilePath))
            throw new ArgumentException("ZIP 文件路径不能为空", nameof(zipFilePath));
        if (string.IsNullOrWhiteSpace(extractPath))
            throw new ArgumentException("解压路径不能为空", nameof(extractPath));
        if (!File.Exists(zipFilePath))
            throw new FileNotFoundException("ZIP 文件不存在", zipFilePath);

        Directory.CreateDirectory(extractPath);

        await using var fileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            4096, useAsync: true);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

        await ExtractTargetNativesAsync(archive, extractPath, overwrite, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 从流中提取当前系统所需的原生文件
    /// </summary>
    public static async Task ExtractNativesFromStreamAsync(Stream zipStream, string extractPath, bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        if (zipStream == null)
            throw new ArgumentNullException(nameof(zipStream));
        if (!zipStream.CanRead)
            throw new ArgumentException("流不可读", nameof(zipStream));
        if (string.IsNullOrWhiteSpace(extractPath))
            throw new ArgumentException("解压路径不能为空", nameof(extractPath));

        Directory.CreateDirectory(extractPath);

        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        await ExtractTargetNativesAsync(archive, extractPath, overwrite, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 核心逻辑：仅提取当前系统需要的原生文件，忽略目录结构
    /// </summary>
    private static async Task ExtractTargetNativesAsync(ZipArchive archive, string extractPath, bool overwrite,
        CancellationToken cancellationToken)
    {
        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 跳过目录条目（无文件名的条目）
            if (string.IsNullOrWhiteSpace(entry.Name))
                continue;

            // 获取文件扩展名（小写）
            var extension = Path.GetExtension(entry.Name).ToLowerInvariant();

            // 只提取当前系统对应的原生文件类型
            if (!TargetExtensions.Contains(extension))
                continue;

            // 目标路径：直接放在 extractPath 下，不保留 ZIP 内部的目录结构
            // （例如 ZIP 中的 "META-INF/native/libxxx.so" 仅提取为 "extractPath/libxxx.so"）
            var targetFilePath = Path.Combine(extractPath, entry.Name);

            // 检查是否需要覆盖
            if (File.Exists(targetFilePath) && !overwrite)
                continue;

            // 解压文件
            await ExtractSingleNativeAsync(entry, targetFilePath, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 提取单个原生文件并设置权限
    /// </summary>
    private static async Task ExtractSingleNativeAsync(ZipArchiveEntry entry, string targetFilePath,
        CancellationToken cancellationToken)
    {
        // 打开 ZIP 条目流
        await using var entryStream = entry.Open();
        // 创建目标文件流（覆盖模式）
        await using var targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write,
            FileShare.None, 4096, useAsync: true);

        // 异步复制内容
        await entryStream.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);

        // 对 Unix 系统的原生文件设置执行权限
        SetUnixExecutePermissionIfNeeded(targetFilePath);
    }

    /// <summary>
    /// 为 Unix 系统的原生文件设置执行权限（.so/.dylib 需要可执行权限才能加载）
    /// </summary>
    private static void SetUnixExecutePermissionIfNeeded(string filePath)
    {
        if (OperatingSystem.IsWindows())
            return; // Windows 不需要执行权限标记

        try
        {
            var currentMode = File.GetUnixFileMode(filePath);
            // 添加用户执行权限（保留已有权限）
            var newMode = currentMode | UnixFileMode.UserExecute;
            if (newMode != currentMode)
            {
                File.SetUnixFileMode(filePath, newMode);
            }
        }
        catch (PlatformNotSupportedException)
        {
            // 非 Unix 系统忽略
        }
        catch (IOException ex)
        {
            // 记录权限设置失败（不影响文件提取，但可能导致运行时加载失败）
            System.Diagnostics.Debug.WriteLine($"警告：无法为 {filePath} 设置执行权限：{ex.Message}");
        }
    }

    /// <summary>
    /// 计算文件的 SHA1 哈希（Minecraft 常用 SHA1 验证文件完整性）
    /// </summary>
    public static async Task<string> CalculateSha1HashAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("文件不存在", filePath);

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        using var sha1 = SHA1.Create();
        var hashBytes = await sha1.ComputeHashAsync(stream, cancellationToken: default).ConfigureAwait(false);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// 验证文件完整性（与 Minecraft 元数据中的 SHA1 比对）
    /// </summary>
    public static async Task<bool> VerifyIntegrityAsync(string filePath, string expectedSha1)
    {
        if (string.IsNullOrWhiteSpace(expectedSha1))
            return true; // 无预期哈希时默认视为有效

        var actualSha1 = await CalculateSha1HashAsync(filePath).ConfigureAwait(false);
        return actualSha1.Equals(expectedSha1, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 清理目标目录中的所有原生文件（仅删除目标扩展名的文件，保留目录结构）
    /// </summary>
    public static void CleanNativeDirectory(string extractPath)
    {
        if (!Directory.Exists(extractPath))
            return;

        foreach (var ext in TargetExtensions)
        {
            var files = Directory.EnumerateFiles(extractPath, $"*{ext}", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"无法删除原生文件 {file}：{ex.Message}");
                }
            }
        }
    }
}