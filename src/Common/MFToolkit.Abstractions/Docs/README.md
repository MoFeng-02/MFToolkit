# MFToolkit.Abstractions - 依赖注入特性定义库

## 概述

本库提供了一套完整的依赖注入特性定义，用于标记需要自动注入的服务类。仅支持 .NET 9.0 及以上版本。

## 安装要求

```xml
<!-- 必须搭配 MFToolkit.AutoGenerator 使用 -->
<PackageReference Include="MFToolkit.Abstractions" Version="1.0.20" />
<PackageReference Include="MFToolkit.AutoGenerator" Version="1.0.20" />
```

## 核心特性

### AutoInjectAttribute 

```csharp
// 不推荐使用，建议使用具体的生命周期特性
```

### 具体生命周期特性

- **SingletonAttribute** - 单例服务
- **ScopedAttribute** - 作用域服务
- **TransientAttribute** - 瞬态服务
- **TrySingletonAttribute** - TryAdd 单例服务
- **TryScopedAttribute** - TryAdd 作用域服务
- **TryTransientAttribute** - TryAdd 瞬态服务

## 使用示例

### 基础用法

```csharp
// 泛型形式 - 推荐
[Singleton<IMyService>]
public class MyService : IMyService { }

[Scoped<IRepository>]
public class Repository : IRepository { }

// 非泛型形式
[Singleton(typeof(IMyService))]
public class MyService : IMyService { }
```

### 带 Key 的注入

```csharp
// 字符串 Key
[Singleton<IMyService>("service1")]
public class MyService1 : IMyService { }

[Singleton<IMyService>("service2")]
public class MyService2 : IMyService { }

// Type 作为 Key（特殊规则）
[Singleton(typeof(string))] // 如果类不继承 string，typeof(string) 会被当作 Key
public class StringKeyService { }
```

### 服务名称自定义

```csharp
[AutoInjectServiceName("AddMyCustomServices")]
[Singleton<IMyService>]
public class MyService : IMyService { }
```

## 重要规则说明

### Type 参数的特殊处理规则：

- 如果指定的 Type 是当前类的基类或实现的接口 → 作为服务类型
- 如果指定的 Type 不是有效的服务类型 → 作为服务 Key

### 示例说明：

```csharp
// ✅ 正确：IMyService 是当前类实现的接口 → 作为服务类型
[Singleton(typeof(IMyService))]
public class MyService : IMyService { }

// 🔑 特殊：string 不是当前类的基类 → typeof(string) 作为 Key
[Singleton(typeof(string))]
public class MyService { }
```