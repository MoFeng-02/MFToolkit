# MFToolkit.Routing 开发需求规格

> **状态**：需求部分已实现，核心功能完成
> **目标框架**：.NET 10.0

---

## 一、项目定位

**框架无关**的路由导航库，服务于 DI 容器，可被 Avalonia、WPF、MAUI 等 .NET UI 框架使用。

### 核心价值
- 统一的路由抽象层
- 与 UI 框架解耦（Router 不直接操作 UI）
- AOT 完整兼容

### 关键原则
> **Router 专注于路由逻辑，UI 操作由框架层通过导航钩子实现**

---

## 二、技术约束与原则

### 2.1 AOT 兼容性（硬性要求）

本库必须完整支持 AOT 编译，适用于 MAUI / Avalonia 等场景。

#### 禁止使用

| 约束 | 说明 |
|------|------|
| ❌ `dynamic` | 禁止 |
| ❌ 运行时反射生成类型 | 禁止 |
| ❌ `Type.GetType()` 字符串查找 | 禁止 |
| ❌ `Activator.CreateInstance(typeName)` | 禁止 |
| ❌ 泛型 `new T()` 反射调用 | 禁止 |

#### 必须遵循

| 约束 | 说明 |
|------|------|
| ✅ 硬编码类型引用 | 必须 |
| ✅ `typeof(Foo)` 编译期已知 | 必须 |
| ✅ `RouteEntity.RouteType` 必须是编译期已知类型 | 必须 |

#### 注册约束

```csharp
// ✅ 正确：使用 typeof，编译期安全
router.RegisterRoute(new RouteEntity { RouteType = typeof(HomePage) });

// ❌ 禁止：字符串类型名，AOT 不安全
router.RegisterRoute(new RouteEntity { RouteType = "HomePage" });
```

---

### 2.2 路由发现机制

**纯手动注册，不支持自动扫描**。

- [x] 用户显式调用 `RegisterRoute()` 注册路由
- [x] 支持批量注册 `RegisterRoutes(IEnumerable<RouteEntity>)`
- [x] 支持 DI 批量注入（通过 `IServiceCollection.AddRouting()`）

---

## 三、核心能力清单

| 能力 | 描述 | 优先级 | 状态 |
|------|------|--------|------|
| 路由注册 | 纯手动注册，不支持自动扫描 | P0 | ✅ |
| 导航 | Navigate / Push 到目标路由 | P0 | ✅ |
| 后退 | GoBack / Pop，支持返回根路由 | P0 | ✅ |
| 路由栈 | 按顶级路由隔离的栈管理 | P0 | ✅ |
| 路由传参 | 支持字典参数 | P0 | ✅ |
| 路由守卫 | 导航前权限验证（CanNavigate） | P0 | ✅ |
| 默认顶级路由 | 无顶级路由时自动兜底 | P0 | ✅ |
| 生命周期钩子 | OnNavigatingFrom / OnNavigatedFrom 等 | P0 | ✅ |
| KeepAlive 缓存 | 保持页面实例，复用（含 Bug 修复） | P1 | ✅ |
| 多守卫支持 | RouterOptions.GuardTypes 支持多个守卫 | P1 | ✅ |
| 导航动作标识 | NavigationActions 区分 Push/Pop/Replace 等 | P1 | ✅ |
| GoBackToAsync | 返回到指定路由 | P1 | ✅ |
| Replace 替换 | 替换当前路由 | P1 | ✅ |
| StackDepth | 获取当前栈深度 | P1 | ✅ |
| ViewModel DI | Router 内部创建 ViewModel 实例 | P1 | ✅ |
| 后置 Action | OnNavigated 回调链 | P1 | 待定 |

---

## 四、RouteEntity 路由实体

### 3.1 基础属性

```csharp
public class RouteEntity
{
    /// 路由唯一ID
    public Guid Id { get; set; }

    /// 路由路径，如 "/home/settings"
    public string? RoutePath { get; set; }

    /// 路由名称（默认取 RouteType.Name）
    public string? RouteName { get; set; }

    /// 路由实体类（页面类型）
    public required Type RouteType { get; set; }

    /// 视图模型类型（可选，用于 MVVM 模式）
    public Type? ViewModelType { get; set; }

    /// 默认路由参数
    public Dictionary<string, object?>? DefaultParameters { get; set; }

    /// 保持活跃，不被缓存清理
    public bool IsKeepalive { get; set; } = false;

    /// 是否为顶级路由
    public bool IsTop { get; set; } = false;

    /// 懒加载（按需实例化）
    public bool IsLazy { get; set; } = true;

    /// 排序权重
    public int SortOrder { get; set; } = 0;

    /// 路由唯一键（用于字典索引）
    public string RouteKey => RoutePath ?? RouteType.Name;
}
```

