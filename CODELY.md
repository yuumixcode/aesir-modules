# Aesir Architecture — Project Context

## Project Overview

Aesir Architecture (`cn.runlab.aesir-architecture`) 是一个轻量级 MVP 架构包，以 `AppContext<T>` 为组合根，提供 IoC 依赖注入、类型事件总线、响应式属性、命令模式，并原生支持 Data-Driven 开发。当前版本 **0.3.0**，MIT 协议开源。

- **构建版本**: Unity 2022.3 或更高版本
- **核心依赖**: 无（零外部依赖）
- **仓库**: https://github.com/yuumixcode/aesir-architecture

## Package Identity

| Field        | Value                           |
|--------------|---------------------------------|
| Package Name | `cn.runlab.aesir-architecture` |
| Display Name | Aesir Architecture              |
| Version      | 0.3.0                           |
| Category     | Architecture                    |
| License      | MIT                             |

## Assembly Definitions

本项目使用 4 个程序集：

| Asmdef                                      | Namespace                                   | Platforms   | References                                  | defineConstraints        |
|---------------------------------------------|---------------------------------------------|-------------|---------------------------------------------|--------------------------|
| `RunLab.AesirArchitecture`                  | `RunLab.AesirArchitecture`                 | Any         | (none)                                      | (none)                   |
| `RunLab.AesirArchitecture.Editor`           | `RunLab.AesirArchitecture.Editor`           | Editor only | Runtime                                     | (none)                   |
| `RunLab.AesirArchitecture.Tests`           | `RunLab.AesirArchitecture.Tests`            | Any         | Runtime + TestRunner                        | `UNITY_INCLUDE_TESTS`    |
| `RunLab.AesirArchitecture.Editor.Tests`     | `RunLab.AesirArchitecture.Editor.Tests`     | Editor only | Runtime + Editor + TestRunner               | `UNITY_INCLUDE_TESTS`    |

**架构说明**：
- 所有架构代码直接放在 `Runtime/` 下，属于 `RunLab.AesirArchitecture` 程序集
- 命名空间为 `RunLab.AesirArchitecture`，与程序集根命名空间一致
- 测试代码使用独立程序集，带有 `UNITY_INCLUDE_TESTS` 约束

## Directory Structure

```
Aesir Architecture/
├── Runtime/                               # 标准运行时根目录 (RunLab.AesirArchitecture)
│   ├── IAppContext.cs                     # 组合根接口
│   ├── AppContext.cs                       # 组合根泛型单例实现
│   ├── IContainerAccess.cs                 # 容器访问 + 生命周期接口
│   ├── IPresenter.cs                       # Presenter 接口 + 扩展方法
│   ├── IModel.cs                           # Model 接口 + ModelBase
│   ├── IService.cs                         # Service 接口 + ServiceBase
│   ├── IProvider.cs                        # Provider 接口
│   ├── ICommand.cs                         # Command 接口 + CommandBase
│   ├── ServiceContainer.cs                 # IoC 容器
│   ├── EventBus.cs                         # 类型事件总线
│   ├── ISubscription.cs                    # 订阅接口 + 集合
│   ├── ObservableProperty.cs               # 响应式属性
│   ├── Event.cs                            # 事件类 + 事件集合
│   ├── IEventCallback.cs                   # 事件回调接口
│   └── SubscriptionExtensions.cs          # Unity 生命周期订阅扩展
├── Editor/                                # 标准编辑器根目录
├── Tests/
│   ├── Runtime/                            # 运行时测试
│   │   └── Architecture/
│   │       └── AppContextTests.cs          # 架构核心测试
│   └── Editor/                             # 编辑器测试
├── Samples/                                # 示例
├── Documentation~/                         # Unity 标准用户文档
├── Docs~/                                  # AI Agent 冷记忆层
│   ├── ARCHITECTURE.md                     # 系统架构 & C4 模型
│   ├── CONVENTIONS.md                      # 代码风格完整规范
│   ├── MODULES.md                          # 模块级 API 文档
│   ├── ADR/                                # 架构决策记录
│   └── SKILLS/                             # 任务专项技能指南
├── AGENTS.md                               # AI Agent 通用入口 (Hot Memory)
├── CODELY.md                               # 项目上下文 (Hot Memory)
├── package.json
├── README.md
├── CHANGELOG.md
├── LICENSE.md
└── Third Party Notices.md
```

