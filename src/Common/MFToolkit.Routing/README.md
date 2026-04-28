# MFToolkit.Routing 使用指南

> 框架无关的 .NET 路由导航库，支持 Avalonia、WPF、MAUI 等 UI 框架

---

## 目录

- [快速开始](#快速开始)
  - [安装](#1-安装)
  - [定义页面](#2-定义页面)
  - [配置 DI 并注册路由](#3-配置-di-并注册路由)
  - [在代码中使用导航](#4-在代码中使用导航)
- [核心概念](#核心概念)
  - [RouteEntity（路由实体）](#routeentity路由实体)
  - [RouteEntry（路由条目）](#routeentry路由条目)
  - [RouteStack（路由栈）](#routestack路由栈)
  - [IsTop 与应用模式](#istop-与应用模式)
  - [RouteRegistry（路由注册表）](#routeregistry路由注册表)
  - [RouterOptions（路由选项）](#routeroptions路由选项)
- [路由注册](#路由注册)
  - [方式一：fluent API](#方式一fluent-api)
  - [方式二：直接传入路由集合](#方式二直接传入路由集合)
- [导航操作](#导航操作)
  - [基本导航](#基本导航)
  - [路径参数导航](#路径参数导航)
  - [返回操作](#返回操作)
  - [切换顶级路由](#切换顶级路由)
- [路由守卫](#路由守卫)
  - [定义守卫](#定义守卫)
  - [注册守卫](#注册守卫)
  - [守卫链](#守卫链)
- [生命周期钩子](#生命周期钩子)
  - [INavigationAware 接口](#inavigationaware-接口)
  - [基类方式](#基类方式)
  - [生命周期时序](#生命周期时序)
- [事件通知](#事件通知)
  - [NavigationEventArgs 属性](#navigationeventargs-属性)
  - [NavigationActions 导航动作常量](#navigationactions-导航动作常量)
  - [UI 框架集成示例](#ui-框架集成示例)
- [ViewModel 支持](#viewmodel-支持)
  - [注册路由时指定 ViewModel](#注册路由时指定-viewmodel)
  - [ViewModel 实现 IQueryAttributable](#viewmodel-实现-iqueryattributable)
  - [生命周期推断规则](#生命周期推断规则)
  - [框架层集成](#框架层集成)
  - [职责划分](#职责划分)
- [高级用法](#高级用法)
  - [KeepAlive 页面缓存](#keepalive-页面缓存)
  - [默认顶级路由](#默认顶级路由)
  - [嵌套路由](#嵌套路由)
  - [AOT 兼容性](#aot-兼容性)
  - [DI 扩展](#di-扩展)
- [常见问题](#常见问题)
- [API 参考](#api-参考)
  - [IRouter](#irouter)
  - [NavigationResult](#navigationresult)
  - [NavigationEventArgs](#navigationeventargs)
  - [NavigationActions](#navigationactions)

---

## 快速开始

### 1. 安装

```bash
dotnet add package MFToolkit.Routing
```

> **⚠️ 重要依赖说明**
>
> `MFToolkit.Routing` 自身仅依赖 `Microsoft.Extensions.DependencyInjection.Abstractions`（接口层），这意味着它可以与任何兼容的 DI 容器配合使用。
>
> 但在**最终启动项目**（如 App、Host 入口项目）中，你必须额外引用完整的 DI 实现包，否则无法构建 `ServiceCollection` 或调用 `BuildServiceProvider()`：
>
> ```bash
> dotnet add package Microsoft.Extensions.DependencyInjection
> ```
>
> 如果你使用的是 `Microsoft.Extensions.Hosting`（`Generic Host`），它已内置 DI 实现，无需单独引用。

### 2. 定义页面

```csharp
// 定义一个支持生命周期钩子的页面
public class HomePage : INavigationAware
{
    public void OnNavigated(Dictionary<string, object?>? parameters)
    {
        Console.WriteLine("HomePage 已激活");
    }

    public void OnNavigatingFrom()
    {
        Console.WriteLine("即将离开 HomePage");
    }

    public void OnNavigatedFrom()
    {
        Console.WriteLine("已离开 HomePage");
    }
}

public class SettingsPage : INavigationAware
{
    public void OnNavigated(Dictionary<string, object?>? parameters)
    {
        var userId = parameters?["userId"];
        Console.WriteLine($"SettingsPage 已激活，参数: {userId}");
    }

    public void OnNavigatingFrom() { }
    public void OnNavigatedFrom() { }
}
```

### 3. 配置 DI 并注册路由

```csharp
// Program.cs 或 Startup.cs
var builder = WebApplication.CreateBuilder(args);

// 添加路由服务
builder.Services.AddRouting();

// 注册路由
builder.Services.AddRoutes(routes =>
{
    routes.Register(new RouteEntity<HomePage>("/home") { IsTop = true });
    routes.Register(new RouteEntity<SettingsPage>("/settings"));
});

var app = builder.Build();
```

### 4. 在代码中使用导航

```csharp
public class MyService
{
    private readonly IRouter _router;

    public MyService(IRouter router)
    {
        _router = router;
    }

    public async Task NavigateToSettings()
    {
        // 方式1：通过路由路径导航
        var result = await _router.NavigateAsync("/settings");

        // 方式2：通过页面类型导航
        var result2 = await _router.NavigateAsync<SettingsPage>();

        // 方式3：带参数导航
        var result3 = await _router.NavigateAsync("/settings", new Dictionary<string, object?>
        {
            ["userId"] = 123,
            ["returnUrl"] = "/home"
        });

        if (result.IsSuccess)
        {
            Console.WriteLine("导航成功");
        }
        else if (result.IsBlocked)
        {
            Console.WriteLine("导航被守卫阻止");
        }
        else if (result.IsNotFound)
        {
            Console.WriteLine("路由未找到");
        }
    }

    public async Task GoBack()
    {
        if (_router.CanGoBack)
        {
            await _router.GoBackAsync();
        }
    }
}
```

---

## 核心概念

### RouteEntity（路由实体）

表示一个路由的配置信息：

```csharp
public class RouteEntity
{
    public Guid Id { get; set; }              // 唯一ID，自动生成
    public string? RoutePath { get; set; }    // 路由路径，如 "/home/settings"
    public string? RouteName { get; set; }   // 显示名称
    public required Type RouteType { get; set; }  // 页面类型（必须）
    public Type? ViewModelType { get; set; } // 视图模型类型（可选）
    public Dictionary<string, object?>? DefaultParameters { get; set; }  // 默认参数
    public bool IsKeepalive { get; set; }     // 是否保持活跃
    public bool IsTop { get; set; }           // 是否为顶级路由
    public bool IsLazy { get; set; }          // 是否懒加载
    public int SortOrder { get; set; }        // 排序权重
    public string RouteKey => RoutePath ?? RouteType.Name;  // 唯一键
}
```

提供多个构造函数方便不同场景使用：

```csharp
// 无参（用于反序列化或手动构建）
var entity = new RouteEntity();

// 通过 Type 构造
var entity = new RouteEntity(typeof(HomePage));
var entity = new RouteEntity(typeof(HomePage), "/home");

// 通过泛型构造（推荐）
var entity = new RouteEntity<HomePage>("/home");

// 泛型 + ViewModel
var entity = new RouteEntity<HomePage, HomeViewModel>("/home");

// 支持 fluent 链式调用
new RouteEntity<HomePage>("/home")
    .SetTop()
    .SetKeepAlive()
    .SetOrder(1);

// 泛型子类提供 SetPath / SetTop / SetKeepAlive / SetOrder 等链式方法
```

> **关于 `required` 修饰符**：所有构造函数均标记了 `[method: SetsRequiredMembers]`，告诉编译器这些构造函数会负责初始化 required 属性。因此即使用对象初始化器语法调用，也不会有警告。

### RouteEntry（路由条目）

表示栈中的一个实际导航记录：

```csharp
public class RouteEntry
{
    public RouteEntity Entity { get; }        // 关联的路由实体
    public Dictionary<string, object?>? Parameters { get; }  // 当前导航参数
    public DateTime NavigatedAt { get; }      // 导航时间
    public object? PageInstance { get; set; } // 页面实例（框架侧填充）
    public object? ViewModelInstance { get; set; } // 视图模型实例（Router 创建）
    public bool IsActivated { get; set; }     // 是否已激活
}
```

### RouteStack（路由栈）

每个顶级路由拥有独立的栈：

```
顶级路由 A 的栈:  [PageA1] → [PageA2] → [PageA3] (当前)
顶级路由 B 的栈:  [PageB1] → [PageB2] (当前)
```

父子关系由栈历史自动确定：

```csharp
// 获取当前路由的父路由（栈中上一个条目）
var parent = router.CurrentStack.Parent;

// 获取完整祖先链（从根到父）
var ancestors = router.CurrentStack.GetAncestors();
```

### IsTop 与应用模式

`IsTop = true` 决定了**应用的整体导航模式**：

| IsTop 数量 | 应用类型 | 行为 |
|-----------|---------|------|
| 0 或 1 | **单页应用（SPA）** | 单一栈，所有导航 Push/Pop 在里面 |
| ≥2 | **多页应用（多窗口）** | 每个 IsTop 有独立栈，`NavigateAsync` 到 IsTop = 切换栈 |

**单页应用（SPA）示例：** 0 或 1 个 IsTop，只有一条栈

```
栈: [Home] → [Detail] → [Settings]  ← Push/Pop 都在这里
```

**多页应用示例：** 2+ 个 IsTop，每个有独立栈

```
顶级路由 A 的栈:  [A1] → [A2] → [A3]  ← 当前
顶级路由 B 的栈:  [B1] → [B2]          ← 切换过来直接到 B2

NavigateAsync("/b") → 切换到 B 栈，激活 B2
NavigateAsync("/a") → 切换回 A 栈，激活 A3
```

> **注意**：顶级路由之间的切换会**保留原栈状态**，切回来时仍停留在上次的位置。

### RouteRegistry（路由注册表）

路由注册表，用于以 fluent 方式注册路由：

```csharp
builder.Services.AddRoutes(routes =>
{
    routes.Register(new RouteEntity<HomePage>("/home") { IsTop = true });
    routes.Register(new RouteEntity<SettingsPage>("/settings"));
    routes.Register(new RouteEntity<DetailPage>("/detail/:id"));
});
```

### RouterOptions（路由选项）

用于配置 Router 的行为：

```csharp
builder.Services.AddRouting(options =>
{
    options.GuardTypes.Add(typeof(AuthGuard));
    options.GuardTypes.Add(typeof(RoleGuard));

    // 顶级路由是否在栈中（影响 CanGoBack 判断）
    // true（默认）：顶级路由算在栈里，GoBack 需要 Count > 1
    // false：顶级路由不算在栈里，GoBack 需要 Count > 0
    //          适用于顶级页面直接写在视图里的场景
    options.TopRouteInStack = true;
});
```

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `GuardTypes` | `[]` | 路由守卫类型列表 |
| `TopRouteInStack` | `true` | 顶级路由是否在栈中 |

---

## 路由注册

### 方式一：fluent API

```csharp
builder.Services.AddRoutes(routes =>
{
    routes.Register(new RouteEntity<HomePage>("/home") { IsTop = true });
    routes.Register(new RouteEntity<SettingsPage>("/settings") { IsKeepalive = true });
});
```

### 方式二：直接传入路由集合

```csharp
var routes = new[]
{
    new RouteEntity<HomePage>("/home") { IsTop = true },
    new RouteEntity<AboutPage>("/about")
};

builder.Services.AddRoutes(routes);
```

> **注意**：`AddRoutes` 会自动将 `RouteType` 和 `ViewModelType` 注册到 DI 容器，无需手动注册。

---

## 导航操作

### 基本导航

```csharp
// 通过路由路径
await router.NavigateAsync("/home");

// 通过页面类型
await router.NavigateAsync<HomePage>();
await router.NavigateAsync(typeof(HomePage));

// 带字典参数
await router.NavigateAsync("/settings", new Dictionary<string, object?>
{
    ["userId"] = 123,
    ["mode"] = "edit"
});

// 指定导航动作类型（UI 框架根据 action 决定如何显示页面）
await router.NavigateAsync("/home", action: "Present");   // 以模态方式呈现
await router.NavigateAsync("/home", action: "Push");      // 默认推入

// 带参数 + 动作
await router.NavigateAsync("/settings", new Dictionary<string, object?> { ["id"] = 1 }, action: "Modal");
```

### 路径参数导航

路由路径支持 `:paramName` 格式的参数占位符：

```csharp
// 注册带参数的路由
builder.Services.AddRoutes(routes =>
{
    routes.Register(new RouteEntity<UserDetailPage>("/user/:userId"));
    routes.Register(new RouteEntity<UserProfilePage>("/user/:userId/profile/:tab"));
});

// 方式一：路径参数（自动解析）
await router.NavigateAsync("/user/123");
// 参数自动提取: { "userId": "123" }

// 方式二：字典参数
await router.NavigateAsync<HomePage>("/user/:userId", new Dictionary<string, object?>
{
    ["userId"] = 456
});

// 方式三：混合使用（参数合并）
await router.NavigateAsync("/user/123/profile", new Dictionary<string, object?>
{
    ["tab"] = "settings"  // 额外参数
});
// 最终参数: { "userId": "123", "tab": "settings" }
```

### 返回操作

```csharp
// 返回上一页（默认 action = NavigationActions.Pop）
if (router.CanGoBack)
{
    await router.GoBackAsync();
}

// 自定义 action
await router.GoBackAsync(action: "PopWithAnimation");

// 返回到栈顶（清空当前栈，默认 action = NavigationActions.PopToRoot）
await router.GoBackToRootAsync();

// 返回到指定页面（默认 action = NavigationActions.PopToPage）
await router.GoBackToAsync("/home");
await router.GoBackToAsync<HomePage>();
```

### 切换顶级路由

在多页应用（≥2 个 IsTop）中：

```csharp
// 获取所有顶级路由
var topRoutes = router.RegisteredTopRoutes;

// 切换到指定顶级路由
router.SwitchTopRoute(topRoutes[0].Id);
```

> **提示**：`NavigateAsync` 到 IsTop 路由会自动完成切换，无需手动调用 `SwitchTopRoute`。

---

## 路由守卫

### 定义守卫

```csharp
public class AuthGuard : IRouteGuard
{
    private readonly IAuthService _authService;

    public AuthGuard(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> CanNavigateAsync(RouteEntity targetRoute, Dictionary<string, object?>? parameters)
    {
        if (RequiresAuth(targetRoute))
        {
            return await _authService.IsLoggedInAsync();
        }
        return true;
    }

    public async Task OnNavigationBlockedAsync(RouteEntity targetRoute, Dictionary<string, object?>? parameters)
    {
        Console.WriteLine($"访问 {targetRoute.RoutePath} 被阻止");
    }

    private bool RequiresAuth(RouteEntity route)
    {
        return route.RoutePath?.StartsWith("/settings") == true
            || route.RoutePath?.StartsWith("/profile") == true;
    }
}
```

### 注册守卫

```csharp
// 方式一：泛型快捷注册（单个守卫）
builder.Services.AddRouting<AuthGuard>();

// 方式二：配置回调注册多个守卫
builder.Services.AddRouting(options =>
{
    options.GuardTypes.Add(typeof(AuthGuard));
    options.GuardTypes.Add(typeof(RoleGuard));  // 按添加顺序执行
});

// 方式三：直接添加（框架层自行管理）
builder.Services.AddSingleton<IRouteGuard, AuthGuard>();
builder.Services.AddSingleton<IRouteGuard, CustomGuard>();
```

### 守卫链

多个守卫按注册顺序执行，任一返回 `false` 即阻止导航：

```csharp
// 执行顺序：AuthGuard → RoleGuard → CustomGuard
// 如果 AuthGuard 返回 false，后续守卫不会执行
```

---

## 生命周期钩子

### INavigationAware 接口

```csharp
public class MyPage : INavigationAware
{
    public void OnNavigated(Dictionary<string, object?>? parameters)
    {
        var id = parameters?["id"];
        Console.WriteLine($"页面激活，参数: {id}");
    }

    public void OnNavigatingFrom()
    {
        Console.WriteLine("即将离开页面");
    }

    public void OnNavigatedFrom()
    {
        Console.WriteLine("已离开页面");
    }
}
```

### 基类方式

```csharp
// 使用 NavigationAware 基类，只需重写关心的方法
public class MyPage : NavigationAware
{
    public override void OnNavigated(Dictionary<string, object?>? parameters)
    {
        // 处理导航参数
    }
}
```

### 生命周期时序

**导航时：**

```
OnNavigatingFrom (当前页) → 入栈 → OnNavigated (目标页)
```

**返回时（非 KeepAlive）：**

```
OnNavigatingFrom (当前页) → OnNavigatedFrom → 销毁实例 → 出栈 →
    (新栈顶实例存在?) → OnNavigated : 重建实例 → OnNavigated
```

---

## ViewModel 支持

### 注册路由时指定 ViewModel

```csharp
builder.Services.AddRoutes(routes =>
{
    routes.Register(new RouteEntity<HomePage, HomeViewModel>("/home") { IsTop = true });
    routes.Register(new RouteEntity<UserPage, UserViewModel>("/user"));
});
```

### ViewModel 实现 IQueryAttributable

```csharp
public class UserViewModel : IQueryAttributable
{
    public string? UserId { get; set; }
    public string? Tab { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object?> parameters)
    {
        if (parameters.TryGetValue("userId", out var userId))
            UserId = userId as string;

        if (parameters.TryGetValue("tab", out var tab))
            Tab = tab as string;
    }
}
```

### 生命周期推断规则

| 路由类型 | Page 生命周期 | ViewModel 生命周期 |
|----------|--------------|-------------------|
| 顶级路由 (`IsTop = true`) | Singleton | Singleton |
| KeepAlive | Transient + 缓存 | Transient + 缓存 |
| 普通页面 | Transient | Transient |

> **注意**：通过 `AddRoutes` 注册路由时，PageType 和 ViewModelType 会自动注册到 DI 容器，生命周期根据 IsTop 推断。

### 框架层集成

Router 会自动从 DI 获取 Page 和 ViewModel 实例，框架层只需处理 UI 切换：

```csharp
// 框架订阅 Navigated 事件
router.Navigated += (sender, args) =>
{
    // Router 已经创建了 PageInstance 和 ViewModelInstance
    // 框架层只需绑定 ViewModel 并切换 UI

    if (args.To?.PageInstance != null)
    {
        if (args.To.ViewModelInstance != null)
        {
            // WPF/Avalonia: page.BindingContext = args.To.ViewModelInstance;
            // MAUI: page.BindingContext = args.To.ViewModelInstance;
        }

        // 切换 UI
        Frame.Navigate(args.To.PageInstance);
    }
};
```

### 职责划分

| 谁做 | 做什么 |
|------|--------|
| **Router** | 从 DI 获取 Page 和 ViewModel，触发 INavigationAware，触发 IQueryAttributable |
| **框架层** | 绑定 ViewModel 到 BindingContext，处理 UI 切换 |

---

## 事件通知

Router 提供事件供 UI 框架订阅：

```csharp
var router = _serviceProvider.GetRequiredService<IRouter>();

// 导航开始
router.NavigationStarting += (sender, args) =>
{
    Console.WriteLine($"开始导航: {args.From?.Entity.RouteKey} → {args.To?.Entity.RouteKey}");
};

// 导航完成
router.Navigated += (sender, args) =>
{
    Console.WriteLine($"导航完成: {args.From?.Entity.RouteKey} → {args.To?.Entity.RouteKey}");
};

// 导航失败
router.NavigationFailed += (sender, args) =>
{
    Console.WriteLine($"导航失败: {args.Status} - {args.Message}");
};
```

### NavigationEventArgs 属性

```csharp
public class NavigationEventArgs : EventArgs
{
    public string Action { get; }         // 导航动作类型
    public RouteEntry? From { get; }      // 来源路由
    public RouteEntry? To { get; }        // 目标路由
    public NavigationStatus Status { get; }  // 状态
    public string? Message { get; }       // 消息
    public Dictionary<string, object?>? Parameters { get; }  // 参数
}
```

### NavigationActions 导航动作常量

```csharp
public static class NavigationActions
{
    public const string Push = "Push";       // 推入新页面
    public const string Pop = "Pop";         // 弹出当前页面
    public const string PopToRoot = "PopToRoot";  // 弹出到根页面
    public const string PopToPage = "PopToPage"; // 弹出到指定页面
    public const string Replace = "Replace"; // 替换当前页面
    public const string SwitchTop = "SwitchTop"; // 切换顶级路由
}
```

### UI 框架集成示例

```csharp
router.Navigated += (sender, args) =>
{
    switch (args.Action)
    {
        case NavigationActions.Push:
            Frame.Push(args.To!.PageInstance);
            break;

        case NavigationActions.Pop:
            Frame.Pop(args.From!.PageInstance);
            break;

        case NavigationActions.PopToRoot:
            Frame.PopToRoot();
            break;

        case NavigationActions.PopToPage:
            Frame.PopToPage(args.To);
            break;

        case NavigationActions.Replace:
            Frame.Replace(args.To!);
            break;

        case NavigationActions.SwitchTop:
            // 切换顶级路由
            Frame.SwitchTo(args.To!);
            break;

        default:
            break;
    }
};
```

---

## 高级用法

### KeepAlive 页面缓存

```csharp
builder.Services.AddRoutes(routes =>
{
    routes.Register(new RouteEntity<DashboardPage>("/dashboard") { IsKeepalive = true });
});

// 行为：
// 1. 首次访问：创建实例
// 2. 导航离开：保留实例（不销毁）
// 3. 返回访问：复用已有实例
// 4. 显式返回根路由：销毁实例
```

### 默认顶级路由

当没有注册任何顶级路由时，系统自动创建一个默认顶级路由作为兜底：

```csharp
builder.Services.AddRoutes(routes =>
{
    // 没有 IsTop，系统自动用兜底顶级路由
    routes.Register(new RouteEntity<HomePage>("/home"));
});
```

### 嵌套路由

父子关系由栈历史自动确定，无需预配置：

```csharp
builder.Services.AddRoutes(routes =>
{
    routes.Register(new RouteEntity<HomePage>("/home") { IsTop = true });
    routes.Register(new RouteEntity<SettingsPage>("/home/settings"));
    routes.Register(new RouteEntity<ProfilePage>("/home/profile"));
});

// 导航时自动建立父子关系
await router.NavigateAsync("/home");           // 栈: [HomePage]
await router.NavigateAsync("/home/settings");  // 栈: [HomePage, SettingsPage]

var parent = router.CurrentStack.Parent;  // HomePage
var ancestors = router.CurrentStack.GetAncestors();  // [HomePage]
```

### AOT 兼容性

本库完整支持 AOT 编译：

```csharp
// ✅ 正确：使用 typeof
routes.Register(new RouteEntity<HomePage>("/home"));

// ❌ 禁止：字符串类型名
routes.Register(new RouteEntity { RouteType = "HomePage" });  // 不支持！
```

### DI 扩展

`MFToolkit.Routing` 通过扩展方法与 Microsoft.Extensions.DependencyInjection 无缝集成：

| 方法 | 说明 |
|------|------|
| `services.AddRouting()` | 添加路由服务（可选配置） |
| `services.AddRouting<TGuard>()` | 添加路由服务并注册单个守卫 |
| `services.AddRoutes(routes)` | 注册路由集合 |
| `services.AddRoutes(configure)` | 通过回调注册路由 |
| `services.GetRegisteredRoutes()` | 从 ServiceProvider 获取已注册的路由 |

---

## 常见问题

### Q: 如何在 UI 框架中使用？

A: Router 本身不操作 UI，但会从 DI 获取 Page 和 ViewModel 实例。你需要：

1. 在框架层订阅 `Navigated` 事件
2. 在事件处理中绑定 ViewModel 并执行框架特定的页面切换代码

```csharp
router.Navigated += async (sender, args) =>
{
    if (args.To?.PageInstance != null)
    {
        if (args.To.ViewModelInstance != null)
        {
            args.To.PageInstance.BindingContext = args.To.ViewModelInstance;
        }
        await Dispatcher.InvokeAsync(() => MainFrame.Navigate(args.To.PageInstance));
    }
};
```

### Q: 如何处理导航参数？

A: 在 `OnNavigated` 方法中接收：

```csharp
public class MyPage : INavigationAware
{
    public void OnNavigated(Dictionary<string, object?>? parameters)
    {
        if (parameters != null && parameters.TryGetValue("id", out var id))
        {
            LoadData((int)id!);
        }
    }
}
```

---

## API 参考

### IRouter

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `CurrentRoute` | `RouteEntry?` | 当前路由条目 |
| `CurrentStack` | `IReadOnlyList<RouteEntry>` | 当前栈历史 |
| `CurrentTopRouteId` | `Guid` | 当前顶级路由的 ID |
| `RegisteredTopRoutes` | `IReadOnlyList<RouteEntity>` | 已注册的顶级路由列表 |
| `IsUsingDefaultTopRoute` | `bool` | 是否使用默认顶级路由 |
| `StackDepth` | `int` | 当前栈深度 |
| `CanGoBack` | `bool` | 是否可以返回 |

#### 方法

| 方法 | 说明 |
|------|------|
| `NavigateAsync(string routeKey, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Push)` | 通过路由键导航 |
| `NavigateAsync(Type pageType, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Push)` | 通过页面类型导航 |
| `NavigateAsync<T>(Dictionary<string, object?>? parameters = null, string action = NavigationActions.Push)` | 泛型导航 |
| `GoBackAsync(string action = NavigationActions.Pop)` | 返回上一页 |
| `GoBackToRootAsync(string action = NavigationActions.PopToRoot)` | 返回栈顶 |
| `GoBackToAsync(string routeKey, string action = NavigationActions.PopToPage)` | 返回到指定路由 |
| `GoBackToAsync(Type pageType, string action = NavigationActions.PopToPage)` | 返回到指定类型 |
| `GoBackToAsync<T>(string action = NavigationActions.PopToPage)` | 泛型返回 |
| `ReplaceAsync(string routeKey, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Replace)` | 替换当前页面 |
| `ReplaceAsync(Type pageType, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Replace)` | 类型替换 |
| `ReplaceAsync<T>(Dictionary<string, object?>? parameters = null, string action = NavigationActions.Replace)` | 泛型替换 |
| `SwitchTopRoute(Guid topRouteId)` | 切换到指定顶级路由 |

#### 事件

| 事件 | 说明 |
|------|------|
| `NavigationStarting` | 导航即将开始 |
| `Navigated` | 导航已完成 |
| `NavigationFailed` | 导航失败或被阻止 |

### NavigationResult

| 属性 | 说明 |
|------|------|
| `IsSuccess` | 是否成功 |
| `IsBlocked` | 是否被守卫阻止 |
| `IsNotFound` | 是否未找到路由 |
| `IsCancelled` | 是否被取消 |
| `Status` | `NavigationStatus` 状态枚举 |
| `TargetRoute` | 目标路由实体 |
| `Message` | 状态消息 |

### NavigationEventArgs

| 属性 | 说明 |
|------|------|
| `Action` | 导航动作类型（Push/Pop/SwitchTop 等） |
| `From` | 来源路由条目 |
| `To` | 目标路由条目 |
| `Status` | `NavigationStatus` 状态 |
| `Message` | 消息 |

### NavigationActions

| 常量 | 值 | 说明 |
|------|---|------|
| `Push` | `"Push"` | 推入新页面 |
| `Pop` | `"Pop"` | 弹出当前页面 |
| `PopToRoot` | `"PopToRoot"` | 弹出到根页面 |
| `PopToPage` | `"PopToPage"` | 弹出到指定页面 |
| `Replace` | `"Replace"` | 替换当前页面 |
| `SwitchTop` | `"SwitchTop"` | 切换顶级路由 |

---

*文档版本：v1.13*
*最后更新：2026-04-28*

## 变更记录

| 版本 | 日期 | 变更内容 |
|------|------|----------|
| v1.0.0 | 2026-04-23 | 初始版本 |
| v1.0.1 | 2026-04-23 | 新增 IQueryAttributable 接口支持 |
| v1.0.2 | 2026-04-23 | 删除 ParentId，改为栈历史动态获取父子关系 |
| v1.0.3 | 2026-04-23 | 新增 ViewModelType/ViewModelInstance 支持，Router 创建 ViewModel |
| v1.0.4 | 2026-04-23 | AddRoutes 自动注册 PageType/ViewModelType 到 DI 容器 |
| v1.0.5 | 2026-04-23 | 新增 NavigationActions 枚举和 NavigationEventArgs.Action，提供导航动作类型给 UI 框架 |
| v1.0.6 | 2026-04-24 | RouterOptions.GuardType 改为 GuardTypes 列表，支持注册多个守卫 |
| v1.0.7 | 2026-04-24 | NavigateAsync/ReplaceAsync 增加可选 action 参数，支持自定义导航动作类型 |
| v1.0.8 | 2026-04-25 | GoBackAsync/GoBackToRootAsync/GoBackToAsync 统一补齐 action 参数，所有导航方法默认值均使用 NavigationActions 常量 |
| v1.0.9 | 2026-04-25 | 文档补充启动项目需引用 Microsoft.Extensions.DependencyInjection 的说明 |
| v1.0.10 | 2026-04-25 | RouteEntity 新增多个构造函数和泛型子类 RouteEntity&lt;TRoute&gt;、RouteEntity&lt;TRoute,TViewModel&gt;，支持 fluent 链式调用 |
| v1.0.11 | 2026-04-25 | 修复 NavigateAsync 到 IsTop 路由时错误 Push 到当前栈的问题；新增 IsTop 与应用模式（SPA/多页）说明文档 |
| v1.0.12 | 2026-04-25 | 文档全面更新：目录补全，新增 RouteRegistry、RouterOptions、DI 扩展章节，API 参考补全所有方法签名和属性 |
| v1.0.13 | 2026-04-28 | 新增 RouterOptions.TopRouteInStack 配置项，支持用户自定义顶级路由是否计入栈中，影响 CanGoBack 判断 |
