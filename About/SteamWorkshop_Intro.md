# Better Equipment Reloading - Enhanced Reload System

## Short Description
Tired of manually managing ammo for your smokepop packs and jump packs? This mod adds smart reloading with inventory priority, auto resource finding, and right-click menu options for all vanilla reloadable equipment.

---

## Description

**Better Equipment Reloading** enhances the vanilla reloadable equipment experience in RimWorld. Instead of the basic reload behavior, your pawns will now intelligently manage resources, prioritize inventory items, and provide clear UI feedback.

### Core Features

**Smart Resource Management**
- **Inventory Priority**: Pawns will first use resources from their own inventory
- **Auto Map Search**: When inventory is insufficient, automatically finds reachable resource stacks on the map
- **Multi-stack Hauling**: Supports merging multiple resource stacks for efficient reloading
- **Acurate Reloading**: Pawn will only pick up the minimum amount of resources needed for the current maximum load count. No carry capacity wasted.
- **World Map Gizmo Supported**: In theory, this mod adds reload functionality to all reloadable gear that have a gizmo icon on the world map.

**Compatibility**
- Automatically adapts to all vanilla equipment using `CompApparelReloadable`
- Preserves vanilla reload logic as fallback
- Should be compatible with everything.

### Supported Equipment

- **Smokepop Pack** - Uses Chemfuel
- **Jump Pack** - Uses Chemfuel
- **Tox Pack** - Uses Chemfuel
- All other equipment using `CompProperties_ApparelReloadable`

### How to Use

1. **Right-click Reload**
   - Right-click on the equipment ability button
   - Select "Reload" option
   - Your pawn will automatically find and haul resources

2. **Resource Priority**
   - Inventory items are used first
   - Map resources are fetched when needed
   - Multiple stacks can be merged for reloading

### Reload Process

1. Take resources from inventory (if available)
2. Walk to map resources if inventory insufficient
3. Haul accurate resources (supports multi-stack merging)
4. Wait for reload (~2 seconds with progress bar)
5. Complete reload

---

## Version & Requirements

- **Game Version**: RimWorld 1.6
- **Required Mod**: [Harmony](https://steamcommunity.com/workshop/filedetails/?id=2009463077)
- **Load Order**: Any position after Harmony

---

## Installation & Uninstallation

**Installation:**
- At any time.

**Uninstallation:**
- Not recommended for removal from saves, but should not have severe consequences.

---

## Changelog

**v1.0.0 (2026-02-26)**
- Official release
- Core reload logic implementation
- Inventory priority + map supplement system
- Multi-stack resource hauling support
- Right-click menu reload options
- Gizmo charge display
- Chinese & English localization
- RimWorld 1.6 support

**v1.0.0-pre (2026-02-26)**
- Initial pre-release
- Fixed: Removed invalid `FloatMenuMakerMap.AddHumanlikeOrders` patch for RW 1.6
- Fixed: Double resource consumption issue in `DoReload`
- Fixed: Added missing translation key `BetterEquipmentReloading_ReloadLabel`
- Fixed: Grayed-out Gizmo right-click reload issue

---



# 更好的装备装填 - 增强装填系统

## 简短描述
厌倦了手动管理烟雾弹背包和跳跃背包的燃料？这个MOD为所有原版可装填装备添加了智能装填系统，支持库存优先、自动寻找资源、右键菜单装填等功能。

---

## 描述

**Better Equipment Reloading** 增强了 RimWorld 原版可装填装备的使用体验。小人现在可以智能管理资源、优先使用背包里的材料，并提供清晰的UI反馈。

### 核心功能

**智能资源管理**
- **库存优先**：小人会优先使用自己背包里的资源
- **自动地图搜索**：背包不够时自动寻找地图上可达的资源堆
- **合并搬运**：支持多堆资源合并搬运，一次装填到位
- **精确装填**：小人只会拿取当前最大装填次数所需的最少资源量，不浪费携带能力
- **大地图Gizmo支持**：理论上，这个MOD为所有在世界地图上有Gizmo图标的可装填装备添加了装填功能

**兼容性**
- 自动适配所有使用原版装填系统的装备
- 保留原版装填逻辑作为后备
- 应该与所有模组兼容

### 支持的装备

- **烟雾弹背包** - 使用化合燃料
- **跳跃背包** - 使用化合燃料
- **毒气背包** - 使用化合燃料
- 其他所有使用原版装填系统的装备

### 使用方法

1. **右键菜单装填**
   - 右键点击装备的技能按钮
   - 选择"装填"选项
   - 小人会自动寻找并搬运资源进行装填

2. **资源优先级**
   - 优先使用小人背包里的资源
   - 背包不足时自动从地图搬运
   - 支持多堆资源合并

### 装填流程

1. 从背包取用资源（如果有）
2. 背包不够？走到地图资源处
3. 搬运精确数量的资源（支持多堆合并）
4. 等待装填（约2秒，有进度条）
5. 完成装填

---

## 版本与需求

- **游戏版本**：RimWorld 1.6
- **前置MOD**：[Harmony](https://steamcommunity.com/workshop/filedetails/?id=2009463077)
- **加载顺序**：Harmony之后任意位置

---

## 安装与卸载

**安装：**
- 随时可安装

**卸载：**
- 不推荐从已有存档中移除，但应该不会有严重后果

---

## 更新日志

**v1.0.0 (2026-02-26)**
- 正式版发布
- 核心装填逻辑实现
- 库存优先 + 地图补充系统
- 多堆资源合并搬运
- 右键菜单装填选项
- Gizmo次数显示
- 中英双语支持
- RimWorld 1.6支持

**v1.0.0-pre (2026-02-26)**
- 初始预发布版
- 修复：适配RimWorld 1.6的FloatMenu系统变更
- 修复：资源双重消耗问题
- 修复：补充缺失的翻译键
- 修复：Gizmo变灰后无法右键装填的问题

---
