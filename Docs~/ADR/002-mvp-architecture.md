# ADR-002: 选择 MVP 作为核心架构

## Status

Accepted

## Context

需要选择一个架构模式作为 Aesir Architecture 的核心框架。候选方案：
1. **MVC** — Model-View-Controller
2. **MVP** — Model-View-Presenter
3. **MVVM** — Model-View-ViewModel
4. **Clean Architecture** — 严格分层 + CQS

## Decision

选择 MVP（Model-View-Presenter）作为核心架构模式。

理由：
1. **Unity 官方推荐** — Unity Learn 的 Design Patterns 教程和 e-book *Level up your code with design patterns and SOLID* 明确推荐 MVP
2. **Unity UI 天然是 View** — UI Toolkit / UGUI 天然充当 View 层，Presenter 作为 Model 与 View 之间的中介
3. **比 MVC 更适合 Unity** — Unity 的 MonoBehaviour 不能被动刷新（Controller → View），需要 Presenter 主动推数据
4. **比 MVVM 更简单** — 不需要双向绑定框架，ObservableProperty 提供单向数据驱动即可
5. **比 Clean Architecture 更轻量** — 不需要 Query 层、Rule 接口等额外概念

映射关系：
- QFramework 的 `IController` → `IPresenter`（MVP 标准术语）
- QFramework 的 `ISystem` → `IService`（.NET 标准命名）
- 移除 `IQuery` 层（MVP 中 Presenter 直接读取 Model）

## Consequences

- **优点**: 与 Unity 社区通用语言一致，学习成本低
- **优点**: Presenter 主动推数据，适合 Unity 的帧驱动模式
- **缺点**: Presenter 可能变胖，需通过 Command 保持精简
- **缺点**: 非纯 MVP（Service 层不在传统 MVP 定义中），但更实用
