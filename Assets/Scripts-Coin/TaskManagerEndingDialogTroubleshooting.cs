/*
TaskManager 结局对话问题排查指南
==========================

问题描述：
到达结局的时候并没有正常触发对话

已修复的问题：
============

1. 原始问题：
   - 在TaskFailed方法中，当达到结局条件时只显示了img1
   - 对话触发需要玩家手动点击才能开始
   - 没有明确的UI提示告诉玩家需要点击

2. 修复方案：
   - 修改TaskFailed方法，在达到结局条件时自动触发对话
   - 自动开始图片切换动画，无需玩家点击
   - 添加详细的调试信息帮助排查问题

修复后的流程：
=============

当任务失败且光年达到目标时：
1. TaskFailed() 被调用
2. 检查 SeedStatic.lightYear >= highLightYear
3. 自动设置两张图片的透明度和激活状态
4. 自动调用 PlayEndStory() 播放对话
5. 自动开始 FadeTransition() 图片切换动画
6. 对话播放完成后，允许点击返回主菜单

排查步骤：
=========

如果对话仍然不触发，请按以下步骤排查：

1. 检查Inspector设置：
   - 确认 endStoryChineseCN 已分配Ink资源
   - 确认 endStoryChineseEN 已分配Ink资源
   - 确认 highLightYear 设置正确（默认100）

2. 检查场景配置：
   - 确认场景中存在InkReader组件
   - 确认InkReader处于激活状态

3. 使用调试工具：
   在Inspector中右键TaskManager，选择：
   - "Debug Ending Status" - 查看当前状态
   - "Force Trigger Ending" - 强制触发结局
   - "Force Play End Story" - 强制播放对话

4. 检查Console输出：
   查找以下关键信息：
   - "Reached ending condition" - 确认结局条件满足
   - "PlayEndStory called" - 确认对话被调用
   - "Playing end story" - 确认对话开始播放
   - "No end story asset assigned" - 缺少Ink资源
   - "No InkReader available" - 缺少InkReader

常见问题及解决方案：
================

问题1：Console显示 "No end story asset assigned"
解决：在Inspector中为TaskManager分配对应的Ink故事资源

问题2：Console显示 "No InkReader available"
解决：确保场景中存在InkReader组件，或手动创建

问题3：光年数值不正确
解决：检查 SeedStatic.lightYear 和 highLightYear 的值

问题4：对话资源为空
解决：确认Ink文件已正确编译，且在Resources或直接引用

问题5：语言切换问题
解决：检查 SeedStatic.isEng 的值，确保选择正确的语言资源

测试建议：
=========

1. 临时测试：
   - 降低 highLightYear 值（如设为10）
   - 使用 Force Trigger Ending 立即测试
   - 在游戏运行时切换语言测试双语言支持

2. 完整测试：
   - 正常游戏流程直到结局
   - 测试中英文两种语言
   - 验证对话完成后的返回主菜单功能

代码更改摘要：
=============

修改位置：TaskManager.TaskFailed()
- 添加自动对话触发
- 添加自动图片切换
- 添加调试信息

修改位置：TaskManager.PlayEndStory()
- 增强调试信息
- 改进错误提示

新增调试方法：
- DebugEndingStatus() - 状态检查
- ForceTriggerEnding() - 强制触发

预期结果：
=========

修复后，当玩家任务失败且达到目标光年时：
1. 自动显示结局图片
2. 自动播放结局对话
3. 对话结束后可点击返回主菜单
4. 无需额外的用户交互

如果问题持续存在，请使用调试工具收集详细信息。
*/