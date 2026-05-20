# ADR-004: ObservableProperty 数据驱动

## Status

Accepted

## Context

需要在 MVP 架构中实现 UI 自动更新。可选方案：
1. **手动刷新** — Presenter 在逻辑执行后调用 `RefreshUI()`
2. **数据绑定框架** — MVVM 双向绑定
3. **ObservableProperty 单向数据驱动** — 值变更自动通知订阅者

## Decision

采用 `ObservableProperty<T>` 单向数据驱动模式。

理由：
1. **消除手动刷新** — 修改 `Value` 即自动通知所有订阅者，无需 `RefreshUI()`
2. **Single Source of Truth** — 数据只在 Model 中存在一份，UI 是只读投影
3. **比双向绑定简单** — 不需要绑定框架，只需 `AddListener` 建立单向数据流
4. **与 EventBus 互补** — 持续状态用 `ObservableProperty`，瞬时事件用 `EventBus`

关键设计：
- `ObservableProperty<T>` 的 `Value` setter 自动触发通知（值确实变化时）
- `SetValueSilently()` 可静默设值不触发通知
- `AddListenerWithInit()` 订阅时立即获取当前值
- Unity 内置类型的比较器通过 `[RuntimeInitializeOnLoadMethod]` 自动注册

## Consequences

- **优点**: UI 更新零遗漏——数据变更即通知，不会忘记调用 RefreshUI
- **优点**: 多订阅者自然支持——一个属性变更可同时通知血条、伤害数字、死亡检测
- **缺点**: 值比较需要自定义 Comparer（引用类型默认使用 Equals）
- **缺点**: 大量属性可能导致通知风暴，但实际项目中很少成为问题