### 3.2 父子关系

> **父子关系由栈历史动态确定，无需预配置**

```csharp
public class RouteStack
{
    /// 当前条目的父条目（栈中上一个条目）
    public RouteEntry? Parent => History.Count >= 2 
        ? History[^2] 
        : null;

    /// 获取祖先链（从根到父的顺序）
    public IReadOnlyList<RouteEntry> GetAncestors();
}
```

| 场景 | 父子关系 |
|------|----------|
| 用户导航 Home → User → Profile | Profile.Parent = User, User.Parent = Home |
| 不需要配置 | 栈历史自动记录 |
| 不同顶级路由 | 各自独立栈，互不影响 |

### 3.2 讨论待定

- [ ] `RouteModel` 属性是否保留？（ViewModel 类型，框架可自行决定关联方式）
- [ ] `Title`、`Icon` 等 UI 元数据是否需要？

---

## 五、Router 核心接口

### 5.1 导航事件通知

> UI 框架订阅这些事件以感知路由变化并执行 UI 操作

```csharp
public class NavigationEventArgs : EventArgs
{
    public RouteEntry? From { get; }
    public RouteEntry? To { get; }
    public NavigationStatus Status { get; }
    public string? Message { get; }
    
    /// 导航动作类型（Push/Pop/Replace 等）
    public string? Action { get; }
}

public interface IRouter
{
    // === 事件通知（UI 框架订阅） ===
    event EventHandler<NavigationEventArgs>? NavigationStarting;
    event EventHandler<NavigationEventArgs>? Navigated;
    event EventHandler<NavigationEventArgs>? NavigationFailed;

    // === 导航 ===
    Task<NavigationResult> NavigateAsync(string routeKey, Dictionary<string, object?>? parameters = null, string? action = null);
    Task<NavigationResult> NavigateAsync(Type pageType, Dictionary<string, object?>? parameters = null, string? action = null);

    // === 后退 ===
    Task<NavigationResult> GoBackAsync();
    Task<NavigationResult> GoBackToRootAsync();
    Task<NavigationResult> GoBackToAsync(string routeKey);

    // === 替换 ===
    Task<NavigationResult> ReplaceAsync(string routeKey, Dictionary<string, object?>? parameters = null, string? action = null);
    Task<NavigationResult> ReplaceAsync(Type pageType, Dictionary<string, object?>? parameters = null, string? action = null);

    // === 当前状态 ===
    Guid CurrentTopRouteId { get; }
    RouteEntry? CurrentRoute { get; }
    IReadOnlyList<RouteEntry> CurrentStack { get; }
    bool CanGoBack { get; }
    int StackDepth { get; }  // 当前栈深度

    // === 栈管理 ===
    void SwitchTopRoute(Guid topRouteId);
    IReadOnlyList<RouteEntity> RegisteredTopRoutes { get; }

    // === 注册 ===
    void RegisterRoute(RouteEntity entity);
    void RegisterRoutes(IEnumerable<RouteEntity> entities);
}
```

### 5.2 事件触发时机

| 事件 | 触发时机 |
|------|----------|
| `NavigationStarting` | 守卫通过后，导航开始前 |
| `Navigated` | 导航完成，页面激活后 |
| `NavigationFailed` | 导航失败（被阻止/找不到/异常） |

---

## 六、NavigationActions 导航动作

> 提供标准化的导航动作常量，供 UI 框架根据动作类型决定如何显示页面

```csharp
public static class NavigationActions
{
    /// 普通推入导航
    public const string Push = "Push";
    
    /// 弹出当前页面
    public const string Pop = "Pop";
    
    /// 返回到根路由
    public const string PopToRoot = "PopToRoot";
    
    /// 返回到指定路由
    public const string PopToPage = "PopToPage";
    
    /// 替换当前路由
    public const string Replace = "Replace";
    
    /// 切换顶级路由
    public const string SwitchTop = "SwitchTop";
}
```

