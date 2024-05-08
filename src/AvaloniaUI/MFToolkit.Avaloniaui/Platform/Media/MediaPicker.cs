using MFToolkit.Avaloniaui.Platform.Storage;

namespace MFToolkit.Avaloniaui.Platform.Media;

/// <summary>
/// MediaPicker API允许用户在设备上挑选或拍摄照片或视频。
/// </summary>
public interface IMediaPicker
{
    /// <summary>
    /// 获取一个值，该值指示此设备上是否支持捕获媒体。
    /// </summary>
    bool IsCaptureSupported { get; }

    /// <summary>
    /// 打开媒体浏览器，选择一张照片。
    /// </summary>
    /// <param name="options">选择要使用的选项。</param>
    /// <returns>一个 <see cref="FileResult"/> 对象，其中包含所选照片的详细信息。当操作被用户取消时，这个将返回 <see langword="null"/>值</returns>
    Task<FileResult?> PickPhotoAsync(MediaPickerOptions? options = null);

    /// <summary>
    /// 打开相机拍照。
    /// </summary>
    /// <param name="options">选择要使用的选项。</param>
    /// <returns>一个 <see cref="FileResult"/> 对象，其中包含捕获的照片的详细信息。当操作被用户取消时，这个将返回 <see langword="null"/>值</returns>
    Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null);

    /// <summary>
    /// 打开媒体浏览器选择视频。
    /// </summary>
    /// <param name="options">选择要使用的选项。</param>
    /// <returns>一个 <see cref="FileResult"/> 对象，其中包含所选视频的详细信息。当操作被用户取消时，这个将返回 <see langword="null"/>值</returns>
    Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null);

    /// <summary>
    /// 打开摄像头拍摄视频。
    /// </summary>
    /// <param name="options">选择要使用的选项。</param>
    /// <returns>一个 <see cref="FileResult"/> 对象，其中包含捕获的视频的详细信息。当操作被用户取消时，这个将返回 <see langword="null"/>值</returns>
    Task<FileResult?> CaptureVideoAsync(MediaPickerOptions? options = null);
}

/// <summary>
/// MediaPicker API允许用户在设备上挑选或拍摄照片或视频。
/// </summary>
public class MediaPicker : IMediaPicker
{
    static IMediaPicker? _defaultImplementation;

    /// <summary>
    /// 提供此API静态使用的默认实现。
    /// </summary>
    /// <exception cref="Exception">异常，无任何实现</exception>
    public static IMediaPicker Default =>
        _defaultImplementation ?? throw new NotImplementedException("未提供任何实现");

    protected MediaPicker() =>
        _defaultImplementation ??= this;

    public bool IsCaptureSupported { get; init; } = false;

    public virtual Task<FileResult?> PickPhotoAsync(MediaPickerOptions? options = null)
    {
        throw new NotImplementedException("未实现方法：PickPhoto");
    }

    public Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null)
    {
        throw new NotImplementedException("未实现方法：CapturePhoto");
    }

    public Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null)
    {
        throw new NotImplementedException("未实现方法：PickVideo");
    }

    public Task<FileResult?> CaptureVideoAsync(MediaPickerOptions? options = null)
    {
        throw new NotImplementedException("未实现方法：CaptureVideo");
    }
}

/// <summary>
/// 选择从设备中拾取媒体的选项。
/// </summary>
public class MediaPickerOptions
{
    /// <summary>
    /// 获取或设置选择媒体时显示的标题。
    /// </summary>
    /// <remarks>本标题不能保证在所有操作系统上显示。</remarks>
    public string? Title { get; set; }
}