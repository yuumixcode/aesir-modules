# Aesir Architecture

Aesir 开发功能模块包，提供核心 MVP 架构模块，支持 IoC 依赖注入、类型事件总线、响应式属性和命令模式。

## Architecture 模块

### 概述

Architecture 模块是一个轻量级 MVP（Model-View-Presenter）框架，以 `AppContext<T>` 为组合根，提供：

- **IoC 依赖注入** — ServiceContainer 管理所有 Model、Service、Provider 实例
- **类型事件总线** — EventBus 提供 AddListener / RemoveListener / Invoke 事件机制
- **响应式属性** — ObservableProperty<T> 值变更自动通知订阅者
- **命令模式** — ICommand / CommandBase 封装无状态写操作
- **Data-Driven** — 数据变更自动驱动 UI 更新，无需手动 RefreshUI

### 核心类型

| 类型 | 职责 |
|------|------|
| `AppContext<T>` | 组合根：依赖注册、事件总线、命令执行 |
| `IPresenter` | UI 中介者接口 |
| `IModel` / `ModelBase` | 数据+领域逻辑层 |
| `IService` / `ServiceBase` | 业务逻辑层 |
| `IProvider` | 基础设施抽象层 |
| `ICommand` / `CommandBase` | 无状态写操作 |
| `EventBus` | 类型事件总线 |
| `ObservableProperty<T>` | 响应式属性 |

### 依赖层级

```
IPresenter → ICommand → IService → IModel → IProvider
```

- **Provider**: 零依赖，最底层
- **Model**: 依赖 Provider
- **Service**: 依赖 Model + Provider
- **Command**: 依赖 Service + Model + Provider
- **Presenter**: 依赖所有层

### 快速开始

```csharp
using RunLab.AesirArchitecture;

// 1. 定义组合根
public class GameApp : AppContext<GameApp>
{
    protected override void Configure()
    {
        RegisterProvider<IStorageProvider>(new PlayerPrefsStorageProvider());
        RegisterModel<IPlayerModel>(new PlayerModel());
        RegisterService<ICombatService>(new CombatService());
    }
}

// 2. 初始化
GameApp.Init();

// 3. 定义 Presenter
public class PlayerHUDPresenter : MonoBehaviour, IPresenter
{
    public IAppContext Context => GameApp.Instance;
    void IContainerAccess.SetContext(IAppContext context) { }

    private ISubscriptionCollection mSubs = new SubscriptionList();

    private void Start()
    {
        var model = this.GetModel<IPlayerModel>();
        model.HP.AddListener(hp => mHealthBar.value = hp).AddTo(mSubs);
    }

    private void OnDestroy() => mSubs.DisposeAll();
}
```

## AI 原生支持

本包内置 AI Agent 文档体系：

- **AGENTS.md** — AI Agent 入口（Hot Memory）
- **CODELY.md** — 项目上下文（Hot Memory）
- **Docs~/** — 架构、规范、模块文档 + ADR + SKILLS（Cold Memory）
