# Better Equipment Reloading | 装备装填增强

为 RimWorld 原版消耗型装备提供增强的装填体验。

**版本**: v1.0.0  
**游戏版本**: RimWorld 1.6  
**作者**: LongYinHaHa

---

## 功能特点

### 智能装填逻辑
- **库存优先**: 优先从 Pawn 身上取用资源
- **地图补充**: 库存不足时自动检索地图上可达的资源堆
- **合并搬运**: 支持多堆资源的合并搬运装填

### UI 增强
- **次数显示**: 在装备 Gizmo 右上角显示剩余次数（当前/最大）
- **右键装填**: 右键点击能力按钮打开装填菜单
- **状态提示**: 清晰显示当前弹药状态

### 兼容性
- 自动适配所有使用 `CompApparelReloadable` 的原版装备
- 不影响 SkylarTech 模组自身的装填逻辑
- 保留原版装填逻辑作为兼容兜底

---

## 支持的装备

| 装备 | 装填材料 |
|------|----------|
| 烟雾弹背包 (Smokepop Pack) | 化合燃料 |
| 跳跃背包 (Jump Pack) | 化合燃料 |
| 毒气背包 (Tox Pack) | 化合燃料 |
| 其他原版消耗型装备 | 按原版配置 |

---

## 前置依赖

| 模组 | 用途 |
|------|------|
| Harmony | 必需 - 代码注入 |

---

## 使用指南

### 装填方式

**右键菜单装填**:
1. 右键点击装备的技能按钮
2. 选择"装填"选项
3. Pawn 自动寻找并搬运资源进行装填

### 装填流程

1. **库存检查**: Pawn 从库存取用资源（如果有）
2. **地图检索**: 如果库存不足，走到地图资源处
3. **资源搬运**: 搬运资源（支持多堆合并）
4. **装填等待**: 等待约 2 秒（显示进度条）
5. **完成装填**: 完成装填，丢弃剩余资源

### 次数显示

装备按钮右上角显示: `当前次数/最大次数`

例如: `烟雾弹 (2/3)`

---

## 技术实现

### 核心组件

**IReloadableComp 接口**
- 统一装填组件接口
- 支持使用次数管理
- 资源检索方法

**CompBetterReloadable**
- 核心装填组件
- 管理使用次数
- 实现资源检索逻辑

**ResourceFinder**
- 资源检索工具类
- 库存 + 地图资源查找
- 按数量排序优化

### Harmony 补丁

**原版装备注入**
- 为 `CompApparelReloadable` 注入增强功能
- `VanillaReloadableWrapper` 统一接口

**右键菜单**
- 拦截 Gizmo 右键事件
- 添加装填菜单选项

**次数显示**
- 修改 `Command_VerbOwner.TopRightLabel`
- 显示当前/最大次数

---

## 兼容性

### 已知兼容
- Harmony 前置
- SkylarTech 模组
- 大多数装备模组

### 可能冲突
- 其他修改 `CompApparelReloadable` 的模组

---

## 翻译支持

| 键值 | 中文 | 英文 |
|------|------|------|
| `BetterEquipmentReloading_ReloadLabel` | 装填 | Reload |
| `BetterEquipmentReloading_NoResource` | 没有足够的 {0} 进行装填 | Not enough {0} to reload |
| `BetterEquipmentReloading_AlreadyFull` | 装备已满，无需装填 | Equipment is already full |

---

## 更新日志

### v1.0.0 (2026-02-26)
- **正式版发布**
- 实现核心装填逻辑移植
- 支持库存优先 + 地图补充
- 支持合并搬运多堆资源
- 添加右键菜单装填选项
- 添加 Gizmo 剩余次数显示
- 添加中英文翻译支持

### v1.0.0-pre (2026-02-26)
- 初始版本
- 适配 RimWorld 1.6 的 `FloatMenuOptionProvider` 系统

---

## 开发者信息

详细技术实现参见 [Source/README.md](Source/README.md)

---

*文档版本: 2026-02-26*
