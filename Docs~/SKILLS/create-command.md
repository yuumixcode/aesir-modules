# Skill: 创建新的 Command

> 当需要封装一个无状态写操作时，按此指南操作。

## 前置条件

- 确认操作是"写"而非"读"（读 → 直接 `GetModel<T>()`）
- 确认操作需要通过 Presenter 触发（保持 Presenter 精简）
- 确认是否需要返回值（需要 → `ICommand<TResult>`）

## 步骤

### 1. 无返回值的 Command

```csharp
using RunLab.AesirArchitecture;

public class HurtCommand : CommandBase
{
    private readonly int mAttackerId;
    private readonly int mRawAttack;

    public HurtCommand(int attackerId, int rawAttack)
    {
        mAttackerId = attackerId;
        mRawAttack = rawAttack;
    }

    protected override void OnExecute()
    {
        GetService<ICombatService>().ApplyDamage(mAttackerId, mRawAttack);
    }
}
```

### 2. 有返回值的 Command

```csharp
public class CalculateDamageCommand : CommandBase<int>
{
    private readonly int mAttack;
    private readonly int mDefense;

    public CalculateDamageCommand(int attack, int defense)
    {
        mAttack = attack;
        mDefense = defense;
    }

    protected override int OnExecute()
    {
        var formula = GetProvider<ICombatFormulaProvider>();
        return formula.CalculateDamage(mAttack, mDefense);
    }
}
```

### 3. 在 Presenter 中执行

```csharp
// 无返回值
this.ExecuteCommand(new HurtCommand(enemyId, attackValue));

// 无参数快捷方式（需 new() 约束）
this.ExecuteCommand<ResetCommand>();

// 有返回值
var damage = this.ExecuteCommand(new CalculateDamageCommand(10, 5));
```

### 4. 非 Presenter 中执行

```csharp
// 通过 AppContext.Instance
GameApp.Instance.ExecuteCommand(new HurtCommand(enemyId, attackValue));
```

## 约束

| 规则 | 说明 |
|------|------|
| 无状态 | Command 不持有 Model 引用，数据通过构造函数传入 |
| OnExecute 中访问容器 | 通过 `GetModel<T>()` / `GetService<T>()` 等 |
| 可访问所有层 | Model + Service + Provider + Events |
| 不可嵌套 ExecuteCommand | 避免命令链（但框架不强制） |

## 验证

1. 编译无报错
2. 通过 `ExecuteCommand` 触发后，Model 数据确实变更