### 使用场景

| Action | UI 框架行为示例 |
|--------|----------------|
| Push | 普通页面切换，支持返回手势 |
| Pop | 页面返回，触发返回动画 |
| Replace | 替换当前页面，无返回动画 |
| SwitchTop | Tab 切换，可能需要特殊过渡效果 |

---

## 七、NavigationResult 导航结果

```csharp
public class NavigationResult
{
    public NavigationStatus Status { get; }
    public string? Message { get; }
    public Exception? Error { get; }

    public bool IsSuccess => Status == NavigationStatus.Success;
    public bool IsCancelled => Status == NavigationStatus.Cancelled;
    public bool IsBlocked => Status == NavigationStatus.Blocked;
    public bool IsNotFound => Status == NavigationStatus.NotFound;
}

public enum NavigationStatus
{
    Success,
    Cancelled,
    Blocked,
    NotFound,
    Error
}
```

---

## 八、路由栈设计

### 6.1 数据结构

```csharp
public class RouteStack
{
    public Guid TopRouteId { get; }
    public RouteEntry Current => _entries.LastOrDefault();
    public IReadOnlyList<RouteEntry> History => _entries.ToList();
    public bool CanGoBack => _entries.Count > 1;

    private readonly List<RouteEntry> _entries = new();
}

public class RouteEntry
{
    public RouteEntity Entity { get; }
    public Dictionary<string, object?>? Parameters { get; }
    public DateTime NavigatedAt { get; }
    
    /// 页面实例（框架侧创建并赋值）
    public object? PageInstance { get; set; }
    
    /// 视图模型实例（Router 内部创建，可为空）
    public object? ViewModelInstance { get; set; }
}
```

### 6.2 栈管理器

```csharp
public class RouteStackManager
{
    // 按顶级路由 ID 隔离栈
    private readonly Dictionary<Guid, RouteStack> _stacks = new();
    private Guid _currentTopRouteId;

    public RouteStack CurrentStack { get; }
    public void SwitchTopRoute(Guid topRouteId);
}
```

---

## 九、实例创建与生命周期

### 8.1 生命周期推断规则

> **基于路由类型自动推断生命周期，无需手动指定**

| 路由类型 | 生命周期 | 说明 |
|----------|----------|------|
| **顶级路由** (`IsTop = true`) | Singleton | 全局唯一，如主 Tab 页 |
| **KeepAlive 页面** | Transient + 缓存 | 每次新建但存入缓存，下次复用 |
| **普通页面** | Transient | 每次导航创建新实例，离开销毁 |

### 8.2 职责划分

```
┌─────────────────────────────────────────────────────────┐
│                    UI 框架层                           │
│  (Avalonia/WPF/MAUI)                                  │
│                                                         │
│  1. 订阅 Router.Navigated 事件                         │
│  2. 通过 DI 创建 Page 实例                             │
│  3. 绑定 ViewModel 到 BindingContext                   │
│  4. 赋值 PageInstance                                  │
│  5. 处理页面切换 UI                                    │
└─────────────────────────────────────────────────────────┘
                           ▲
                           │ 触发 Navigated 事件
                           │
┌─────────────────────────────────────────────────────────┐
│                  MFToolkit.Routing                     │
│                     (Router)                            │
│                                                         │
│  1. 管理路由栈                                          │
│  2. 通过 DI 创建 ViewModel（如果注册了）                │
│  3. 调用 IQueryAttributable 注入参数                   │
│  4. 调用 INavigationAware 生命周期钩子                │
│  5. 触发事件通知框架                                   │
│  ❌ 不创建 Page，不处理绑定                           │
└─────────────────────────────────────────────────────────┘
```

### 8.3 Router 创建 ViewModel

```csharp
// Router.cs NavigateInternalAsync
private async Task<NavigationResult> NavigateInternalAsync(RouteEntity route, Dictionary<string, object?>? parameters)
{
    // ...

    // 创建 ViewModel（如果注册了）
    if (route.ViewModelType != null)
    {
        // 从 IServiceProvider 获取（框架注入）
        toEntry.ViewModelInstance = _serviceProvider.GetRequiredService(route.ViewModelType);

        // 调用参数注入
        if (toEntry.ViewModelInstance is IQueryAttributable attributable)
        {
            attributable.ApplyQueryAttributes(parameters);
        }
    }

    // 入栈
    _stackManager.CurrentStack.Push(toEntry);

    // 触发 INavigationAware（Page 生命周期）
    if (toEntry.PageInstance is INavigationAware targetAware)
    {
        targetAware.OnNavigated(toParameters);
    }
}
```

