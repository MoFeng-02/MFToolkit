# MFToolkit.Minecraft

一个全面的Minecraft认证和档案管理库，适用于.NET 9。

## 特性 (Features)

- **认证系统**: 支持离线模式、微软账号、Xbox账号和第三方账号认证
- **档案管理**: 获取和更新玩家档案，更改用户名
- **皮肤与披风管理**: 上传和管理皮肤与披风
- **依赖注入**: 内置依赖注入支持
- **空值安全**: 完整的空值安全支持
- **AOT兼容**: 兼容.NET Native AOT
- **JSON序列化**: 使用源生成的自定义JSON序列化

## 安装 (Installation)

```bash
dotnet add package MFToolkit.Minecraft
```

## 使用方法 (Usage)

### 基本设置 (Basic Setup)

```csharp
using Microsoft.Extensions.DependencyInjection;
using MFToolkit.Minecraft.Extensions;

var services = new ServiceCollection();
services.AddMinecraftServices(options =>
{
    options.ClientId = "your-client-id";
    options.RedirectUri = "https://your-redirect-uri";
    options.ClientSecret = "your-client-secret"; // Optional
});

var serviceProvider = services.BuildServiceProvider();
```

### 离线认证 (Offline Authentication)

```csharp
using MFToolkit.Minecraft.Services.Auth.Interfaces;
using MFToolkit.Minecraft.Entities.Account;

var offlineAuthService = serviceProviderProvider.GetServiceProvider.GetRequiredService<IOfflineAuthService>();

// Create offline account
var account = await offlineAuthService.CreateOfflineAccountAsync("PlayerName");

// Login
await offlineAuthService.LoginAsync(account);

// Create session
var session = await offlineAuthService.CreateSessionAsync(account, "ServerId");
```

### 微软认证 (Microsoft Authentication)

MFToolkit.Minecraft提供两种微软认证方式：授权码流和设备流（推荐）。

#### 授权码流 (Authorization Code Flow)

```csharp
using MFToolkit.Minecraft.Services.Auth.Interfaces;

var officialAuthService = serviceProvider.GetRequiredService<IOfficialAuthService>();

// 获取授权URL
var state = Guid.NewGuid().ToString();
var authUrl = officialAuthService.GetMicrosoftAuthorizationUrl(state);

// 将用户重定向到authUrl并获取code...

// 使用code登录
var account = await officialAuthService.LoginWithMicrosoftCodeAsync(code);

// 创建会话
var session = await officialAuthService.CreateSessionAsync(account, "ServerId");
```

#### 设备流 (Device Flow 推荐)

设备流适用于没有浏览器的设备，用户需要在另一台设备上完成登录。

```csharp
using MFToolkit.Minecraft.Services.Auth.Interfaces;
using MFToolkit.Minecraft.Internal.Auth;

var officialAuthService = serviceProvider.GetRequiredService<IOfficialAuthService>();

try
{
    // 获取设备代码
    var deviceCodeResult = await officialAuthService.GetMicrosoftDeviceCodeAsync();
    
    // 显示给用户
    Console.WriteLine("请在浏览器中打开以下URL:");
    Console.WriteLine(deviceCodeResult.VerificationUri);
    Console.WriteLine($"输入代码: {deviceCodeResult.UserCode}");
    Console.WriteLine("等待用户完成登录...");
    
    // 使用设备代码登录（自动轮询直到完成或超时）
    var account = await officialAuthService.LoginWithMicrosoftDeviceCodeResultAsync(deviceCodeResult);
    
    // 创建会话
    var session = await officialAuthService.CreateSessionAsync(account, "ServerId");
    
    Console.WriteLine($"登录成功！玩家名称: {account.PlayerName}");
}
catch (AuthorizationPendingException)
{
    Console.WriteLine("用户尚未完成登录操作。");
}
catch (AuthorizationDeclinedException)
{
    Console.WriteLine("用户拒绝了授权请求。");
}
catch (ExpiredTokenException)
{
    Console.WriteLine("设备代码已过期，请重新获取。");
}
catch (Exception ex)
{
    Console.WriteLine($"登录失败: {ex.Message}");
}
```

