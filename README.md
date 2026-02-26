# Better Equipment Reloading

**版本**: v1.0.0  
**作者**: SkylarTech  
**支持游戏版本**: RimWorld 1.6

## 功能说明

Better Equipment Reloading 是一个独立的 RimWorld Mod，为原版消耗型装备提供增强的装填体验。

### 核心功能

1. **增强装填逻辑**
   - 库存优先：优先从 Pawn 身上取用资源进行装填
   - 地图补充：库存不足时自动检索地图上可达的资源堆
   - 合并搬运：支持多堆资源的合并搬运装填

2. **UI 增强**
   - 在装备 Gizmo 右上角显示剩余次数（当前/最大）
   - 右键菜单装填选项，显示当前弹药状态

3. **兼容性**
   - 自动适配所有使用 `CompApparelReloadable` 的原版装备
   - 不影响 SkylarTech 模组自身的装填逻辑
   - 保留原版装填逻辑作为兼容兜底

### 支持的原版装备

- 烟雾弹背包 (Smokepop Pack) - 使用化合燃料装填
- 跳跃背包 (Jump Pack) - 使用化合燃料装填
- 毒气背包 (Tox Pack) - 使用化合燃料装填
- 其他使用 `CompProperties_ApparelReloadable` 的装备

## 使用指南

### 装填方式

1. **右键菜单装填**
   - 右键点击装备的技能按钮
   - 选择"装填"选项
   - Pawn 会自动寻找并搬运资源进行装填

2. **自动资源检索**
   - 优先使用 Pawn 身上携带的资源
   - 库存不足时自动寻找地图上可达的资源堆
   - 支持多堆资源合并搬运

### 装填流程

1. Pawn 从库存取用资源（如果有）
2. 如果库存不足，走到地图资源处
3. 搬运资源（支持多堆合并）
4. 等待装填时间（约2秒，显示进度条）
5. 完成装填，丢弃剩余资源

## 兼容性说明

### 依赖
- **Harmony** (必需) - 用于代码注入

### 兼容的 Mod
- SkylarTech - 完全兼容，不影响其自身装填逻辑
- 其他修改 `CompApparelReloadable` 的 Mod - 可能存在冲突

### 不兼容的 Mod
- 暂无已知不兼容的 Mod

## 更新日志

### v1.0.0 (2026-02-26)
- **正式版发布**
- 实现核心装填逻辑移植
- 支持库存优先 + 地图补充的装填方式
- 支持合并搬运多堆资源
- 添加右键菜单装填选项（通过 Gizmo 右键菜单）
- 添加 Gizmo 剩余次数显示
- 添加中英文翻译支持
- 支持 RimWorld 1.6

### v1.0.0-pre (2026-02-26)
- 初始版本
- **修复**: 移除无效的 `FloatMenuMakerMap.AddHumanlikeOrders` 补丁（RimWorld 1.6 已移除该方法，改用 `FloatMenuOptionProvider` 系统）
- **修复**: 修复 `DoReload` 方法中资源被双重消耗但装填次数未增长的问题
- **修复**: 补充缺失的翻译键 `BetterEquipmentReloading_ReloadLabel`
- **修复**: 修复使用次数耗尽后 Gizmo 变灰无法右键装填的问题

