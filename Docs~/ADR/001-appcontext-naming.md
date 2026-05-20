# ADR-001: AppContext 命名决策

## Status

Accepted

## Context

Aesir Architecture 原始核心类命名为 `AppContainer<T>`，源自 QFramework 的 `Architecture<T>`。在迁移至 Aesir Architecture 包时，需要决定核心类的命名。

## Decision

将 `AppContainer<T>` 重命名为 `AppContext<T>`，`IAppContainer` 重命名为 `IAppContext`。

理由：
1. **"Context" 比 "Container" 更准确** — Container 强调 IoC 容器的实现机制，Context 表达"应用执行上下文"的语义
2. **对齐 .NET 命名** — .NET 中有 `ApplicationContext`、`HttpContext`、`SynchronizationContext` 等 Context 模式
3. **减少与 IoC Container 的混淆** — `AppContext` 包含 IoC 容器但不等于 IoC 容器，还包含 EventBus 和 Command 执行
4. **更短更自然** — `self.Context.GetModel<T>()` 比 `self.Container.GetModel<T>()` 更自然

同时将 `IContainerAccess.Container` 重命名为 `IContainerAccess.Context`，`SetContainer()` 重命名为 `SetContext()`。

## Consequences

- **优点**: 语义更准确，命名更自然
- **优点**: 对齐 .NET BCL 的 Context 模式惯例
- **缺点**: 与 `System.AppContext` 同名，但泛型参数 `AppContext<T>` 使其可区分
- **缺点**: 从 Aesir Architecture 迁移需注意此命名变更