### Xbox认证 (Xbox Authentication)

```csharp
using MFToolkit.Minecraft.Services.Auth.Interfaces;

var officialAuthService = serviceProvider.GetRequiredService<IOfficialAuthService>();

// 使用Xbox令牌登录
var account = await officialAuthService.LoginWithXboxTokenAsync(xboxToken, userHash);

// 创建会话
var session = await officialAuthService.CreateSessionAsync(account, "ServerId");
```

### 档案管理 (Profile Management)

```csharp
using MFToolkit.Minecraft.Services.Profile;

var profileService = serviceProvider.GetRequiredService<IProfileService>();

// Get profile
var profile = await profileService.GetProfileAsync(account);

// Change username
if (await profileService.CheckNameAvailabilityAsync("NewName"))
{
    profile = await profileService.ChangePlayerNameAsync(account, "NewName");
}
```

### 皮肤管理 (Skin Management)

```csharp
using MFToolkit.Minecraft.Services.Skin;

var skinService = serviceProvider.GetRequiredService<ISkinService>();

// Upload skin from file
using var skinStream = File.OpenRead("skin.png");
var skinInfo = await skinService.UploadSkinAsync(account, skinStream, "image/png", isSlim: false);

// Upload skin from URL
var skinInfo = await skinService.UploadSkinFromUrlAsync(account, "https://example.com/skin.png", isSlim: true);
```

### 披风管理 (Cape Management)

```csharp
using MFToolkit.Minecraft.Services.Cape;

var capeService = serviceProvider.GetRequiredService<ICapeService>();

// Upload cape from file
using var capeStream = File.OpenRead("cape.png");
var capeInfo = await capeService.UploadCapeAsync(account, capeStream, "image/png");

// Upload cape from URL
var capeInfo = await capeService.UploadCapeFromUrlAsync(account, "https://example.com/cape.png");
```

## 配置 (Configuration)

### 官方认证 (Official Authentication) Options

```json
{
  "Minecraft:OfficialAuth": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret", // Optional
    "RedirectUri": "https://your-redirect-uri",
    "TimeoutSeconds": 30,
    "MaxRetryCount": 3,
    "RetryDelayMilliseconds": 1000
  }
}
```

### 离线认证 (Offline Authentication) Options

```json
{
  "Minecraft:OfflineAuth": {
    "DefaultPlayerName": "Player",
    "AllowCustomUuid": true,
    "DefaultUuidVersion": 4,
    "SessionExpirationHours": 24,
    "AutoGenerateSkin": true,
    "DefaultSkinType": "classic"
  }
}
```

### 皮肤 (Skin) Options

```json
{
  "Minecraft:Skin": {
    "UploadTimeoutSeconds": 60,
    "MaxSizeBytes": 2097152, // 2MB
    "AllowedContentTypes": ["image/png", "image/jpeg"],
    "AllowedWidths": [64, 128],
    "AllowedHeights": [64, 128],
    "DefaultModel": "classic",
    "AllowSkinDeletion": true
  }
}
```

## 许可证 (License)

本项目基于MIT许可证开源 - 详见[LICENSE](LICENSE)文件。

## 正版验证方式总结

MFToolkit.Minecraft提供两种正版验证方式：

1. **授权码流 (Authorization Code Flow)**
   - 适用于有浏览器的设备
   - 用户需要在同一设备上完成登录
   - 需要配置重定向URL

2. **设备流 (Device Flow)**
   - 适用于没有浏览器的设备（如服务器、IoT设备）
   - 用户需要在另一台设备上完成登录
   - 不需要配置重定向URL
   - 更适合命令行应用和嵌入式设备

两种方式都支持完整的Minecraft账号功能，包括皮肤和披风管理、多人游戏等。