### 8.4 框架层集成示例

```csharp
// 框架集成
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

### 8.5 DI 注册规则（框架层）

```csharp
public void RegisterRoute(RouteEntity entity)
{
    // Page 注册
    if (entity.IsTop)
        Services.AddSingleton(entity.RouteType);   // 顶级路由：单例
    else
        Services.AddTransient(entity.RouteType);   // 普通：瞬态

    // ViewModel 注册（按同样规则）
    if (entity.ViewModelType != null)
    {
        if (entity.IsTop)
            Services.AddSingleton(entity.ViewModelType);
        else
            Services.AddTransient(entity.ViewModelType);
    }
}
```

---

## 十、默认顶级路由机制

### 9.1 设计原则

> 当没有任何顶级路由注册时，自动生成一个空的顶级路由作为兜底。用户无感知，对外不暴露。

### 7.2 实现位置

- **不在 RouteEntity 层面处理**
- **由 Router / RouteStackManager 内部维护**
- 对外只暴露 `RegisteredTopRoutes`（用户注册的）

### 7.3 内部默认顶级路由

```csharp
// 默认顶级路由只是一个 Guid，不创建 RouteEntity
private readonly Guid _defaultTopRouteId = Guid.NewGuid();
```

### 7.4 行为规则

| 场景 | 行为 |
|------|------|
| 初始状态 | 自动创建并激活默认顶级路由 |
| 用户注册首个顶级路由 | 自动激活新路由；可选保留默认路由 |
| 用户显式切换 | `SwitchTopRoute()` 切换栈上下文 |
| 用户无感知 | 对外 API 不暴露默认路由存在 |

---

## 十一、路由守卫

### 10.1 接口定义

```csharp
public interface IRouteGuard
{
    Task<bool> CanNavigateAsync(RouteEntity targetRoute, Dictionary<string, object?>? parameters);
    Task OnNavigationBlockedAsync(RouteEntity targetRoute, Dictionary<string, object?>? parameters);
}
```

### 8.2 使用方式

- 全局守卫：注册到 DI，作为单例
- 单路由守卫：挂载到 RouteEntity 上
- 守卫链：按顺序执行，任一拒绝则阻止

### 10.3 多守卫支持（RouterOptions）

```csharp
public class RouterOptions
{
    /// 守卫类型列表（支持多个守卫）
    public List<Type> GuardTypes { get; set; } = new();
}

// DI 注册
services.AddRouting(options =>
{
    options.GuardTypes.Add(typeof(AuthGuard));
    options.GuardTypes.Add(typeof(PermissionGuard));
});

