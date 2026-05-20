# Architecture

> Aesir Architecture 系统架构文档。基于 C4 模型，面向 AI Agent 设计。

## C1 — System Context

```mermaid
graph TB
    Dev[开发者] --> AICode[AI Coding Agent]
    AICode --> AI[Aesir Architecture<br/>Agent Context Layer]
    Dev --> Editor[Tuanjie Editor]
    Editor --> PKG[Aesir Architecture Package]
    PKG --> Unity[Unity/Tuanjie API]
```

**外部系统**：

| 系统 | 关系 | 说明 |
|------|------|------|
| Tuanjie Editor | 运行平台 | Unity 2022.3 分支，场景文件 `.scene` |
| AI Coding Agents | 消费者 | 读取 AGENTS.md + Docs~/ 获取项目上下文 |

## C2 — Container

```mermaid
graph TB
    subgraph "Aesir Architecture Package"
        subgraph Runtime["Runtime/ (运行时)"]
            Arch["Architecture/<br/>MVP 架构"]
        end
        Editor["Editor/"]
        Tests["Tests/"]
        Samples["Samples/"]
    end

    Arch -->|"依赖"| Unity[Unity/Tuanjie API]
```

**程序集依赖图**：

```mermaid
graph LR
    RT[RunLab.AesirArchitecture] -->|"无外部依赖"| None[ ]
    ED[RunLab.AesirArchitecture.Editor] -->|引用| RT
    TEST[RunLab.AesirArchitecture.Tests] -->|引用| RT
    ETEST[...Editor.Tests] -->|引用| RT
    ETEST -->|引用| ED
```

## C3 — Component

### Runtime/ — MVP 架构

| 组件 | 文件 | 职责 |
|------|------|------|
| `IAppContext` | `IAppContext.cs` | 组合根接口：依赖注册、事件总线、命令执行 |
| `AppContext<T>` | `AppContext.cs` | 泛型单例组合根实现 |
| `IContainerAccess` | `IContainerAccess.cs` | 容器访问契约（Context 属性 + SetContext 方法） |
| `IInitializable` | `IContainerAccess.cs` | 生命周期契约（Init/Deinit） |
| `IPresenter` | `IPresenter.cs` | UI 中介者接口 |
| `PresenterExtensions` | `IPresenter.cs` | Presenter 便捷扩展方法 |
| `IModel` / `ModelBase` | `IModel.cs` | 数据+领域逻辑层 |
| `IService` / `ServiceBase` | `IService.cs` | 业务逻辑层 |
| `IProvider` | `IProvider.cs` | 基础设施抽象层 |
| `ICommand` / `CommandBase` | `ICommand.cs` | 无状态写操作 |
| `ServiceContainer` | `ServiceContainer.cs` | 最小 IoC 容器 |
| `EventBus` | `EventBus.cs` | 类型事件总线 |
| `ISubscription` | `ISubscription.cs` | 订阅句柄 + 集合 |
| `ObservableProperty<T>` | `ObservableProperty.cs` | 响应式属性 |
| `Event` / `Event<T>` | `Event.cs` | 事件类 + 事件集合 |
| `IEventCallback<T>` | `IEventCallback.cs` | 事件回调接口 |
| `SubscriptionExtensions` | `SubscriptionExtensions.cs` | Unity 生命周期自动注销 |

## MVP 依赖层级

```mermaid
graph TB
    P["IPresenter<br/>UI 中介"] --> Cmd["ICommand<br/>写操作"]
    Cmd --> Svc["IService<br/>业务逻辑"]
    Svc --> Mdl["IModel<br/>数据+领域"]
    Mdl --> Prov["IProvider<br/>基础设施"]

    Svc -.->|"也可以直接使用"| Prov

    style Prov fill:#ffcc80,stroke:#e65100,color:#000
    style Mdl fill:#a5d6a7,stroke:#2e7d32,color:#000
    style Svc fill:#90caf9,stroke:#1565c0,color:#000
    style Cmd fill:#ce93d8,stroke:#6a1b9a,color:#000
    style P fill:#4fc3f7,stroke:#0288d1,color:#000
```

## MVP 数据流

```mermaid
sequenceDiagram
    participant V as View (MonoBehaviour)
    participant P as IPresenter
    participant Cmd as Command
    participant Svc as Service
    participant Mdl as Model
    participant OP as ObservableProperty
    participant Bus as EventBus

    V->>P: 用户输入回调
    P->>Cmd: ExecuteCommand(new HurtCommand(10))
    Cmd->>Svc: GetService&lt;ICombatService&gt;().ApplyDamage(10)
    Svc->>Mdl: HP.Value -= 10
    Mdl->>OP: 值变更 → Trigger
    OP-->>P: AddListener 回调
    P->>V: 更新 UI 显示
```

## 双模式架构

```mermaid
graph TB
    subgraph "模式 1: MVP（UI 场景）"
        V1["View<br/>MonoBehaviour + UI"] -->|"用户输入"| P1["IPresenter<br/>UI 中介者"]
        P1 -->|"更新 UI"| V1
        P1 -->|"ExecuteCommand"| Cmd1["ICommand"]
    end

    subgraph "模式 2: Service-Driven（非 UI 场景）"
        MB["MonoBehaviour<br/>输入/碰撞/生命周期"] -->|"AppContext.Instance"| AC["IAppContext"]
        AC -->|"ExecuteCommand"| Cmd2["ICommand"]
    end

    Cmd1 --> Svc["IService"]
    Cmd2 --> Svc
    Svc --> Mdl["IModel"]
    Mdl -->|"ObservableProperty<br/>+ EventBus"| EVT["EventBus"]
    EVT -.->|"可选：通知 UI"| P1
```
