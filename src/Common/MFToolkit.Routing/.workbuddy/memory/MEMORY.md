# MFToolkit.Routing 项目记忆

## 项目概述

**MFToolkit.Routing** 是一个框架无关的 .NET 路由导航库，面向 Avalonia、WPF、MAUI 等 UI 框架。

### 技术栈
- .NET 10.0
- C# + Nullable + ImplicitUsings
- Microsoft.Extensions.DependencyInjection.Abstractions

### 核心设计
- Router 不直接操作 UI，只负责路由逻辑
- UI 框架通过订阅事件来感知路由变化
- 完整支持 AOT 编译
- 纯手动路由注册，不支持自动扫描

## 项目结构

```
MFToolkit.Routing/
├── Core/
│   ├── Interfaces/
│   │   ├── IQueryAttributable.cs     # 路由参数接口
│   │   ├── IRouteGuard.cs            # 路由守卫接口
│   │   ├── INavigationAware.cs       # 生命周期钩子
│   │   └── IRouter.cs                # 路由器接口
│   ├── NavigationActions.cs            # 导航动作常量（Push/Pop/Replace等）
│   ├── NavigationEventArgs.cs         # 导航事件参数
│   ├── NavigationResult.cs            # 导航结果
│   ├── NavigationStatus.cs            # 导航状态枚举
│   ├── RouteRegistry.cs              # 路由注册表
│   ├── RouteStack.cs                 # 路由栈
│   ├── RouteStackManager.cs          # 栈管理器
│   └── Router.cs                     # 路由器实现
├── DependencyInjection/
│   └── RoutingExtensions.cs          # DI 扩展
├── Entities/
│   ├── RouteEntity.cs                # 路由实体
│   └── RouteEntry.cs                 # 路由条目
├── REQUIREMENTS.md                    # 需求规格文档
└── README.md                          # 使用文档
```

## 关键接口

| 接口 | 用途 |
|------|------|
| `IRouter` | 核心导航接口 |
| `IRouteGuard` | 路由守卫 |
| `INavigationAware` | 生命周期钩子 |
| `IQueryAttributable` | 路由参数接收 |

## 重要特性

1. **默认顶级路由兜底**：无顶级路由时自动创建内部默认路由
2. **并发安全**：导航操作使用 SemaphoreSlim 锁保护
3. **KeepAlive 支持**：非活跃页面实例可销毁，下次重建
4. **守卫链**：多个守卫按顺序执行
5. **父子关系动态获取**：无需配置 ParentId，从栈历史自动推断
6. **ViewModel 支持**：RouteEntity 可选注册 ViewModelType，Router 内部创建

## 生命周期推断规则

| 路由类型 | Page 生命周期 | ViewModel 生命周期 |
|----------|--------------|-------------------|
| 顶级路由 (`IsTop = true`) | Singleton | Singleton |
| KeepAlive | Transient + 缓存 | Transient + 缓存 |
| 普通页面 | Transient | Transient |

## DI 自动注册

- **RoutingExtensions.AddRoutes** 会自动将 RouteType 和 ViewModelType 注册到 DI 容器
- 生命周期根据 IsTop 推断（顶级=Singleton，其他=Transient）
- Router 构造函数接收 IServiceProvider 用于创建 ViewModel

## 职责划分

| 谁做 | 做什么 |
|------|--------|
| **Router** | 从 DI 获取 Page 和 ViewModel，触发 INavigationAware，触发 IQueryAttributable |
| **框架层** | 绑定 ViewModel 到 BindingContext，处理 UI 切换 |

## 开发记录

- 2026-04-23：完成 MFToolkit.Routing 核心开发
  - 完成需求讨论和规格文档
  - 实现 RouteEntity、RouteEntry、RouteStack 等核心类
  - 实现 Router 路由器（含导航、返回、守卫、生命周期钩子）
  - 完成 DI 扩展
  - 编写使用文档 README.md
- 2026-04-23（晚）：重构接口文件结构
  - INavigationAware、IRouter 移动到 Core/Interfaces/ 文件夹，与 IQueryAttributable 同级
- 2026-04-23（晚）：新增 NavigationActions 枚举
  - 提供 Push/Pop/PopToRoot/PopToPage/Replace/SwitchTop 常量
  - NavigationEventArgs 新增 Action 属性
  - Router 新增 GoBackToAsync、ReplaceAsync 方法
  - IRouter 新增 StackDepth 属性
  - UI 框架可根据 Action 类型决定如何显示（普通 push 还是 modal 等）
- 2026-04-23（夜）：Router 从 DI 获取 PageInstance 和 ViewModelInstance
  - 修复：NavigateInternalAsync 和 ReplaceAsync 中使用 GetRequiredService 获取 PageInstance
  - AddRoutes 保证 PageType/ViewModelType 已注册到 DI
- 2026-04-23（夜）：内部类型访问级别收紧
  - 删除 Router 的 RegisterRoute/RegisterRoutes 方法（已无用）
  - RouteRegistry 改为 internal
  - Router.Registry、Router.StackManager 改为 internal
  - IStartupRouteRegistration 改为 public（供 Router 构造函数使用）
  - 路由完全通过 AddRoutes 注册，Router 构造函数接收路由列表参数
