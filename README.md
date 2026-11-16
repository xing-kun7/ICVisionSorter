# 基于 WPF 与 Halcon 的 IC 芯片引脚视觉分拣系统上位机

> **定位**：面向 LM386 等集成电路（IC）芯片封测产线的自动化高速视觉质检分拣上位机。集成 Halcon 机器视觉算法库与雷赛运动控制卡，完成流水线送料、高速拍照、引脚缺陷（缺脚、连锡短路、弯曲歪斜）亚像素检测与气动分拣闭环，并通过 EF Core + SQL Server 实现全流程生产数据追踪与良率看板。

---

## 🌟 核心工业亮点（面试与技术重点）

### 1. 多线程解耦与内存泄漏治理
- **WPF + MVVM 异步架构**：基于 `CommunityToolkit.Mvvm` 设计，采用多线程异步流水线与 Dispatcher 调度，将图像采集、算法检测、运动控制、数据落库完全剥离至后台非 UI 线程，彻底避免界面卡顿。
- **Halcon 非托管内存严管**：工业级长周期稳定运行设计，严格管控 Halcon 非托管对象生命周期，确保 `HObject`、`HTuple` 均在 `finally` 块中显式 `Dispose` 释放，彻底杜绝连续高频运行下的内存泄漏与句柄耗尽问题。

### 2. 缺陷检测流水线与光学防呆
- **高精度引脚检测**：基于 Halcon 开发引脚检测算法，结合形态学处理与动态 ROI 定位逻辑，精准检测**引脚缺失、连锡短路、引脚弯曲歪斜**，产线误检率控制在 **2%** 以内，综合判定准确率稳定 **≥ 98%**。
- **光学前置防呆**：前置增加图像全局灰度均值智能校验，在现场环境发生光源衰减（均值 < 35）或外界杂光/过度曝光（均值 > 220）时立即报警停机，杜绝异常画面导致的误判流出。

### 3. 闭环分拣运控与 Fail-Safe 联锁
- **高节拍闭环分选**：对接雷赛运动控制卡，实现皮带输送、传感器到位定点触发采集、气动推料机构 OK/NG 分流全流程闭环控制，单芯片检测分拣节拍 **≤ 2s**。
- **工业级 Fail-Safe 联锁保护**：
  - 队列水位超过 80% 触发机台降速预警；
  - 遇队列为空（漏检断流）或检测超时异常时，系统遵循 Fail-Safe 强安全原则，**强制打入 NG 废料盒**，严禁静默放行未检品。

### 4. 数据持久化与质量报表看板
- **全流程追溯**：采用 Entity Framework Core + SQL Server 异步落盘，记录每颗芯片的批次号、检测时间、判定状态、缺陷分类成因及操作员编号。
- **动态图表看板**：集成 LiveCharts，实时动态刷新良率走势折线图与批次合格率统计，支持历史追溯与 CSV/Excel 导出。

---

## 🛠️ 技术栈与架构

| 模块 | 关键技术 |
| :--- | :--- |
| **界面与架构** | C# / .NET Framework 4.8 / WPF / CommunityToolkit.Mvvm / MahApps.Metro |
| **机器视觉** | MVTec Halcon 19+ (动态模板匹配, 形态学滤波, 亚像素轮廓提取) |
| **运动控制** | 雷赛 (Leadshine) 运动控制卡 SDK / 仿真驱动模块 (SimulationServices) |
| **数据持久化** | Entity Framework 6 / EF Core + SQL Server (含初始化脚本 `init_db.sql`) |
| **图表展示** | LiveCharts / WPF Toolkit |

---

## 🚀 快速启动与仿真运行

本项目内置**仿真驱动服务 (Simulation Mode)**，即使现场未连接物理雷赛运控卡与工业相机，也可通过预置仿真图像集进行全流程逻辑与界面演练。

### 1. 数据库准备
在 SQL Server 中执行项目根目录下的 `init_db.sql` 创建 `VisualSortingDB` 数据库及数据表：
```sql
-- 执行根目录下的 init_db.sql 完成表结构与初始账号初始化
```

### 2. 仿真模式配置
打开 `VisualSorting/App.config`，确认以下仿真配置项：
```xml
<appSettings>
  <!-- 开启仿真模式 -->
  <add key="UseSimulation" value="true" />
  <!-- 仿真图像输入路径 -->
  <add key="SimImageDir" value=".\Image1\new_dataset" />
</appSettings>
```

### 3. 编译运行
1. 用 **Visual Studio 2022** 打开 `VisualSorting.sln`；
2. 还原 NuGet 包并选择 `x64` 或 `Any CPU`；
3. 按 **F5** 启动上位机；
4. 默认账号登录：管理员 `admin` / 密码 `admin`。

---

## 📄 技术文档

- 详细系统架构设计与算法原理请参阅：[IC芯片视觉分拣系统 技术文档.pdf](https://github.com/user-attachments/files/27302839/IC.pdf)
