# Unity MCP (Unity 2019.4 LTS & C# 7.3 兼容版本)

> **基于 [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp) 改造的低版本 Unity (2019.4 LTS) 深度兼容分支。**  
> 实现了完整的语言语法降级（C# 9.0 $\to$ C# 7.3）与引擎底层 API 垫片适配，使经典 Unity 2019.4 项目能够无缝接入现代 AI 助手（Cursor、Claude Code、VS Code、Windsurf 等）。

---

## 📖 项目背景

Unity 官方与社区最新的 MCP (Model Context Protocol) 插件通常依赖 **Unity 2021.3+** 以及 **C# 9.0+** 的现代语法特性（例如模式匹配、记录类型、Switch 表达式、目标类型 `new()`、范围索引切片等），同时重度使用了 2021+ 引入的底层 API（如 `NamedBuildTarget`、`UI Toolkit Runtime`、`ProfilerRecorder` 等）。

然而在很多商业生产环境中，大量老项目、工业仿真项目以及特定平台项目仍长期维护在 **Unity 2019.4 LTS** 版本。本项目通过系统性的语法降级重构、API 垫片（Shim）封装与兜底机制，让 Unity 2019.4 开发者无需升级引擎即可享受现代 AI 编程助手带来的生产力跃迁。

---

## ✨ 核心特性

- 🎮 **原生兼容 Unity 2019.4 LTS**：经实测在 Unity 2019.4.x 环境下 **0 错误、0 警告** 编译通过。
- 🛠️ **严格保持 100% 原始语义**：所有降级均通过语法树比对与严格复审，包括大小写比较器（`StringComparer.OrdinalIgnoreCase`）、空安全检查与布尔逻辑均与原版完全一致。
- ⚡ **无缝支持主流 AI 客户端**：
  - **Cursor**
  - **Claude Code**
  - **VS Code**
  - **Windsurf**
  - **OpenClaw**
- 🔌 **双模式传输**：支持 HTTP (SSE / WebSocket) 与 Stdio 双向通信协议桥接。
- 🧩 **全功能 Unity 工具集**：
  - **场景与层级操作**：GameObject 创建、查找、移动、变换、复制、删除与层级管理。
  - **组件与属性**：动态添加/移除组件、属性读取与修改（反射支持）。
  - **资源与预制体**：Prefab 实例化、解包、保存变体、资源移动与导入。
  - **动画与相机**：Animation Clip、Controller 生成与混合树（BlendTree）配置，Cinemachine 与普通相机控制。
  - **渲染与着色器**：材质创建与属性修改、天空盒、烘焙参数设置、渲染统计信息采集。
  - **脚本与包管理**：C# 脚本创建、安全编辑与编译校验，UPM 包安装/卸载与编译重载状态恢复。

---

## 🔧 降级与兼容性适配细节

### 1. 语法层面（C# 9.0 $\to$ C# 7.3）

| 现代 C# 特性 | 降级处理方案 | 涉及重点模块 |
| :--- | :--- | :--- |
| **`switch` 表达式** | 转换为标准 `switch` 语句或 `if-else` 分支，严格对齐默认分支（`default`） | `Tommy.cs`、`ManageScript.cs`、`BuildSettingsHelper.cs` 等 |
| **模式匹配 (`is not`, 属性模式, 关系模式)** | 展开为标准布尔表达式（如 `!(x is T)`，`c >= '0' && c <= '9'`） | `Tommy.cs`、`UnityTypeConverters.cs` |
| **`using var` 声明** | 显式转换为具有相同生命周期的 `using (...) { ... }` 块 | `SkillSyncService.cs`、`WebSocketTransportClient.cs` |
| **目标类型 `new()`** | 补齐显式类型声明，并**严格保留所有比较器参数**（如 `StringComparer.OrdinalIgnoreCase`） | `CameraCreate.cs`、`ManageUI.cs`、`McpToolsSection.cs` 等 |
| **范围切片 (`[..^x]`, `[7..]`)** | 等价转换为 `.Substring(...)`、`.Skip(...)` 或安全索引截取 | `StdioBridgeHost.cs`、`AssetPathUtility.cs` |
| **空合并赋值 (`??=`)** | 等价转换为 `if (x == null) x = ...;` | `Tommy.cs`、`TransportManager.cs` |

