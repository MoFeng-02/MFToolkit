namespace MFToolkit.Avaloniaui.Platform.AppInfo;

/// <summary>
/// 表示有关应用程序的信息。
/// </summary>
public interface IAppInfo
{
    /// <summary>
    /// 获取应用程序包名称或标识符。
    /// </summary>
    /// <remarks>在Android和iOS上，这是应用程序包的名称。在Windows上，这是应用程序GUID。</remarks>
    string PackageName { get; }

    /// <summary>
    /// 获取应用程序名称。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取应用程序版本作为字符串表示形式。
    /// </summary>
    string VersionString { get; }

    /// <summary>
    /// 获取作为对象<see cref="Version"/>的应用程序版本 
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// 获取应用程序构建号。
    /// </summary>
    string BuildString { get; }

    /// <summary>
    /// 打开此应用程序的设置菜单或页面。
    /// </summary>
    void ShowSettingsUI();

    /// <summary>
    /// 获取系统或应用程序的检测主题。
    /// </summary>
    /// <remarks>对于不支持主题的平台或平台版本， <see cref="AppTheme.Unspecified"/>返回。</remarks>
    AppTheme RequestedTheme { get; }

    /// <summary>
    /// 获取此应用程序的打包模型
    /// </summary>
    /// <remarks> 在Windows以外的其他平台上，这种情况总是会出现 <see cref="AppPackagingModel.Packaged"/>.</remarks>
    AppPackagingModel PackagingModel { get; }

    /// <summary>
    /// 获取系统或应用程序的请求布局方向
    /// </summary>
    LayoutDirection RequestedLayoutDirection { get; }
}
/// <summary>
/// 表示有关应用程序的信息。
/// </summary>
public static class AppInfo
{
    /// <summary>
    /// Gets the application package name or identifier.
    /// </summary>
    /// <remarks>On Android and iOS, this is the application package name. On Windows, this is the application GUID.</remarks>
    public static string PackageName => Current.PackageName;

    /// <summary>
    /// Gets the application name.
    /// </summary>
    public static string Name => Current.Name;

    /// <summary>
    /// Gets the application version as a string representation.
    /// </summary>
    public static string VersionString => Current.VersionString;

    /// <summary>
    /// Gets the application version as a <see cref="Version"/> object.
    /// </summary>
    public static Version Version => Current.Version;

    /// <summary>
    /// Gets the application build number.
    /// </summary>
    public static string BuildString => Current.BuildString;

    /// <summary>
    /// Open the settings menu or page for this application.
    /// </summary>
    public static void ShowSettingsUI() => Current.ShowSettingsUI();

    /// <summary>
    /// Gets the detected theme of the system or application.
    /// </summary>
    /// <remarks>For platforms or platform versions which do not support themes, <see cref="AppTheme.Unspecified"/> is returned.</remarks>
    public static AppTheme RequestedTheme => Current.RequestedTheme;

    /// <summary>
    /// Gets the packaging model of this application.
    /// </summary>
    /// <remarks>On other platforms than Windows, this will always return <see cref="AppPackagingModel.Packaged"/>.</remarks>
    public static AppPackagingModel PackagingModel => Current.PackagingModel;

    /// <summary>
    /// Gets the requested layout direction of the system or application.
    /// </summary>
    public static LayoutDirection RequestedLayoutDirection => Current.RequestedLayoutDirection;

    static IAppInfo? currentImplementation;

    /// <summary>
    /// Provides the default implementation for static usage of this API.
    /// </summary>
    public static IAppInfo Current =>
        currentImplementation ??= new AppInfoImplementation();

    internal static void SetCurrent(IAppInfo? implementation) =>
        currentImplementation = implementation;
}

/// <summary>
/// 描述Windows应用程序的打包选项。
/// </summary>
public enum AppPackagingModel
{
    /// <summary>该应用程序是打包的，可以通过MSIX或商店分发。</summary>
    Packaged,

    /// <summary>该应用程序是未打包的，可以作为可执行文件的集合分发。</summary>
    Unpackaged,
}