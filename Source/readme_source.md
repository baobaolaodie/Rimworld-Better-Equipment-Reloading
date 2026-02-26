# Better Equipment Reloading - 源码说明

本文档详细描述 Better Equipment Reloading Mod 的源码结构与技术实现。

## 项目结构

```
Source/
├── BetterEquipmentReloading.csproj    # 项目配置文件
├── HarmonyInit.cs                     # Harmony 初始化入口
├── IReloadableComp.cs                 # 装填组件接口定义
├── CompProperties_BetterReloadable.cs # 装填组件属性
├── CompBetterReloadable.cs            # 核心装填组件
├── ResourceFinder.cs                  # 资源检索工具类
├── JobDriver_BetterReload.cs          # 装填 Job 驱动
├── HarmonyPatches_ReloadableInject.cs # 原版装备注入补丁
├── HarmonyPatches_RightClickMenu.cs   # 右键菜单补丁
└── HarmonyPatches_VerbGizmo.cs        # Gizmo 显示补丁
```

## 核心组件说明

### IReloadableComp.cs
定义装填组件接口，包含：
- `RemainingCharges` / `MaxCharges` - 当前/最大使用次数
- `AmmoDef` - 装填资源类型
- `AmmoCountPerCharge` / `AmmoCountToRefill` - 每次装填消耗
- `CanUse()` / `Use()` - 使用次数管理
- `FindAllResources()` / `CountInventoryResources()` / `FindReachableMapResources()` - 资源检索
- `StartReloadJob()` - 启动装填 Job

### CompBetterReloadable.cs
核心装填组件，实现 `IReloadableComp` 接口：
- 管理使用次数（`remainingCharges`）
- 实现资源检索逻辑（库存优先 + 地图补充）
- 启动装填 Job，支持多堆资源合并搬运
- 存档支持（`PostExposeData`）

### ResourceFinder.cs
静态工具类，提供资源检索方法：
- `CountInventoryResources()` - 统计库存资源数量
- `FindLargestInventoryResource()` - 找到库存中最大的资源堆
- `FindReachableMapResources()` - 找到地图上所有可达的资源堆
- `FindAllResources()` - 找到所有可用资源（库存 + 地图）
- `CountAvailableResources()` - 统计所有可用资源总量

### JobDriver_BetterReload.cs
装填 Job 驱动，处理完整的装填流程：
1. 从库存取用资源（如果有）
2. 如果库存不足，走到地图资源处
3. 搬运资源（支持多堆合并）
4. 等待装填时间（120 ticks，显示进度条）
5. 执行装填（调用 `ReloadFrom`，由原版组件处理资源消耗和次数恢复）
6. 丢弃剩余资源

## Harmony 补丁说明

### HarmonyPatches_ReloadableInject.cs
为原版 `CompApparelReloadable` 注入增强功能：
- `VanillaReloadableWrapper` - 包装原版组件，统一接口
- `NeedsReload_Postfix` - 修改 `NeedsReload` 判断逻辑
- `StartEnhancedReloadJob()` - 启动增强装填 Job

### HarmonyPatches_RightClickMenu.cs
包含两个Harmony补丁：

**Command_GizmoOnGUIInt_Patch** (Prefix):
- 拦截禁用状态Gizmo的右键点击事件
- 强制打开浮动菜单，确保使用次数耗尽后仍可右键装填

**Gizmo_RightClickFloatMenuOptions_Patch** (Postfix):
- 为 `Command_VerbOwner` 添加右键菜单装填选项
- 检测装备的 `CompApparelReloadable` 组件
- 如果需要装填，添加"装填"菜单选项

### HarmonyPatches_VerbGizmo.cs
修改 `Command_VerbOwner.TopRightLabel` 显示剩余次数：
- Prefix 补丁拦截属性获取
- 如果装备有装填组件，显示 "当前次数/最大次数"

## 技术要点

### 库存优先策略
资源检索时优先检查 Pawn 库存，库存不足时才从地图搬运。这通过 `FindAllResources()` 方法实现，该方法返回的列表中库存资源排在前面。

### 合并搬运支持
通过 `job.targetQueueB` 实现多堆资源的队列收集。在 `JobDriver_BetterReload` 中，使用 `Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue` 支持从多堆资源中收集。

### 与原版兼容
通过 `VanillaReloadableWrapper` 类包装原版的 `CompApparelReloadable`，使其符合 `IReloadableComp` 接口。这样可以在不修改原版代码的情况下，为原版装备添加增强功能。

### RimWorld 1.6 兼容性
- RimWorld 1.6 移除了 `FloatMenuMakerMap.AddHumanlikeOrders` 方法，改用 `FloatMenuOptionProvider` 系统
- 本Mod通过直接Patch `Gizmo.RightClickFloatMenuOptions` 属性实现右键菜单功能
- 当装备使用次数耗尽时，Gizmo会进入禁用状态，通过 `Command_GizmoOnGUIInt_Patch` 前缀补丁拦截右键事件，强制打开浮动菜单

## 翻译键值

| 键值 | 中文 | 英文 |
|------|------|------|
| `BetterEquipmentReloading_ReloadLabel` | 装填 | Reload |
| `BetterEquipmentReloading_NoResource` | 没有足够的 {0} 进行装填。 | Not enough {0} to reload. |
| `BetterEquipmentReloading_AlreadyFull` | 装备已满，无需装填。 | Equipment is already full. |
| `BetterEquipmentReloading_NoAmmoDef` | 未配置装填材料。 | No ammo type configured. |
| `BetterEquipmentReloading_InvalidCost` | 装填配置无效。 | Invalid reload configuration. |
