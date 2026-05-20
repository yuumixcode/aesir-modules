# Modules

> Aesir Architecture 模块级文档。每个类型的职责、API 签名和用法。

---

## Runtime/ — MVP 架构

**命名空间**: `RunLab.AesirArchitecture`

### IAppContext

**文件**: `IAppContext.cs`

组合根接口，应用级单一入口。

```csharp
public interface IAppContext
{
    void RegisterService<T>(T service) where T : IService;
    void RegisterModel<T>(T model) where T : IModel;
    void RegisterProvider<T>(T provider) where T : IProvider;

    T GetService<T>() where T : class, IService;
    T GetModel<T>() where T : class, IModel;
    T GetProvider<T>() where T : class, IProvider;

    void ExecuteCommand<T>(T command) where T : ICommand;
    TResult ExecuteCommand<TResult>(ICommand<TResult> command);

    void Invoke<T>() where T : new();
    void Invoke<T>(T e);

    ISubscription AddListener<T>(Action<T> onEvent);
    void RemoveListener<T>(Action<T> onEvent);

    void Deinit();
}
```

### AppContext\<T\>

**文件**: `AppContext.cs`

泛型单例组合根。派生此类并在 `Configure()` 中注册依赖。

```csharp
public abstract class AppContext<T> : IAppContext where T : AppContext<T>, new()
{
    public static IAppContext Instance { get; }
    public static void Init();
    public static Action<T> OnRegisterPatch;

    protected abstract void Configure();
    protected virtual void OnDeinit();
}
```

**用法**：

```csharp
public class GameApp : AppContext<GameApp>
{
    protected override void Configure()
    {
        RegisterProvider<IStorageProvider>(new PlayerPrefsStorageProvider());
        RegisterModel<IPlayerModel>(new PlayerModel());
        RegisterService<ICombatService>(new CombatService());
    }
}

// 初始化
GameApp.Init();

// 访问
var model = GameApp.Instance.GetModel<IPlayerModel>();
```

### IContainerAccess / IInitializable

**文件**: `IContainerAccess.cs`

```csharp
public interface IContainerAccess
{
    IAppContext Context { get; }
    void SetContext(IAppContext context);
}

public interface IInitializable
{
    bool Initialized { get; set; }
    void Init();
    void Deinit();
}
```

### IPresenter / PresenterExtensions

**文件**: `IPresenter.cs`

```csharp
public interface IPresenter : IContainerAccess { }

public static class PresenterExtensions
{
    public static T GetModel<T>(this IPresenter self) where T : class, IModel;
    public static T GetService<T>(this IPresenter self) where T : class, IService;
    public static T GetProvider<T>(this IPresenter self) where T : class, IProvider;
    public static void ExecuteCommand<T>(this IPresenter self, T command) where T : ICommand;
    public static void ExecuteCommand<T>(this IPresenter self) where T : ICommand, new();
    public static TResult ExecuteCommand<TResult>(this IPresenter self, ICommand<TResult> command);
    public static void Invoke<TEvent>(this IPresenter self) where TEvent : new();
    public static void Invoke<TEvent>(this IPresenter self, TEvent e);
    public static ISubscription AddListener<TEvent>(this IPresenter self, Action<TEvent> onEvent);
    public static void RemoveListener<TEvent>(this IPresenter self, Action<TEvent> onEvent);
}
```

### IModel / ModelBase

**文件**: `IModel.cs`

```csharp
public interface IModel : IContainerAccess, IInitializable { }

public abstract class ModelBase : IModel
{
    // 便捷访问器：Provider + Events
    public T GetProvider<T>() where T : class, IProvider;
    public void Invoke<TEvent>() where TEvent : new();
    public void Invoke<TEvent>(TEvent e);

    protected abstract void OnInit();
    protected virtual void OnDeinit();
}
```

**约束**：Model **不可**访问 Service（保持数据层纯净）。

### IService / ServiceBase

**文件**: `IService.cs`

```csharp
public interface IService : IContainerAccess, IInitializable { }

public abstract class ServiceBase : IService
{
    // 便捷访问器：Model + Service + Provider + Events
    public T GetModel<T>() where T : class, IModel;
    public T GetService<T>() where T : class, IService;
    public T GetProvider<T>() where T : class, IProvider;
    public void Invoke<TEvent>() where TEvent : new();
    public void Invoke<TEvent>(TEvent e);
    public ISubscription AddListener<TEvent>(Action<TEvent> onEvent);

    protected abstract void OnInit();
    protected virtual void OnDeinit();
}
```