// 或泛型方式
services.AddRouting<AuthGuard>();
services.AddRouting<PermissionGuard>();
```

---

## 十二、路由生命周期钩子

### 11.1 钩子定义

| 钩子 | 触发时机 | 必要性 |
|------|----------|--------|
| OnNavigating | 导航前，守卫通过后 | P1 |
| OnNavigated | 导航完成，实例创建后 | P0 |
| OnNavigatingFrom | 离开当前页前 | P1 |
| OnNavigatedFrom | 已离开当前页，实例可能销毁 | P1 |
| OnDisposing | 实例即将销毁时（非 KeepAlive） | P2 |

### 9.2 接口定义

```csharp
public interface INavigationAware
{
    void OnNavigated(Dictionary<string, object?>? parameters);
    void OnNavigatingFrom();
    void OnNavigatedFrom();
}
```

---

## 十三、生命周期接口设计决策

### 12.1 两个接口的定位差异

| 接口 | 职责定位 | 典型使用者 | 方法数量 |
|------|----------|-----------|----------|
| `INavigationAware` | 完整生命周期 + 参数接收 | Page / View | 3 个 |
| `IQueryAttributable` | 纯参数注入 | ViewModel | 1 个 |

### 12.2 设计原则

> **两个接口是"正交"的，解决不同问题**

```
INavigationAware      → 解决"什么时候做什么事"（生命周期管理）
IQueryAttributable    → 解决"参数怎么进来"（数据绑定）
```

### 12.3 使用场景对比

| 场景 | 推荐接口 | 说明 |
|------|----------|------|
| Page/View 需要控制动画/过渡 | `INavigationAware` | 完整生命周期 |
| ViewModel 只关心参数变化 | `IQueryAttributable` | 只需实现 `ApplyQueryAttributes` |
| MVVM 模式，View 和 ViewModel 解耦 | 两者同时使用 | View 用 `INavigationAware`，ViewModel 用 `IQueryAttributable` |

### 12.4 调用优先级

当实例同时实现两个接口时，Router **先调用 `INavigationAware`**：

```csharp
// Router.cs NavigateInternalAsync 伪代码
if (instance is INavigationAware aware)
{
    aware.OnNavigated(parameters);  // 完整生命周期优先
}
// 不需要 else if，因为一个实例通常只实现其中一个
```

### 12.5 IQueryAttributable 是轻量选择

> **IQueryAttributable 是 INavigationAware 的"参数子集"**

- 如果 ViewModel 只需要知道"参数来了" → 实现 `IQueryAttributable`
- 如果 View/Page 需要完整生命周期控制 → 实现 `INavigationAware`

这种设计让 ViewModel 避免实现空方法，保持简洁。

---

## 十四、非 KeepAlive 路由返回处理

### 13.1 设计原则

> **栈 Entry 保留，PageInstance 按需销毁**。返回时若实例已销毁，下次导航自动重建。

### 13.2 行为规则

| 场景 | 行为 |
|------|------|
| 导航到 PageC | 创建 PageC 实例，入栈 |
| 返回（GoBack） | PageC 实例销毁，Entry 保留（标记 PageInstance = null） |
| 再次导航到 PageC | 重新创建实例 |

### 13.3 流程图

```
用户 GoBack
    │
    ▼
┌─────────────────────────────────────┐
│ 1. 触发 OnNavigatingFrom (当前页)   │
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│ 2. 判断 IsKeepalive                  │
│    ├─ true  → 保留 PageInstance      │
│    └─ false → 触发 OnDisposing      │
│              → Dispose PageInstance  │
│              → PageInstance = null   │
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│ 3. 栈指针回退（Entry 不删除）         │
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│ 4. 触发 OnNavigatedFrom (旧页)      │
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│ 5. 新栈顶处理                        │
│    ├─ PageInstance != null → 直接激活│
│    └─ PageInstance == null → 重建实例│
│       → 触发 OnNavigated            │
└─────────────────────────────────────┘
```

### 13.4 KeepAlive 缓存策略

> **KeepAlive 页面实例由独立缓存字典管理，确保实例不被 GC 回收**

```csharp
// Router 内部 KeepAlive 缓存
private readonly Dictionary<string, object> _keepAliveCache = new();
```

#### 缓存规则

| 场景 | 行为 |
|------|------|
| Navigate Push | 优先从缓存获取实例，无缓存则创建新实例 |
| GoBack | KeepAlive 页面实例保存到缓存，不销毁 |
| GoBackToRoot | 清除当前顶级路由的所有 KeepAlive 缓存（重置语义） |
| GoBackTo | KeepAlive 页面实例保存到缓存 |
| 再次 Navigate | 从缓存恢复实例，触发 OnNavigated |

#### 缓存 Key 规则

```csharp
// 使用 RouteKey 作为缓存键
string cacheKey = routeEntity.RouteKey;
```

---

## 十五、路由发现机制

> **已确认：纯手动注册，不支持自动扫描**

- 用户显式调用 `RegisterRoute()` 注册路由
- 支持批量注册 `RegisterRoutes(IEnumerable<RouteEntity>)`
- DI 集成：`AddRouting()` 批量注入

---

## 十六、执行顺序与并发安全

### 15.1 注册顺序

```csharp
// 后注册的同名 RouteKey 覆盖先注册的
router.RegisterRoute(routeA);   // RouteKey = "home"
router.RegisterRoute(routeB);   // RouteKey = "home" → 覆盖 routeA

// 顶级路由：后注册的 IsTop 路由自动成为当前活跃顶级路由
```

### 15.2 守卫链执行顺序

```
全局守卫（按注册顺序） → 单路由守卫（按注册顺序）
任一返回 false → 立即阻止导航
```

### 15.3 生命周期钩子执行顺序（导航时）

```
1. NavigationStarting 事件
2. 守卫链检查（CanNavigateAsync）
   └─ 拒绝 → NavigationFailed 事件 → 返回 Blocked
