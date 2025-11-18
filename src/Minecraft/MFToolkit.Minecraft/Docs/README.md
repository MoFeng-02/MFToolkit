# Minecraft 启动器核心功能包说明文档

> **推荐开发环境**：Visual Studio 2026
>
> **技术栈**：C# 14.0 + .NET 10.0

## 📝 免责声明
本软件包为 Minecraft 第三方启动器功能包，与 Minecraft、Mojang、Microsoft 无任何官方关联。

## 🚀 快速开始
1.  **环境准备**: 确保已安装 Visual Studio 2026（或 Rider 2025.3 以上版本）和 .NET 10.0 SDK
2.  **获取代码**: `git clone https://github.com/MoFeng-02/MFToolkit.git`
3.  **定位项目**: 在 `src/Minecraft/MFToolkit.Minecraft` 目录下找到项目文件
4.  **打开项目**: 推荐使用 VS2026 打开解决方案文件进行开发

## 💡 功能模块

### 核心功能
- **启动核心** - 游戏启动器核心逻辑处理
- **Java 环境管理** - 自动处理游戏所需的 Java 环境
  - 默认支持开源 Java 运行时（如 Microsoft Build of OpenJDK，同时支持 Eclipse Temurin、Oracle OpenJDK 等主流开源发行版）
  - 自动 Java 版本检测与兼容性验证
  - 无需用户手动安装配置 Java
- **下载核心** - 资源文件多线程下载管理系统
- **账号管理** - 用户账号认证（支持 Microsoft/OAuth2）、会话管理与安全验证

## 💻 系统要求

### 开发环境
- **主要IDE**: Visual Studio 2026（推荐，提供完整的 C# 14.0 特性支持）
- **可选IDE**: JetBrains Rider 2025.3 及以上版本
- **框架**: .NET 10.0
- **语言**: C# 14.0 编译器

### 硬件建议
| 配置等级 | 内存 | 体验说明 |
|---------|---------|----------|
| **基础配置** | 16GB | 运行流畅，UI体验良好 |
| **推荐配置** | 64GB | 官方推荐，最佳性能 |

## ⚠️ 注意事项
- 确保开发环境满足上述要求
- 使用 VS2026 以获得完整的 C# 14.0 特性支持
- 建议在推荐配置下进行开发以获得最佳体验
- **Java 环境建议**：默认使用开源 Java 分发版以避免授权问题