### IProvider

**文件**: `IProvider.cs`

```csharp
public interface IProvider { }
```

**特征**：零框架依赖，无生命周期，纯接口标记。

### ICommand / CommandBase

**文件**: `ICommand.cs`

```csharp
public interface ICommand : IContainerAccess { void Execute(); }
public interface ICommand<TResult> : IContainerAccess { TResult Execute(); }

public abstract class CommandBase : ICommand { /* 便捷访问器 */ }
public abstract class CommandBase<TResult> : ICommand<TResult> { /* 便捷访问器 */ }
```

### ServiceContainer

**文件**: `ServiceContainer.cs`

最小 IoC 容器，按类型键注册/获取实例。

```csharp
public class ServiceContainer
{
    public void Register<T>(T instance);
    public T Get<T>() where T : class;
    public IEnumerable<T> GetInstances<T>();
    public void Clear();
}
```

### EventBus

**文件**: `EventBus.cs`

类型事件总线，Unity-idiomatic 命名。

```csharp
public class EventBus
{
    public static readonly EventBus Global;

    public void Invoke<T>() where T : new();
    public void Invoke<T>(T e);
    public ISubscription AddListener<T>(Action<T> onEvent);
    public void RemoveListener<T>(Action<T> onEvent);
}
```

### ISubscription

**文件**: `ISubscription.cs`

```csharp
public interface ISubscription : IDisposable { }

public interface ISubscriptionCollection
{
    List<ISubscription> Subscriptions { get; }
}

// 扩展
public static void AddTo(this ISubscription subscription, ISubscriptionCollection collection);
public static void DisposeAll(this ISubscriptionCollection self);

// 实现
public class SubscriptionList : ISubscriptionCollection { }
```

### ObservableProperty\<T\>

**文件**: `ObservableProperty.cs`

```csharp
public interface IObservableProperty<T> : IReadOnlyObservableProperty<T>
{
    new T Value { get; set; }
    void SetValueSilently(T value);
}

public interface IReadOnlyObservableProperty<T> : IEvent
{
    T Value { get; }
    ISubscription AddListener(Action<T> onChanged);
    ISubscription AddListenerWithInit(Action<T> onChanged);
    void RemoveListener(Action<T> onChanged);
}

public class ObservableProperty<T> : IObservableProperty<T>
{
    public static Func<T, T, bool> Comparer { get; set; }
    public ObservableProperty(T defaultValue = default);
    public T Value { get; set; }
    public void SetValueSilently(T value);
    public ISubscription AddListener(Action<T> onChanged);
    public ISubscription AddListenerWithInit(Action<T> onChanged);
    public void RemoveListener(Action<T> onChanged);
}
```

### Event / EventCollection

**文件**: `Event.cs`

```csharp
public class Event { /* AddListener, RemoveListener, Invoke */ }
public class Event<T> { /* AddListener, RemoveListener, Trigger */ }
public class Event<T1, T2> { /* AddListener, RemoveListener, Trigger */ }
public class Event<T1, T2, T3> { /* AddListener, RemoveListener, Trigger */ }
public class EventCollection { /* Get, GetOrAdd */ }
```

### IEventCallback\<T\>

**文件**: `IEventCallback.cs`

```csharp
public interface IEventCallback<T> { void OnEvent(T e); }

public static class EventCallbackExtensions
{
    public static ISubscription AddListener<T>(this IEventCallback<T> self) where T : struct;
    public static void RemoveListener<T>(this IEventCallback<T> self) where T : struct;
}
```

### SubscriptionExtensions

**文件**: `SubscriptionExtensions.cs`

Unity 生命周期自动注销扩展。

```csharp
public static class SubscriptionExtensions
{
    public static ISubscription DisposeWhenDestroyed(this ISubscription self, GameObject go);
    public static ISubscription DisposeWhenDestroyed<T>(this ISubscription self, T component) where T : Component;
    public static ISubscription DisposeWhenDisabled(this ISubscription self, GameObject go);
    public static ISubscription DisposeWhenDisabled<T>(this ISubscription self, T component) where T : Component;
    public static ISubscription DisposeWhenSceneUnloaded(this ISubscription self);
}
```