3. OnNavigatingFrom (当前页)
4. 创建/获取目标页面实例
   └─ KeepAlive → 优先从缓存获取
5. 注入参数（ApplyQueryAttributes）
6. 入栈
7. OnNavigated (目标页)
8. Navigated 事件
```

### 15.4 生命周期钩子执行顺序（返回时）

```
1. NavigationStarting 事件
2. OnNavigatingFrom (当前页)
3. 判断 IsKeepalive
   ├─ true  → 保存到 _keepAliveCache
   └─ false → OnDisposing → Dispose → PageInstance = null
4. 出栈
5. OnNavigatedFrom (旧页)
6. 栈顶处理
   └─ PageInstance == null → 重建实例 → OnNavigated
7. Navigated 事件
```

### 15.5 并发安全

```csharp
public class Router : IRouter
{
    // 导航操作需要加锁，防止并发导航
    private readonly SemaphoreSlim _navigationLock = new(1, 1);

    public async Task<NavigationResult> NavigateAsync(string routeKey, ...)
    {
        await _navigationLock.WaitAsync();
        try
        {
            // 导航逻辑
        }
        finally
        {
            _navigationLock.Release();
        }
    }
}
```

**规则**：
- 同一时刻只允许一个导航操作执行
- 等待中的导航请求会排队
- 返回操作（GoBack）同样受锁保护

### 15.6 DI 注册顺序建议

```csharp
// 建议顺序
services.AddRouting(options =>           // 1. Router 先注册
{
    options.GuardTypes.Add(typeof(AuthGuard));
});
services.AddTransient<HomePage>();                   // 2. 页面后注册
services.AddSingleton<IRouteGuard, AuthGuard>();    // 3. 守卫最后注册
```

---

## 十七、待讨论事项

- [ ] 嵌套路由的栈行为
- [ ] Uri 解析（query string、路径参数）
- [ ] 路由别名 / 重定向
- [ ] UI 元数据（Title、Icon）是否纳入
- [ ] RouteModel 属性是否保留

---

## 十八、里程碑规划

| 阶段 | 内容 | 优先级 | 状态 |
|------|------|--------|------|
| M1 | 基础导航 + 栈管理 + 传参 + AOT 安全 | P0 | ✅ |
| M2 | 路由守卫 + 多守卫支持 | P0 | ✅ |
| M3 | 默认顶级路由兜底 | P0 | ✅ |
| M4 | 生命周期钩子 + KeepAlive 缓存 | P1 | ✅ |
| M5 | 非 KeepAlive 页面销毁机制 | P1 | ✅ |
| M6 | 导航事件通知 + 并发安全 | P1 | ✅ |
| M7 | NavigationActions 导航动作标识 | P1 | ✅ |
| M8 | GoBackToAsync + ReplaceAsync | P1 | ✅ |
| M9 | Uri 解析增强 | P2 | 待定 |

---

*文档版本：v1.4*
*最后更新：2026-04-28*
*状态：核心功能已实现，部分高级特性待定*

## 变更记录

| 版本 | 日期 | 变更内容 |
|------|------|----------|
| v1.0 | 2026-04-23 | 初始版本，核心路由能力定义 |
| v1.1 | 2026-04-23 | 新增第十章：生命周期接口设计决策（INavigationAware vs IQueryAttributable） |
| v1.2 | 2026-04-23 | 删除 ParentId，改为栈历史动态获取父子关系 |
| v1.3 | 2026-04-23 | 新增第八章：实例创建与生命周期（ViewModel 创建、生命周期推断规则、框架层职责） |
| v1.4 | 2026-04-28 | 1. 新增 NavigationActions 导航动作枚举（第六章）<br>2. 更新 IRouter 接口（StackDepth、GoBackToAsync、ReplaceAsync、action 参数）<br>3. 更新 NavigationEventArgs（Action 属性）<br>4. 新增多守卫支持文档（RouterOptions.GuardTypes）<br>5. 更新 KeepAlive 缓存策略（Bug 修复细节）<br>6. 更新核心能力清单（添加状态列）<br>7. 更新里程碑规划状态 |
