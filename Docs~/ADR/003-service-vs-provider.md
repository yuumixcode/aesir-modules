# ADR-003: Service 与 Provider 分层

## Status

Accepted

## Context

Aesir Architecture 的业务逻辑层和基础设施层都使用 QFramework 的 `ISystem` / `IUtility`。需要在 Aesir Architecture 中明确两者的边界和命名。

## Decision

将业务逻辑层和基础设施层分为 `IService` 和 `IProvider`。

**IService（业务逻辑）**：
- 有框架生命周期（OnInit / OnDeinit）
- 可访问 Model + Service + Provider + Events
- 接口参数含游戏业务概念（Player、Item、Score 等）
- 不可跨项目复用

**IProvider（基础设施）**：
- 零框架依赖（不实现 IContainerAccess）
- 无生命周期（纯接口标记）
- 接口参数无游戏业务概念
- 可跨项目复用

判断标准：**接口参数中是否出现游戏业务概念**。如果出现 `Player`、`Item`、`HP`、`Score` 等概念，即使只有一个，它就是 Service。

## Consequences

- **优点**: 依赖方向清晰：Service → Model → Provider，无循环依赖
- **优点**: Provider 可独立测试和替换，无需框架上下文
- **优点**: 职责边界明确，减少"这个该放哪里"的困惑
- **缺点**: 边界场景需要判断（如伤害公式是业务规则即使无状态），但决策流程图提供指引