## Core Types

| 类型 | 职责 |
|------|------|
| `IAppContext` / `AppContext<T>` | 组合根：依赖注册、事件总线、命令执行 |
| `IPresenter` | UI 中介者接口，MVP 的 P |
| `IModel` / `ModelBase` | 数据+领域逻辑层，MVP 的 M |
| `IService` / `ServiceBase` | 业务逻辑层，编排 Model + Provider |
| `IProvider` | 基础设施层，零框架依赖 |
| `ICommand` / `CommandBase` | 无状态写操作 |
| `EventBus` | 类型事件总线（AddListener/RemoveListener/Invoke） |
| `ObservableProperty<T>` | 响应式属性，值变更自动通知 |
| `ISubscription` | 事件订阅句柄，IDisposable |
| `ServiceContainer` | 最小 IoC 容器 |

## MVP Architecture

### 依赖层级

```
IPresenter → ICommand → IService → IModel → IProvider
                      ↘ IModel ↘ IProvider
```

- **Provider**: 零依赖，最底层（基础设施抽象）
- **Model**: 依赖 Provider（数据持久化等基础设施）
- **Service**: 依赖 Model + Provider（业务逻辑编排）
- **Command**: 依赖 Service + Model + Provider（写操作）
- **Presenter**: 依赖所有层（UI 中介）

### Data-Driven 模式

- `ObservableProperty<T>` 实现属性级数据驱动：值变更自动通知
- `EventBus` 实现事件级数据驱动：领域事件跨模块传播
- Presenter 只建绑定，不写 `RefreshUI()`
- Command 是 Model 修改的唯一入口

### 双模式使用

1. **MVP 模式**（UI 场景）：MonoBehaviour 实现 `IPresenter`，充当中介者
2. **Service-Driven 模式**（非 UI 场景）：通过 `AppContext<T>.Instance` 静态入口直接访问

## Development Conventions

### 命名规范

- **核心类**: `AppContext<T>`（不是 `AppContainer<T>` 或 `Architecture<T>`）
- **属性访问**: `.Context`（不是 `.Container`）
- **方法**: `.SetContext()`（不是 `.SetContainer()`）
- **事件**: `Invoke` / `AddListener` / `RemoveListener`（对齐 UnityEvent）
- **命令**: `ExecuteCommand`（不是 `SendCommand`）
- **响应式**: `ObservableProperty<T>`（不是 `BindableProperty<T>`）
- **订阅**: `ISubscription : IDisposable`（不是 `IUnRegister`）
- **静默设值**: `SetValueSilently`（不是 `SetValueWithoutEvent`）
- **生命周期**: `DisposeWhenDestroyed` / `DisposeWhenDisabled`（不是 `UnRegisterWhen*`）

### Unity 禁忌

- 严禁对 `UnityEngine.Object` 派生类使用 `?.` 或 `??`
- 严禁在 `Update` 中调用 `GetComponent`、`Find`、字符串拼接、LINQ

### Service vs Provider 决策

- **Provider**: 接口参数无游戏业务概念 → 可跨项目复用 → 不决定游戏规则
- **Service**: 接口参数含游戏业务概念 → 不可跨项目复用 → 决定游戏规则

## Testing

- **Framework**: NUnit (Unity Test Framework)
- **Runtime Tests**: `Tests/Runtime/Architecture/AppContextTests.cs`
- **运行方式**: Unity Test Runner → Play Mode

## Version Control

`Library/`、`Temp/`、`obj/`、`Build/` 应在项目级 `.gitignore` 中排除。

## Important Notes

- **零外部依赖** — `RunLab.AesirArchitecture` 不依赖任何第三方库
- **AppContext 命名** — 核心类是 `AppContext<T>`，不是 `AppContainer<T>`。这来自 AesirFramework 的命名优化，"Context" 比 "Container" 更准确地表达"应用执行上下文"的语义
- **MVP 标准术语** — 使用 `IPresenter` 而非 `IController`，与 Unity 官方 MVP 教程一致
- **包名语义** — 使用 `Aesir Architecture` 而非 `Aesir Modules`，因为本包专注于 MVP 架构模式，不是大量功能模块的集合
- **AI Agent 文档体系** — `AGENTS.md` 为 Hot Memory 入口，`Docs~/` 为 Cold Memory 冷记忆层，详见 `Docs~/ARCHITECTURE.md`
