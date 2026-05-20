# Aesir Architecture — Agent Context

## Project Identity

- **Package**: `cn.runlab.aesir-architecture` | **Version**: 0.3.0
- **Engine**: Tuanjie (Unity 2022.3 fork)
- **Language**: C# | .NET Standard 2.1
- **License**: MIT

## Commands

- **Test**: Unity Test Runner → Play Mode (no CLI)
- **Build**: Tuanjie Editor → Build (no CLI build)

## Project Structure

```
Aesir Architecture/
├── Runtime/                  # 运行时 (RunLab.AesirArchitecture)
├── Editor/                   # 编辑器扩展
├── Tests/
│   ├── Runtime/              # 运行时测试
│   └── Editor/               # 编辑器测试
├── Samples/                  # 示例
├── Documentation~/           # Unity 用户文档
└── Docs~/                    # AI agent deep context (Cold Memory)
```

## Key Rules

- **严禁**对 `UnityEngine.Object` 派生类使用 `?.` 或 `??`
- 核心类命名为 `AppContext<T>`，不是 `AppContainer<T>` 或 `Architecture<T>`
- `IContainerAccess.Context` 返回 `IAppContext`（不是 `.Container`）
- Model 只能访问 Provider + Events，不能访问 Service
- Service 可访问 Model + Provider + Events
- Provider 零框架依赖，无生命周期
- 事件命名：`Invoke` / `AddListener` / `RemoveListener`（对齐 UnityEvent）
- Command 执行：`ExecuteCommand`（不是 `SendCommand`）
- `ObservableProperty<T>` 的值变更自动通知，无需手动 RefreshUI

## Deep Context

For architecture, conventions, and task guides:

- Architecture & C4 model → `Docs~/ARCHITECTURE.md`
- Code style & naming → `Docs~/CONVENTIONS.md`
- Module API docs → `Docs~/MODULES.md`
- Design decisions → `Docs~/ADR/`
- Task-specific guides → `Docs~/SKILLS/`
