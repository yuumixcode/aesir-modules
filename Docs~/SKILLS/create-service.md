# Skill: 创建新的 Service

> 当需要新增业务逻辑编排层时，按此指南操作。

## 前置条件

- 确认业务逻辑需要读写 Model 数据
- 确认接口参数包含游戏业务概念（Player、Item、Score 等）
- 确认逻辑是"游戏规则"而非"基础设施"

## 步骤

### 1. 定义接口

```csharp
using RunLab.AesirArchitecture;

public interface ICombatService : IService
{
    void ApplyDamage(int targetId, int rawAttack);
    void Heal(int targetId, int amount);
}
```

- 接口命名：`I{Name}Service`
- 方法命名反映业务操作

### 2. 实现类

```csharp
public class CombatService : ServiceBase, ICombatService
{
    private IPlayerModel mPlayer;
    private ICombatFormulaProvider mFormula;

    protected override void OnInit()
    {
        mPlayer = GetModel<IPlayerModel>();
        mFormula = GetProvider<ICombatFormulaProvider>();
    }

    public void ApplyDamage(int targetId, int rawAttack)
    {
        var damage = mFormula.CalculateDamage(rawAttack, defense: 10);
        mPlayer.HP.Value = Mathf.Max(0, mPlayer.HP.Value - damage);

        if (mPlayer.HP.Value <= 0)
            Invoke(new PlayerDeadEvent { KillerId = targetId });
    }

    public void Heal(int targetId, int amount)
    {
        mPlayer.HP.Value = Mathf.Min(mPlayer.MaxHP.Value, mPlayer.HP.Value + amount);
    }
}
```

- 类命名：`{Name}Service`
- 在 `OnInit()` 中获取 Model 和 Provider 引用
- 业务逻辑编排 Model + Provider
- 领域事件通过 `Invoke<TEvent>()` 发布

### 3. 注册到 AppContext

```csharp
public class GameApp : AppContext<GameApp>
{
    protected override void Configure()
    {
        RegisterService<ICombatService>(new CombatService());
    }
}
```

## 约束

| 规则 | 说明 |
|------|------|
| 可访问 IModel | 读写数据 |
| 可访问 IService | 编排其他 Service |
| 可访问 IProvider | 调用基础设施 |
| 可发布/监听事件 | `Invoke` / `AddListener` |
| 必须在 OnInit 中获取引用 | 不在构造函数中访问 Model |

## 验证

1. 编译无报错
2. `GameApp.Instance.GetService<ICombatService>()` 返回非 null
3. 调用 `ApplyDamage()` 后 Model 数据变更