### 2. 引擎与底层 API 层面（Unity 2021+ $\to$ Unity 2019.4）

- **UIElements 垫片 (`DropdownFieldShim`)**：
  - Unity 2019.4 缺少标准 `DropdownField`。本项目内置了完整的 `DropdownFieldShim` 垫片，并在 Editor 窗口中将 `VisualTreeAsset.Instantiate()` 适配为 2019.4 原生支持的 `CloneTree()`。
- **构建目标垫片 (`NamedBuildTargetShim`)**：
  - Unity 2021.2 之前使用 `BuildTargetGroup`，之后引入了 `NamedBuildTarget`。垫片提供了透明隐式类型转换，确保跨版本构建逻辑平滑运行。
- **包管理与 Domain Reload 恢复机制**：
  - 在 Unity 2019.4 中无法直接使用 `PackageInfo.GetAllRegisteredPackages()`。在 `PackageJobManager.cs` 中实现了 `GetAllRegisteredPackagesCompat()`，通过读取 `projectLock.json` 以及本地 `Packages/` 目录进行复合扫描，彻底解决了编译重载后 Package 任务丢失的问题。
- **性能分析与统计平替**：
  - 针对 2019.4 缺失的 `ProfilerRecorder`（Unity 2020.2+ 特性），在 `RenderingStatsOps.cs` 中平替为经典的 `UnityStats`，保留了 Draw Calls、Batches、Triangles、Vertices 的实时获取能力。
- **Newtonsoft.Json 9.0 泛型转换器**：
  - Unity 2019.4 内置的 Json.NET 版本较低（不支持 `JsonConverter<T>`），在 `UnityTypeConverters.cs` 中实现泛型基类垫片，确保基础数学类型（`Vector2/3/4`、`Quaternion` 等）正常序列化。

---

## 📦 如何在其他项目中导入此包

本项目支持通过 **Unity Package Manager (UPM)** 直接通过 Git URL 导入，或者作为本地嵌入式包引入。

### 方式一：Unity Package Manager (UPM) 界面导入（推荐）

> [!IMPORTANT]
> **切勿直接输入裸仓库链接！** 由于本仓库包含完整的示例工程，`package.json` 位于 `/Assets/UnityMCP` 子目录中。如果直接输入裸 URL，Unity 会报错：`No package manifest was found`。必须在链接末尾附带 `?path=/Assets/UnityMCP` 参数。

1. 打开目标 Unity 项目，点击顶部菜单栏：**Window** $\to$ **Package Manager**。
2. 点击左上角的 **`+`** 加号，选择 **"Add package from git URL..."**。
3. 输入以下完整地址并点击 **Add**：
   ```text
   https://github.com/JiangJianGame/UnityMCP.git?path=/Assets/UnityMCP
   ```
   *(如果需要锁定特定分支，可加上 `#main`：`https://github.com/JiangJianGame/UnityMCP.git?path=/Assets/UnityMCP#main`)*

---

### 方式二：直接修改 `Packages/manifest.json`

在目标项目的 `Packages/manifest.json` 文件中，向 `"dependencies"` 字典中添加以下键值对：

```json
{
  "dependencies": {
    "com.coplaydev.unity-mcp": "https://github.com/JiangJianGame/UnityMCP.git?path=/Assets/UnityMCP",
    ...
  }
}
```
保存文件后切回 Unity，编辑器将自动拉取并导入。

---

### 方式三：本地离线导入（Local Package / Assets）

