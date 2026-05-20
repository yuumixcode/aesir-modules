# Skill: 创建新的 Model

> 当需要新增一个数据+领域逻辑层组件时，按此指南操作。

## 前置条件

- 确认需要持久化的数据属性（HP、分数、位置等）
- 确认数据变更是否需要通知 UI（需要 → 用 `ObservableProperty<T>`）
- 确认是否需要基础设施（需要 → 依赖 IProvider）

## 步骤

### 1. 定义接口

```csharp
using RunLab.AesirArchitecture;

public interface IPlayerModel : IModel
{
    ObservableProperty<string> Name { get; }
    ObservableProperty<int> HP { get; }
    ObservableProperty<int> MaxHP { get; }
    ObservableProperty<Vector3> Position { get; }
}
```

- 接口命名：`I{Name}Model`
- 属性用 `ObservableProperty<T>`（只读 `get;`）
- 可选：普通属性（非响应式）也可定义

### 2. 实现类

```csharp
public class PlayerModel : ModelBase, IPlayerModel
{
    public ObservableProperty<string> Name { get; } = new ObservableProperty<string>("Player");
    public ObservableProperty<int> HP { get; } = new ObservableProperty<int>(100);
    public ObservableProperty<int> MaxHP { get; } = new ObservableProperty<int>(100);
    public ObservableProperty<Vector3> Position { get; } = new ObservableProperty<Vector3>();

    protected override void OnInit() { }
}
```

- 类命名：`{Name}Model`
- `ObservableProperty` 在声明时初始化（含默认值）
- `OnInit()` 用于初始化逻辑（通常为空）
- **不可**访问 Service（保持数据层纯净）
- 可通过 `GetProvider<T>()` 访问基础设施

### 3. 注册到 AppContext

```csharp
public class GameApp : AppContext<GameApp>
{
    protected override void Configure()
    {
        RegisterModel<IPlayerModel>(new PlayerModel());
    }
}
```

## 约束

| 规则 | 说明 |
|------|------|
| 不可访问 IService | Model 是纯数据层，不能依赖业务逻辑 |
| 可访问 IProvider | 数据持久化等基础设施通过 Provider |
| 可发布事件 | `Invoke<TEvent>()` 通知领域事件 |
| 属性用 ObservableProperty | 值变更自动通知 UI |

## 验证

1. 编译无报错
2. `GameApp.Instance.GetModel<IPlayerModel>()` 返回非 null
3. 修改 `HP.Value` 触发 AddListener 回调
