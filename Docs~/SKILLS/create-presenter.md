# Skill: 创建新的 Presenter

> 当需要为 UI 组件建立 MVP 中介时，按此指南操作。

## 前置条件

- 确认有 MonoBehaviour 需要展示 UI
- 确认 UI 需要响应 Model 数据变更
- 确认 UI 需要触发写操作（Command）

## 步骤

### 1. 创建 Presenter（UI 场景）

```csharp
using RunLab.AesirArchitecture;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDPresenter : MonoBehaviour, IPresenter
{
    [SerializeField] private Slider mHealthBar;
    [SerializeField] private Text mHPText;
    [SerializeField] private GameObject mDeathPanel;

    private ISubscriptionCollection mSubs = new SubscriptionList();

    public IAppContext Context => GameApp.Instance;
    void IContainerAccess.SetContext(IAppContext context) { }

    private void Start()
    {
        var model = this.GetModel<IPlayerModel>();

        // 数据绑定：HP 变更 → 自动更新血条和文字
        model.HP.AddListener(OnHPChanged).AddTo(mSubs);

        // 事件绑定：玩家死亡 → 显示死亡面板
        this.AddListener<PlayerDeadEvent>(OnPlayerDead).AddTo(mSubs);
    }

    private void OnHPChanged(int hp)
    {
        var maxHP = this.GetModel<IPlayerModel>().MaxHP.Value;
        mHealthBar.value = (float)hp / maxHP;
        mHPText.text = $"{hp} / {maxHP}";
    }

    private void OnPlayerDead(PlayerDeadEvent e) => mDeathPanel.SetActive(true);

    // 用户输入触发 Command
    public void OnAttackButtonClicked(int enemyId)
    {
        this.ExecuteCommand(new HurtCommand(enemyId, attackValue: 10));
    }

    private void OnDestroy() => mSubs.DisposeAll();
}
```

### 2. 非 UI 场景（Service-Driven 模式）

不需要 `IPresenter`，直接通过静态入口访问：

```csharp
public class PlayerInputHandler : MonoBehaviour
{
    private IAppContext App => GameApp.Instance;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            App.ExecuteCommand(new HurtCommand(enemyId: 1, rawAttack: 10));
    }
}
```

## 关键模式

| 模式 | 说明 |
|------|------|
| 只建绑定 | Start 中建立 ObservableProperty + EventBus 绑定，之后不手动 RefreshUI |
| AddTo(mSubs) | 所有订阅添加到集合，OnDestroy 时批量释放 |
| Context 属性 | 返回 `GameApp.Instance`（你的 AppContext 派生类） |
| SetContext 空实现 | Unity 不需要外部设置 Context |

## 约束

| 规则 | 说明 |
|------|------|
| 不缓存 Model 数据 | UI 是 Model 的只读投影，不持有状态副本 |
| 不直接修改 Model | 通过 Command 执行写操作 |
| OnDestroy 释放订阅 | `mSubs.DisposeAll()` 防止内存泄漏 |
| 可用扩展方法 | `this.GetModel<T>()`、`this.ExecuteCommand()` 等 |

## 验证

1. 编译无报错
2. 运行后 UI 正确显示 Model 初始值
3. 修改 Model 数据后 UI 自动更新
4. OnDestroy 后无内存泄漏（订阅已释放）
