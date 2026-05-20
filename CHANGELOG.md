# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.0] - 2026-05-19

### Changed

- **Breaking** 包重命名：`cn.runlab.aesir-modules` → `cn.runlab.aesir-architecture`
- **Breaking** 命名空间展平：`RunLab.AesirModules.Architecture` → `RunLab.AesirArchitecture`
- **Breaking** 测试命名空间：`RunLab.AesirModules.Tests` → `RunLab.AesirArchitecture.Tests`
- **Breaking** 目录展平：`Runtime/Architecture/*.cs` → `Runtime/*.cs`（消除冗余子目录）
- 程序集重命名：`RunLab.AesirModules` → `RunLab.AesirArchitecture`（含 Editor/Tests 变体）
- 显示名：`Aesir Modules` → `Aesir Architecture`

### Removed

- `Runtime/Architecture/` 子目录（文件已上移至 `Runtime/`）

## [0.2.0] - 2026-05-14

### Added

- Architecture 模块：MVP 架构框架（AppContext, IPresenter, IModel, IService, IProvider, ICommand）
- EventBus 类型事件总线（AddListener / RemoveListener / Invoke）
- ObservableProperty\<T\> 响应式属性
- ISubscription 订阅管理 + Unity 生命周期自动注销
- ServiceContainer 最小 IoC 容器
- AppContextTests 运行时测试
- AI Agent 文档体系（AGENTS.md + CODELY.md + Docs~/）

## [0.1.0] - 2026-05-14

### Added

- Initial release.
