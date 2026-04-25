

# MFToolkit


## 概述

MFToolkit 是一个专为 .NET 9+（部分支持 .NET 8.0）设计的现代化工具库，提供丰富的常用工具类和组件，帮助开发者快速构建各类应用程序。（非常抱歉，大部分API已经不再更新，各个小程序（包括公众号）端暂时不再支持，Avalonia【Avalonia端的Routing库已经迁移出，不再绑定UI框架，可放心使用】、maui端暂时不会再更新）

## 主要特性

### 🎯 核心工具库 (MFToolkit)
- **扩展方法**: 字符串、日期时间、类型转换、集合操作等常用扩展
- **文件管理**: 文件上传、下载、接收等完整解决方案
- **HTTP 客户端**: 封装 HttpClient，提供便捷的 HTTP 请求功能
- **JSON 处理**: 高性能 JSON 序列化/反序列化，支持 AOT 编译
- **数据库**: Entity Framework Core 集成，Repository 模式支持
- **加密解密**: AES、MD5、RSA、SM3 等多种加密算法
- **日志系统**: 本地文件日志（MFLogger）、Serilog、NLog 支持
- **网络通信**: SignalR 客户端、gRPC、TCP 服务器
- **验证工具**: 邮箱、手机号、身份证、银行卡等常用验证
- **实用工具**: 金额计算、雪花 ID 生成器、二维码生成、Excel 操作等
- **异常处理**: 统一异常类型和异常抛出辅助类
- **偏好设置**: 跨平台本地存储解决方案

### 🔄 依赖注入
- **自动注入生成器**: 通过源码生成实现自动依赖注入
- **多种生命周期**: 支持 Singleton、Scoped、Transient
- **Keyed Services**: 支持键值服务注册

### 🛤️ 路由框架 (MFToolkit.Routing)
- 轻量级路由解决方案
- 支持路由守卫
- 页面生命周期管理
- AOT 兼容

### 🌐 微信小程序集成
- 多平台支持（微信、支付宝、百度、字节等）
- 统一请求服务
- 配置管理

### 🎨 UI 组件库
- **AvaloniaUI**: MFButton、MFCell、MFTabbar、MFLoading 等 Material 风格组件
- **MAUI**: 基础组件支持
- **路由导航**: 完整的页面导航解决方案

### 🎮 Minecraft 启动器 (MFToolkit.Minecraft) （待继续开发完善）
- 微软账号认证
- 离线模式支持
- 版本管理
- 游戏下载
- 启动参数配置

### 🔐 认证授权
- JWT 认证
- 统一响应结果处理
- 全局异常处理中间件

## 快速开始

### 安装

通过 NuGet 包管理器安装：

```powershell
# 核心工具库
Install-Package MFToolkit

# 自动注入生成器
Install-Package MFToolkit.AutoGenerator

# 依赖注入抽象
Install-Package MFToolkit.Abstractions

# 路由框架
Install-Package MFToolkit.Routing

# AvaloniaUI 组件
Install-Package MFToolkit.Avaloniaui
Install-Package MFToolkit.Avaloniaui.Material
```

或使用 dotnet CLI：

```bash
dotnet add package MFToolkit
dotnet add package MFToolkit.AutoGenerator
dotnet add package MFToolkit.Abstractions
dotnet add package MFToolkit.Routing
```

### 基本使用

#### 1. 在 ASP.NET Core 中使用

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 添加 MFToolkit 服务
builder.Services.AddInjectMFAppService();

// 添加自动注入生成器服务
builder.Services.AddGeneratorDemoServices();

var app = builder.Build();

app.Run();
```

#### 2. 使用自动注入

```csharp
using MFToolkit.Abstractions.DependencyInjection;

// 定义服务
[Singleton]
public class MyService : IMyService
{
    public void DoSomething()
    {
        Console.WriteLine("Hello MFToolkit!");
    }
}

// 自动生成扩展方法
public static partial class AutoInjectExtensions
{
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection 
        AddAutoInjectServices(
            this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        // 自动生成的服务注册代码
        return services.AddSingleton<IMyService, MyService>();
    }
}
```

#### 3. 使用工具类

```csharp
using MFToolkit.Extensions;
using MFToolkit.Json.Extensions;

// 字符串操作
var email = "test@example.com";
bool isValid = email.IsValidEmail();

// JSON 序列化
var obj = new { Name = "MFToolkit", Version = "1.0" };
string json = obj.ValueToJson();

// JSON 反序列化
var result = json.JsonToValue<MyClass>();

// 验证工具
bool isValidPhone = Validator.IsValidChinaMobilePhoneNumber("13800138000");
bool isValidIdCard = Validator.IsValidIDCard("110101199001011234");

// 加密
string encrypted = "Hello".ToEncryptionPassword();
string md5Hash = "password".ToMD5Encrypt();
```

#### 4. 使用路由导航

```csharp
using MFToolkit.Routing;
using MFToolkit.Routing.Entities;

// 注册路由
var services = new ServiceCollection();
services.AddRouting(routes => 
{
    routes.AddRoute<HomePage>("/");
    routes.AddRoute<DetailPage>("/detail/{id}");
});

// 导航
var router = serviceProvider.GetRequiredService<IRouter>();
await router.NavigateAsync("/detail/123");
```

## 项目结构

```
MFToolkit/
├── src/
│   ├── Common/
│   │   ├── MFToolkit/                 # 核心工具库
│   │   ├── MFToolkit.Abstractions/    # 依赖注入抽象
│   │   ├── MFToolkit.AutoGenerator/   # 自动注入生成器
│   │   ├── MFToolkit.Routing/         # 路由框架
│   │   ├── MFToolkit.AspNetCore/      # ASP.NET Core 扩展
│   │   └── ...
│   ├── AvaloniaUI/
│   │   ├── MFToolkit.Avaloniaui/           # 基础组件
│   │   └── MFToolkit.Avaloniaui.Material/  # Material 风格组件
│   ├── MAUI/
│   │   └── MFToolkit.Maui/            # MAUI 支持
│   ├── Applet/
│   │   ├── WeChat/                    # 微信支持
│   │   └── Integration/               # 小程序集成
│   └── Minecraft/
│       └── MFToolkit.Minecraft/       # Minecraft 启动器
├── DemoDesktop/                       # Avalonia UI 示例
├── GeneratorDemo/                     # 生成器示例
└── docs/                              # 文档
```

## 更新日志

### 2025/3/9
- 提供 SqlSugarExtension 实体操作拓展类

### 2025/3/3
- 提供第一版自动生成器，用于依赖注入

## 许可证

本项目基于 MIT 协议开源。

## 贡献

欢迎提交 Issue 和 Pull Request！

## 联系方式

- Gitee: https://gitee.com/MoFeng-02/MFToolkit
- GitHub: https://github.com/MoFeng-02/MFToolkit