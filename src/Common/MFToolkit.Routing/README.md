# MFToolkit.Routing 使用指南

> 框架无关的 .NET 路由导航库，支持 Avalonia、WPF、MAUI 等 UI 框架

---

## 目录

- [快速开始](#快速开始)
- [核心概念](#核心概念)
- [路由注册](#路由注册)
- [导航操作](#导航操作)
- [路由守卫](#路由守卫)
- [生命周期钩子](#生命周期钩子)
- [事件通知](#事件通知)
- [高级用法](#高级用法)

---

## 快速开始

### 1. 安装

```bash
dotnet add package MFToolkit.Routing
```

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
    routes.Register(new RouteEntity
    {
        RoutePath = "/home",
        RouteType = typeof(HomePage),
        IsTop = true
    });

    routes.Register(new RouteEntity
    {
        RoutePath = "/settings",
        RouteType = typeof(SettingsPage)
    });
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

---

## 路由注册

### 方式一： fluent API

```csharp
builder.Services.AddRoutes(routes =>
{
    routes.Register(new RouteEntity
    {
        RoutePath = "/home",
        RouteType = typeof(HomePage),
        IsTop = true
    });

    routes.Register(new RouteEntity
    {
        RoutePath = "/settings",
        RouteType = typeof(SettingsPage),
        IsKeepalive = true  // 保持活跃
    });
});
```

### 方式二：直接注册到 Router

```csharp
var router = new Router();

router.RegisterRoute(new RouteEntity
{
    RoutePath = "/home",
    RouteType = typeof(HomePage),
    IsTop = true
});

router.RegisterRoutes(new[]
{
    new RouteEntity { RoutePath = "/about", RouteType = typeof(AboutPage) },
    new RouteEntity { RoutePath = "/contact", RouteType = typeof(ContactPage) }
});
```

### 方式三：使用 RouteBuilder 辅助类

```csharp
public static class RouteBuilder
{
    public static RouteEntity Create(string path, Type type, bool isTop = false)
    {
        return new RouteEntity
        {
            RoutePath = path,
            RouteType = type,
            IsTop = isTop
        };
    }
}

// 使用
router.RegisterRoutes(new[]
{
    RouteBuilder.Create("/home", typeof(HomePage), isTop: true),
    RouteBuilder.Create("/settings", typeof(SettingsPage)),
    RouteBuilder.Create("/profile", typeof(ProfilePage))
});
```

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
```

### 路径参数导航

路由路径支持 `:paramName` 格式的参数占位符：

```csharp
// 注册带参数的路由
router.RegisterRoute(new RouteEntity
{
    RoutePath = "/user/:userId",
    RouteType = typeof(UserDetailPage)
});

router.RegisterRoute(new RouteEntity
{
    RoutePath = "/user/:userId/profile/:tab",
    RouteType = typeof(UserProfilePage)
});

// 方式一：路径参数（自动解析）
await router.NavigateAsync("/user/123");
// 参数自动提取: { "userId": "123" }

// 方式二：字典参数
await router.NavigateAsync("/user/:userId", new Dictionary<string, object?>
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

### 返回操作

```csharp
// 返回上一页
if (router.CanGoBack)
{
    await router.GoBackAsync();
}

// 返回到栈顶（清空当前栈）
await router.GoBackToRootAsync();
```

### 切换顶级路由

```csharp
// 获取所有顶级路由
var topRoutes = router.RegisteredTopRoutes;

// 切换到指定顶级路由
router.SwitchTopRoute(topRoutes[0].Id);
```

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
        // 检查是否需要登录
        if (RequiresAuth(targetRoute))
        {
            return await _authService.IsLoggedInAsync();
        }
        return true;
    }

    public async Task OnNavigationBlockedAsync(RouteEntity targetRoute, Dictionary<string, object?>? parameters)
    {
        // 导航被阻止时的处理
        Console.WriteLine($"访问 {targetRoute.RoutePath} 被阻止");
        // 例如：跳转到登录页
        // await _router.NavigateAsync("/login");
    }

    private bool RequiresAuth(RouteEntity route)
    {
        // 根据路由路径判断是否需要认证
        return route.RoutePath?.StartsWith("/settings") == true
            || route.RoutePath?.StartsWith("/profile") == true;
    }
}
```

### 注册守卫

```csharp
// 方式一：在 AddRouting 中指定
builder.Services.AddRouting<AuthGuard>();

// 方式二：使用配置回调
builder.Services.AddRouting(options =>
{
    options.GuardType = typeof(AuthGuard);
});

// 方式三：直接添加
builder.Services.AddSingleton<IRouteGuard, AuthGuard>();
builder.Services.AddSingleton<IRouteGuard, RoleGuard>();  // 多个守卫按顺序执行
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
    /// <summary>
    /// 页面激活时调用
    /// </summary>
    public void OnNavigated(Dictionary<string, object?>? parameters)
    {
        // 接收导航参数
        var id = parameters?["id"];
        Console.WriteLine($"页面激活，参数: {id}");
    }

    /// <summary>
    /// 即将离开页面时调用
    /// </summary>
    public void OnNavigatingFrom()
    {
        // 保存状态、取消订阅等
        Console.WriteLine("即将离开页面");
    }

    /// <summary>
    /// 已离开页面后调用
    /// </summary>
    public void OnNavigatedFrom()
    {
        // 清理资源
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
router.RegisterRoutes(new[]
{
    new RouteEntity
    {
        RoutePath = "/home",
        RouteType = typeof(HomePage),
        ViewModelType = typeof(HomeViewModel),
        IsTop = true
    },
    new RouteEntity
    {
        RoutePath = "/user",
        RouteType = typeof(UserPage),
        ViewModelType = typeof(UserViewModel)
    }
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
| 顶级路由 (`IsTop = true`) | Singleton | Transient |
| KeepAlive | Transient + 缓存 | Transient + 缓存 |
| 普通页面 | Transient | Transient |

> **注意**：通过 `AddRoutes` 注册路由时，PageType 和 ViewModelType 会自动注册到 DI 容器，无需手动注册。

### 框架层集成

```csharp
// 框架订阅 Navigated 事件
router.Navigated += (sender, args) =>
{
    // 1. 创建 Page
    var page = _serviceProvider.GetRequiredService(args.To.Entity.RouteType);
    args.To.PageInstance = page;

    // 2. 绑定 ViewModel（框架做的事）
    if (args.To.ViewModelInstance != null)
    {
        page.BindingContext = args.To.ViewModelInstance;
    }

    // 3. 切换 UI
    Frame.Navigate(page);
};
```

### 职责划分

| 谁做 | 做什么 |
|------|--------|
| **Router** | 创建 ViewModel，触发 INavigationAware，触发 IQueryAttributable |
| **框架层** | 创建 Page，绑定 ViewModel 到 BindingContext，处理 UI 切换 |

---

## 事件通知

Router 提供事件供 UI 框架订阅：

```csharp
var router = new Router();

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
    public RouteEntry? From { get; }      // 来源路由
    public RouteEntry? To { get; }        // 目标路由
    public NavigationStatus Status { get; }  // 状态
    public string? Message { get; }       // 消息
    public Dictionary<string, object?>? Parameters { get; }  // 参数
}
```

---

## 高级用法

### KeepAlive 页面缓存

```csharp
// 注册 KeepAlive 页面
router.RegisterRoute(new RouteEntity
{
    RoutePath = "/dashboard",
    RouteType = typeof(DashboardPage),
    IsKeepalive = true  // 离开时保留实例
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
// 这种情况会使用默认顶级路由
router.RegisterRoute(new RouteEntity
{
    RoutePath = "/home",
    RouteType = typeof(HomePage)
    // 没有设置 IsTop = true
});

// 注册首个 IsTop = true 的路由时，自动切换到它
router.RegisterRoute(new RouteEntity
{
    RoutePath = "/admin",
    RouteType = typeof(AdminPage),
    IsTop = true  // 切换到 admin 栈
});
```

### 嵌套路由

父子关系由栈历史自动确定，无需预配置：

```csharp
// 注册路由（无需配置父子关系）
router.RegisterRoutes(new[]
{
    new RouteEntity { RoutePath = "/home", RouteType = typeof(HomePage), IsTop = true },
    new RouteEntity { RoutePath = "/home/settings", RouteType = typeof(SettingsPage) },
    new RouteEntity { RoutePath = "/home/profile", RouteType = typeof(ProfilePage) }
});

// 导航时自动建立父子关系
await router.NavigateAsync("/home");           // 栈: [HomePage]
await router.NavigateAsync("/home/settings");  // 栈: [HomePage, SettingsPage]
                                              // SettingsPage.Parent = HomePage

// 从栈历史获取父路由
var parent = router.CurrentStack.Parent;  // HomePage
var ancestors = router.CurrentStack.GetAncestors();  // [HomePage]
```

### AOT 兼容性

本库完整支持 AOT 编译：

```csharp
// ✅ 正确：使用 typeof
router.RegisterRoute(new RouteEntity { RouteType = typeof(HomePage) });

// ❌ 禁止：字符串类型名
router.RegisterRoute(new RouteEntity { RouteType = "HomePage" });  // 不支持！
```

---

## 常见问题

### Q: 如何在 UI 框架中使用？

A: Router 本身不操作 UI，你需要：

1. 在框架层订阅 `Navigated` 事件
2. 在事件处理中执行框架特定的页面切换代码
3. 框架负责创建/销毁页面实例并赋值给 `RouteEntry.PageInstance`

示例（WPF 伪代码）：
```csharp
router.Navigated += async (sender, args) =>
{
    if (args.To?.PageInstance == null)
    {
        // 框架创建页面实例
        var page = _serviceProvider.GetRequiredService(args.To.Entity.RouteType);
        args.To.PageInstance = page;
    }
    // 切换 UI
    await Dispatcher.InvokeAsync(() => MainFrame.Navigate(args.To.PageInstance));
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
            // 使用参数
            LoadData((int)id!);
        }
    }
}
```

---

## API 参考

### IRouter

| 方法/属性 | 说明 |
|-----------|------|
| `NavigateAsync(routeKey, parameters)` | 通过路由键导航 |
| `NavigateAsync<T>(parameters)` | 通过类型导航（泛型） |
| `GoBackAsync()` | 返回上一页 |
| `GoBackToRootAsync()` | 返回栈顶 |
| `SwitchTopRoute(topRouteId)` | 切换顶级路由 |
| `RegisterRoute(entity)` | 注册路由 |
| `CurrentRoute` | 当前路由条目 |
| `CurrentStack` | 当前栈 |
| `CanGoBack` | 是否可以返回 |

### NavigationResult

| 属性 | 说明 |
|------|------|
| `IsSuccess` | 是否成功 |
| `IsBlocked` | 是否被阻止 |
| `IsNotFound` | 是否未找到 |
| `IsCancelled` | 是否取消 |
| `Status` | 状态枚举 |
| `TargetRoute` | 目标路由 |
| `Message` | 消息 |

---

*文档版本：v1.4*
*最后更新：2026-04-23*

## 变更记录

| 版本 | 日期 | 变更内容 |
|------|------|----------|
| v1.0 | 2026-04-23 | 初始版本 |
| v1.1 | 2026-04-23 | 新增 IQueryAttributable 接口支持 |
| v1.2 | 2026-04-23 | 删除 ParentId，改为栈历史动态获取父子关系 |
| v1.3 | 2026-04-23 | 新增 ViewModelType/ViewModelInstance 支持，Router 创建 ViewModel |
| v1.4 | 2026-04-23 | AddRoutes 自动注册 PageType/ViewModelType 到 DI 容器 |
