/*
GameStart 重载关卡和光年显示功能指南（更新版）
=========================================

新增功能概述：
=============

1. 光年显示功能
   - 实时显示当前行驶的光年数
   - 支持中英文切换显示
   - 自动更新文本内容

2. 智能重载关卡功能
   - 动态获取当前场景名称，不依赖硬编码
   - 保持当前光年不变
   - 支持多种重载方式
   - 智能选择最佳重载方案

功能详解：
=========

1. 光年显示 (UpdateLightYearDisplay)
   -------------------------------------
   
   显示文本：
   - 中文："目前行驶了XX光年"
   - 英文："Currently traveled XX light years"
   
   自动更新：
   - 在Start()中初始化显示
   - 在Update()中持续更新（响应语言切换）
   - 根据SeedStatic.isEng自动选择语言

2. 智能重载关卡功能
   ------------------
   
   三种重载方式：
   
   a) ReloadCurrentLevel()
      - 动态获取当前场景名称：SceneManager.GetActiveScene().name
      - 直接重载当前场景，保持SeedStatic.lightYear不变
      - 快速重载，无过渡动画
   
   b) ReloadCurrentLevelWithLoading()
      - 动态获取当前场景名称
      - 使用LoadingScene的过渡效果
      - 保持当前光年和场景数
      - 使用try-catch处理LoadingData异常
      - 异常时自动回退到普通重载
   
   c) SmartReloadLevel() ⭐推荐
      - 智能判断场景类型
      - 游戏场景使用Loading重载
      - 菜单场景使用直接重载
      - 自动选择最佳方案

使用设置：
=========

在Unity Inspector中设置：

1. 拖拽TextMeshProUGUI组件到lightYearText字段
2. 该文本组件会自动显示光年信息
3. 为UI按钮绑定对应的重载方法

按钮绑定建议：
- "重新尝试"按钮 → SmartReloadLevel() ⭐推荐
- "重载关卡"按钮 → ReloadCurrentLevel()
- "重载关卡(Loading)"按钮 → ReloadCurrentLevelWithLoading()

UI设计建议：
- 在失败界面添加"重新尝试"按钮
- 绑定SmartReloadLevel()方法
- 按钮文本支持中英文：
  - 中文："重新尝试"
  - 英文："Try Again"

代码结构：
=========

新增字段：
```csharp
[Header("UI组件")]
public TextMeshProUGUI lightYearText; // 显示光年的文本组件
```

核心方法：
- UpdateLightYearDisplay(): 更新光年显示文本
- ReloadCurrentLevel(): 动态获取场景名，直接重载
- ReloadCurrentLevelWithLoading(): 动态获取场景名，带Loading重载
- SmartReloadLevel(): 智能选择重载方式
- IsGameScene(): 判断是否为游戏场景

与现有功能的区别：
================

现有方法：
- OnClick(): 新游戏开始，光年重置为默认
- Retry(): 重试，光年重置为1
- Back(): 返回主菜单，光年重置为1

新增方法：
- ReloadCurrentLevel(): 动态重载当前场景，保持光年
- ReloadCurrentLevelWithLoading(): 动态重载+Loading，保持光年
- SmartReloadLevel(): 智能选择重载方式，保持光年

关键改进：
- ❌ 旧版：SceneManager.LoadScene(1) 硬编码场景索引
- ✅ 新版：SceneManager.GetActiveScene().name 动态获取场景名
- ✅ 异常处理：try-catch确保LoadingData异常时的回退机制
- ✅ 场景智能识别：自动判断游戏场景vs菜单场景

使用场景：
=========

1. 光年显示：
   - 主菜单显示当前进度
   - 游戏开始前确认进度
   - 设置界面显示当前状态

2. 重载关卡：
   - 玩家想重新挑战当前关卡
   - 保持已获得的光年进度
   - 不想从头开始游戏

技术细节：
=========

语言切换响应：
- Update()中持续检查SeedStatic.isEng
- 实时更新文本内容
- 无需手动刷新

加载界面集成：
- 检查LoadingData类是否存在
- 使用反射避免编译错误
- 优雅降级到普通加载

性能考虑：
- Update()中的文本更新开销很小
- 可根据需要优化为事件驱动更新
- 字符串格式化性能良好

扩展建议：
=========

1. 添加光年里程碑显示
2. 显示最高记录
3. 添加游戏统计信息
4. 支持更多语言选项

这个实现为游戏提供了更好的用户体验和进度管理功能。
*/