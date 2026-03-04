# Better Equipment Reloading - 源码文档

本文档详细描述 Better Equipment Reloading Mod 的源码结构与技术实现。

---

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

---

## 核心组件

### IReloadableComp.cs

**接口定义**:
```csharp
public interface IReloadableComp
{
    int RemainingCharges { get; }
    int MaxCharges { get; }
    ThingDef AmmoDef { get; }
    int AmmoCountPerCharge { get; }
    
    bool CanUse();
    void Use();
    
    List<Thing> FindAllResources();
    int CountInventoryResources();
    List<Thing> FindReachableMapResources();
    
    void StartReloadJob();
}
```

### CompBetterReloadable.cs

**核心功能**:
- 管理使用次数 (`remainingCharges`)
- 实现资源检索逻辑（库存优先 + 地图补充）
- 启动装填 Job，支持多堆资源合并搬运
- 存档支持（`PostExposeData`）

**资源检索策略**:
```csharp
// 1. 优先检查库存
int inventoryCount = CountInventoryResources();

// 2. 库存不足时检索地图
if (inventoryCount < needed)
{
    var mapResources = FindReachableMapResources();
    // 按数量排序，优先使用大堆
}
```

### ResourceFinder.cs

**静态工具方法**:
| 方法 | 功能 |
|------|------|
| `CountInventoryResources()` | 统计库存资源数量 |
| `FindLargestInventoryResource()` | 找到库存中最大的资源堆 |
| `FindReachableMapResources()` | 找到地图上所有可达的资源堆 |
| `FindAllResources()` | 找到所有可用资源（库存 + 地图） |
| `CountAvailableResources()` | 统计所有可用资源总量 |

### JobDriver_BetterReload.cs

**装填流程**:
1. **从库存取用**（如果有）
2. **走到地图资源处**（如果库存不足）
3. **搬运资源**（支持多堆合并）
4. **等待装填时间**（120 ticks，显示进度条）
5. **执行装填**（调用 `ReloadFrom`）
6. **丢弃剩余资源**

**关键代码**:
```csharp
yield return Toils_Haul.StartCarryThing(
    TargetIndex.B, 
    subtractNumTakenFromJobCount: true,
    failIfStackCountLessThanJobCount: false
);
```

---

## Harmony 补丁

### HarmonyPatches_ReloadableInject.cs

**原版装备注入**:

**VanillaReloadableWrapper**
- 包装原版的 `CompApparelReloadable`
- 统一实现 `IReloadableComp` 接口
- 使原版装备可使用增强装填逻辑

**NeedsReload_Postfix**
- 修改 `NeedsReload` 判断逻辑
- 确保增强装填系统正确识别装填需求

### HarmonyPatches_RightClickMenu.cs

**Command_GizmoOnGUIInt_Patch (Prefix)**:
- 拦截禁用状态 Gizmo 的右键点击事件
- 强制打开浮动菜单，确保使用次数耗尽后仍可右键装填

**Gizmo_RightClickFloatMenuOptions_Patch (Postfix)**:
- 为 `Command_VerbOwner` 添加右键菜单装填选项
- 检测装备的 `CompApparelReloadable` 组件
- 如果需要装填，添加"装填"菜单选项

### HarmonyPatches_VerbGizmo.cs

**TopRightLabel_Patch (Prefix)**:
- 拦截 `Command_VerbOwner.TopRightLabel` 属性获取
- 如果装备有装填组件，显示 "当前次数/最大次数"

---

## 技术要点

### 库存优先策略

资源检索时优先检查 Pawn 库存，库存不足时才从地图搬运：

```csharp
public List<Thing> FindAllResources()
{
    var resources = new List<Thing>();
    
    // 1. 先加库存资源
    var inventoryResource = FindLargestInventoryResource();
    if (inventoryResource != null)
        resources.Add(inventoryResource);
    
    // 2. 再加地图资源
    resources.AddRange(FindReachableMapResources());
    
    return resources;
}
```

### 合并搬运支持

通过 `job.targetQueueB` 实现多堆资源的队列收集：

```csharp
// 设置多目标队列
job.targetQueueB = resources.Select(r => new LocalTargetInfo(r)).ToList();

// 使用 JumpIfAlsoCollectingNextTargetInQueue 支持合并搬运
toil = Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(
    TargetIndex.B, 
    toil
);
```

### 与原版兼容

**VanillaReloadableWrapper** 实现适配器模式：

```csharp
public class VanillaReloadableWrapper : IReloadableComp
{
    private readonly CompApparelReloadable _vanilla;
    
    public VanillaReloadableWrapper(CompApparelReloadable vanilla)
    {
        _vanilla = vanilla;
    }
    
    public int RemainingCharges => _vanilla.RemainingCharges;
    public int MaxCharges => _vanilla.MaxCharges;
    // ... 其他属性和方法转发到原版组件
}
```

### RimWorld 1.6 兼容性

- RimWorld 1.6 移除了 `FloatMenuMakerMap.AddHumanlikeOrders` 方法
- 改用 `FloatMenuOptionProvider` 系统
- 本 Mod 通过直接 Patch `Gizmo.RightClickFloatMenuOptions` 属性实现右键菜单功能

---

## 翻译键值

| 键值 | 中文 | 英文 |
|------|------|------|
| `BetterEquipmentReloading_ReloadLabel` | 装填 | Reload |
| `BetterEquipmentReloading_NoResource` | 没有足够的 {0} 进行装填。 | Not enough {0} to reload. |
| `BetterEquipmentReloading_AlreadyFull` | 装备已满，无需装填。 | Equipment is already full. |
| `BetterEquipmentReloading_NoAmmoDef` | 未配置装填材料。 | No ammo type configured. |
| `BetterEquipmentReloading_InvalidCost` | 装填配置无效。 | Invalid reload configuration. |

---

## 扩展指南

### 添加对新装备的支持

本 Mod 自动适配所有使用 `CompApparelReloadable` 的装备，无需额外配置。

### 自定义装填逻辑

继承 `CompBetterReloadable` 并重写相关方法：

```csharp
public class MyCustomReloadable : CompBetterReloadable
{
    public override void StartReloadJob()
    {
        // 自定义装填逻辑
        base.StartReloadJob();
    }
}
```

---

*文档版本: 2026-02-26*
