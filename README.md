# Aesir Architecture

[![license](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)

`Aesir Architecture` 是一个轻量级 MVP 架构包，以 `AppContext<T>` 为组合根，提供 IoC 依赖注入、类型事件总线、响应式属性和命令模式。原生支持 Data-Driven 开发与 AI Coding Agent。

> **💡 关于命名**：核心类 `AppContext<T>` 取名自 .NET 的 Context 模式（如 `ApplicationContext`、`HttpContext`），表达"应用执行上下文"的语义——它不仅是 IoC 容器，还包含事件总线和命令执行。

## 适用人群

- **游戏架构设计者**：需要为 Unity 项目引入清晰分层架构（MVP）的开发者。
- **Data-Driven 实践者**：希望数据变更自动驱动 UI 更新，消除手动 `RefreshUI()` 的开发者。
- **跨模块协作项目**：多系统之间需要松耦合通信（EventBus）的团队。
- **AI 辅助开发团队**：希望 AI Coding Agent 能理解项目架构并自动生成符合规范的代码的团队。

## 安装说明

### 通过 Git URL 安装

1. 打开 Unity Package Manager 窗口。
2. 点击左上角的 `+` 按钮，选择 `Add package from git URL...`。
3. 输入以下地址：
   ```
   https://github.com/yuumixcode/aesir-architecture.git
   ```

### 通过 manifest.json 安装

在项目的 `Packages/manifest.json` 文件中添加：

```json
{
  "dependencies": {
    "cn.runlab.aesir-architecture": "https://github.com/yuumixcode/aesir-architecture.git"
  }
}
```

## 环境依赖

- **Unity**: 2022.3 或更高版本（Tuanjie 引擎兼容）。
- **无外部依赖**：核心程序集零第三方依赖。

## 核心类型

| 类型 | 职责 |
|------|------|
| `AppContext<T>` | 泛型单例组合根：依赖注册、事件总线、命令执行 |
| `IPresenter` | UI 中介者接口（MVP 的 P） |
| `IModel` / `ModelBase` | 数据+领域逻辑层（MVP 的 M） |
| `IService` / `ServiceBase` | 业务逻辑层，编排 Model + Provider |
| `IProvider` | 基础设施抽象层，零框架依赖 |
| `ICommand` / `CommandBase` | 无状态写操作 |
| `EventBus` | 类型事件总线（AddListener / RemoveListener / Invoke） |
| `ObservableProperty<T>` | 响应式属性，值变更自动通知 |
| `ISubscription` | 订阅句柄，IDisposable 语义 |

### 依赖层级

```
IPresenter → ICommand → IService → IModel → IProvider
```

- **Provider**：零依赖，最底层（基础设施抽象）
- **Model**：依赖 Provider（数据持久化等基础设施）
- **Service**：依赖 Model + Provider（业务逻辑编排）
- **Command**：依赖 Service + Model + Provider（写操作）
- **Presenter**：依赖所有层（UI 中介）

## 快速开始

```csharp
using RunLab.AesirArchitecture;

// 1. 定义事件
public struct PlayerDeadEvent { public int KillerId; }

// 2. 定义 Model
public interface IPlayerModel : IModel
{
    ObservableProperty<int> HP { get; }
    ObservableProperty<int> MaxHP { get; }
}

public class PlayerModel : ModelBase, IPlayerModel
{
    public ObservableProperty<int> HP { get; } = new ObservableProperty<int>(100);
    public ObservableProperty<int> MaxHP { get; } = new ObservableProperty<int>(100);
    protected override void OnInit() { }
}

// 3. 定义 Service
public interface ICombatService : IService
{
    void ApplyDamage(int rawAttack);
}

public class CombatService : ServiceBase, ICombatService
{
    private IPlayerModel mPlayer;
    protected override void OnInit() => mPlayer = GetModel<IPlayerModel>();

    public void ApplyDamage(int rawAttack)
    {
        mPlayer.HP.Value = Mathf.Max(0, mPlayer.HP.Value - rawAttack);
        if (mPlayer.HP.Value <= 0)
            Invoke(new PlayerDeadEvent());
    }
}

// 4. 定义 Command
public class HurtCommand : CommandBase
{
    private readonly int mDamage;
    public HurtCommand(int damage) => mDamage = damage;
    protected override void OnExecute() => GetService<ICombatService>().ApplyDamage(mDamage);
}

// 5. 定义组合根
public class GameApp : AppContext<GameApp>
{
    protected override void Configure()
    {
        RegisterModel<IPlayerModel>(new PlayerModel());
        RegisterService<ICombatService>(new CombatService());
    }
}

// 6. 初始化
GameApp.Init();
```

## Data-Driven MVP

传统 MVP 中，Presenter 需要手动调用 `RefreshUI()` 同步数据。Aesir Architecture 通过 `ObservableProperty<T>` 实现数据驱动——数据变更自动流向 UI：

```csharp
// ✅ Data-Driven：Presenter 只建绑定，数据变更自动更新 UI
public class PlayerHUDPresenter : MonoBehaviour, IPresenter
{
    [SerializeField] private Slider mHealthBar;
    [SerializeField] private Text mHPText;

    public IAppContext Context => GameApp.Instance;
    void IContainerAccess.SetContext(IAppContext context) { }

    private ISubscriptionCollection mSubs = new SubscriptionList();

    private void Start()
    {
        var model = this.GetModel<IPlayerModel>();

        // 建立绑定——之后无需手动 RefreshUI
        model.HP.AddListener(OnHPChanged).AddTo(mSubs);
        this.AddListener<PlayerDeadEvent>(OnPlayerDead).AddTo(mSubs);
    }

    private void OnHPChanged(int hp)
    {
        var maxHP = this.GetModel<IPlayerModel>().MaxHP.Value;
        mHealthBar.value = (float)hp / maxHP;
        mHPText.text = $"{hp} / {maxHP}";
    }

    private void OnPlayerDead(PlayerDeadEvent e) => mDeathPanel.SetActive(true);

    private void OnDestroy() => mSubs.DisposeAll();
}
```

```csharp
// ❌ 传统方式：容易遗漏 RefreshUI()
public class PlayerHUDLegacy : MonoBehaviour
{
    public void OnPlayerHurt()
    {
        GameApp.Instance.ExecuteCommand(new HurtCommand(10));
        RefreshUI(); // ← 容易遗漏
    }

    private void RefreshUI() { /* 手动拉取所有数据 */ }
}
```

## 双模式使用

**模式 1：MVP（UI 场景）** — MonoBehaviour 实现 `IPresenter`，充当中介者。

**模式 2：Service-Driven（非 UI 场景）** — 通过 `AppContext<T>.Instance` 静态入口直接访问容器，无需 `IPresenter`。

```csharp
// 非 UI 场景：直接通过静态入口
public class PlayerInputHandler : MonoBehaviour
{
    private IAppContext App => GameApp.Instance;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            App.ExecuteCommand(new HurtCommand(10));
    }
}
```

## Service vs Provider

判断标准：**接口参数中是否出现游戏业务概念**。

| 问题 | Provider | Service |
|------|----------|---------|
| 接口里有业务概念？ | ❌ | ✅ |
| 换项目能复用？ | ✅ | ❌ |
| 决定游戏规则？ | ❌ | ✅ |

```csharp
// Provider — 接口参数无业务概念，可跨项目复用
public interface IStorageProvider : IProvider
{
    void Save(string key, string value);
    string Load(string key, string defaultValue = "");
}

// Service — 接口参数含业务概念，决定游戏规则
public interface ICombatService : IService
{
    void ApplyDamage(int targetId, int rawAttack);
}
```

## 订阅生命周期

`ISubscription` 实现 `IDisposable`，支持 `using` 语句和批量释放：

```csharp
// 方式 1：AddTo 批量释放
model.HP.AddListener(OnHPChanged).AddTo(mSubs);
// OnDestroy 时：mSubs.DisposeAll();

// 方式 2：Unity 生命周期自动注销
model.HP.AddListener(OnHPChanged).DisposeWhenDestroyed(gameObject);

// 方式 3：using 语句
using (context.AddListener<PlayerDeadEvent>(OnPlayerDead))
{
    // 作用域结束自动注销
}
```

## 命名对照

从 QFramework / AesirFramework 迁移时的命名变更：

| 旧命名 | 新命名 | 说明 |
|--------|--------|------|
| `Architecture<T>` | `AppContext<T>` | Context 比 Container 更准确 |
| `IAppContainer` | `IAppContext` | 与类名一致 |
| `.Container` | `.Context` | 属性重命名 |
| `IController` | `IPresenter` | MVP 标准术语 |
| `ISystem` | `IService` | .NET 标准命名 |
| `IUtility` | `IProvider` | Provider 模式命名 |
| `SendCommand()` | `ExecuteCommand()` | Command 是被执行的 |
| `SendEvent()` | `Invoke()` | 对齐 UnityEvent.Invoke() |
| `RegisterEvent()` | `AddListener()` | 对齐 UnityEvent.AddListener() |
| `IUnRegister` | `ISubscription : IDisposable` | .NET 资源管理惯例 |
| `BindableProperty<T>` | `ObservableProperty<T>` | 对齐 IObservable 模式 |

## AI 原生支持

本包内置 AI Agent 文档体系，AI Coding Agent 可自动发现并理解项目上下文：

- **AGENTS.md** — AI Agent 入口（Hot Memory），包含核心规则和快速参考
- **CODELY.md** — 项目上下文（Hot Memory），包含完整的项目信息和开发规范
- **Docs~/** — 深度上下文（Cold Memory）：
  - `ARCHITECTURE.md` — C4 架构模型
  - `CONVENTIONS.md` — 代码风格与命名规范
  - `MODULES.md` — 模块级 API 文档
  - `ADR/` — 架构决策记录
  - `SKILLS/` — 任务专项技能指南（创建 Model/Service/Provider/Command/Presenter、配置 AppContext）

## 从 Aesir Modules 迁移

| 旧值 | 新值 |
|------|------|
| 包名 `cn.runlab.aesir-modules` | `cn.runlab.aesir-architecture` |
| 命名空间 `RunLab.AesirModules.Architecture` | `RunLab.AesirArchitecture` |
| 测试命名空间 `RunLab.AesirModules.Tests` | `RunLab.AesirArchitecture.Tests` |
| 程序集 `RunLab.AesirModules` | `RunLab.AesirArchitecture` |
| Git URL `aesir-modules` | `aesir-architecture` |

## 许可协议

本项目采用 MIT 协议开源。详情请参阅 [LICENSE.md](LICENSE.md)。
