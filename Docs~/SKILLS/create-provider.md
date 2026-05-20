# Skill: 创建新的 Provider

> 当需要抽象外部依赖（存储、网络、SDK 等）时，按此指南操作。

## 前置条件

- 确认功能是纯基础设施（无游戏业务概念）
- 确认接口参数不包含 Player、Item、Score 等游戏类型
- 确认换个项目可原封不动复用

## 步骤

### 1. 定义接口

```csharp
using RunLab.AesirArchitecture;

public interface IStorageProvider : IProvider
{
    void Save(string key, string value);
    string Load(string key, string defaultValue = "");
    void Delete(string key);
    bool HasKey(string key);
}
```

- 接口命名：`I{Name}Provider`
- 方法参数只用基础类型（string、int 等），不用游戏业务类型

### 2. 实现类

```csharp
using UnityEngine;

public class PlayerPrefsStorageProvider : IStorageProvider
{
    public void Save(string key, string value) => PlayerPrefs.SetString(key, value);
    public string Load(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);
    public void Delete(string key) => PlayerPrefs.DeleteKey(key);
    public bool HasKey(string key) => PlayerPrefs.HasKey(key);
}
```

- 类命名：`{Technology}{Name}Provider`（如 `PlayerPrefsStorageProvider`）
- **不继承** `ServiceBase` 或 `ModelBase`
- **无** `OnInit()` / `OnDeinit()` 生命周期
- **无** `GetModel<T>()` / `GetService<T>()` 访问权
- 构造函数即初始化

### 3. 注册到 AppContext

```csharp
public class GameApp : AppContext<GameApp>
{
    protected override void Configure()
    {
        RegisterProvider<IStorageProvider>(new PlayerPrefsStorageProvider());
    }
}
```

## 约束

| 规则 | 说明 |
|------|------|
| 零框架依赖 | 不实现 IContainerAccess |
| 无生命周期 | 无 OnInit / OnDeinit |
| 接口无业务概念 | 参数只用 string/int 等基础类型 |
| 可跨项目复用 | 换游戏项目不需修改 |

## 常见 Provider 清单

| Provider | 职责 |
|----------|------|
| `IStorageProvider` | 本地键值存储 |
| `INetworkProvider` | HTTP 请求 |
| `IAudioProvider` | 音频播放 |
| `ITimeProvider` | 系统时间 |
| `IRandomProvider` | 随机数 |
| `ILogProvider` | 日志写入 |
| `IAssetProvider` | 资源加载 |

## 验证

1. 编译无报错
2. `GameApp.Instance.GetProvider<IStorageProvider>()` 返回非 null
3. 可独立实例化和测试（无需 AppContext）