- **本地 Package 引入**：Clone 或下载本仓库到本地，在 Package Manager 中点击 **`+`** $\to$ **"Add package from disk..."**，选择仓库内的 `Assets/UnityMCP/package.json` 文件。
- **直接放入 Assets**：直接将本仓库的 `Assets/UnityMCP` 文件夹完整拷贝到目标项目的 `Assets/` 目录下即可。

---

## 🚀 快速上手

### 1. 环境准备
- **Unity**：2019.4.x LTS (推荐 2019.4.36f1 或更高)
- **.NET 运行环境**：项目 Scripting Runtime 设为 .NET 4.x Equivalent / .NET Standard 2.0
- **Python 环境**：Python 3.10+，建议安装 [`uv`](https://github.com/astral-sh/uv) (`pip install uv`)

### 2. 启动与配置
1. 在 Unity 编辑器顶部菜单中打开：
   ```text
   Window > MCP for Unity
   ```
   *(或使用快捷键：Windows/Linux 为 `Ctrl+Shift+M`，macOS 为 `Cmd+Shift+M`)*
2. 窗口弹出后，点击 **Auto-Setup** 按钮：
   - 插件将自动检测环境依赖（Python、uv、Claude CLI 等）。
   - 点击 **Start Bridge** 启动 Unity 内部通信桥。
3. 选择您使用的 AI 客户端并一键配置：
   - **Cursor**：点击 *Auto Configure* 自动注入配置文件，或点击 *Manual Setup* 复制 JSON 到 Cursor 设置。
   - **Claude Code**：点击 *Register with Claude Code*。
   - **VS Code / Windsurf**：点击 *Auto Configure* 或复制配置文件。

### 3. 开始使用
配置完成后，在 AI 对话框中即可直接通过自然语言调度 Unity 2019.4 编辑器！例如：
- *"在场景中创建一个红色立方体，坐标放在 (0, 1, 0)"*
- *"帮我检查一下 Scripts 目录下的 PlayerController.cs 是否有语法错误"*
- *"获取当前场景的渲染性能统计信息（DrawCall、面数、顶点数）"*
- *"创建一个平滑跟随主相机的 Cinemachine 虚拟相机"*

---

## 📁 核心目录结构

```text
Assets/UnityMCP/
├── Editor/
│   ├── Clients/              # 各 AI 客户端配置管理器 (Cursor, Claude, VSCode 等)
│   ├── Helpers/              # 工具函数与兼容垫片 (DropdownFieldShim, NamedBuildTargetShim 等)
│   ├── Resources/            # 资源与查询接口
│   ├── Security/             # 凭据与安全存储
│   ├── Services/             # 传输服务、网络管理与包恢复 (PackageJobManager)
│   ├── Setup/                # 安装与初始化引导
│   ├── Tools/                # MCP 工具定义 (GameObjects, Scene, Build, Graphics, UI 等)
│   └── Windows/              # UIElements 编辑器面板实现
├── Runtime/
│   ├── External/Tommy/       # 纯 C# 7.3 兼容的 TOML 解析器
│   └── Serialization/        # Json 转换器与 Unity 数据结构序列化
└── package.json              # 包描述 (指定 unity: "2019.4")
```

---

## ⚠️ 已知限制说明

1. **Runtime UI Toolkit**：`UIDocument` 与 `PanelSettings` 属于 Unity 2021.1+ 新增特性。在 2019.4 中，`ManageUI` 对运行时 UI Toolkit 的操作会自动返回安全阻断提示（建议在 2019.4 下使用 UGUI）。
2. **Roslyn 代码深度语义分析**：严格的 Roslyn 静态分析依赖 Roslyn 分析器程序集，2019.4 默认采用内置的语法校验（Standard / Basic 级别）。

---

## 📄 开源许可

本项目遵循原项目的开源协议（[MIT License](https://github.com/CoplayDev/unity-mcp/blob/main/LICENSE)）。  
感谢 [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp) 原作者及开源社区贡献者的优秀工作。

