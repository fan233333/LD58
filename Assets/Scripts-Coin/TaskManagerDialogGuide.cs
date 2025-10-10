/*
TaskManager 对话系统使用指南
=======================

概述:
TaskManager现在支持在任务完成后、两幅图切换之间插入对话，
与ManageScenes中的对话系统格式保持一致。

功能特性:
1. 双语言支持 (中文/英文)
2. 自动对话播放
3. 对话状态管理
4. 防止重复播放
5. 调试工具

设置步骤:
=========

1. 在Unity Inspector中配置TaskManager:
   
   [结束对话设置]
   - End Story Chinese CN: 拖入中文Ink故事资源
   - End Story Chinese EN: 拖入英文Ink故事资源

2. 确保场景中有InkReader组件:
   - TaskManager会自动查找场景中的InkReader
   - 如果没有找到会在Console中显示警告

工作流程:
=========

当玩家达到目标光年数并点击第一张图片时:

1. 图片开始切换 (img1 -> img2)
2. 自动播放结束对话 (根据当前语言选择)
3. 对话播放期间阻止用户交互
4. 对话结束后允许点击返回主菜单

代码实现细节:
============

核心属性:
- SeedStatic.isEng: 控制语言切换
- CurrentEndStory: 根据语言返回对应的故事资源
- IsEndStoryPlaying: 检查对话是否正在播放

关键方法:
- PlayEndStory(): 播放结束对话
- StopEndStory(): 停止对话播放  
- CheckStoryCompletion(): 检查对话是否完成
- FindAndSetupInkReader(): 查找并设置InkReader

状态管理:
- isEndStoryPlaying: 对话播放状态
- hasEndStoryPlayed: 防止重复播放
- _inkReader: InkReader组件引用

交互逻辑:
========

在Update方法中:
1. 任务计时器会在对话播放时暂停
2. 第一次点击触发图片切换和对话播放
3. 第二次点击需等待对话结束才能返回主菜单

调试功能:
=========

在Inspector中右键TaskManager可以看到:
- Force Play End Story: 强制播放结束对话
- Reset End Story Progress: 重置对话进度

使用建议:
=========

1. 故事资源命名规范:
   - 中文: EndStory_CN.ink
   - 英文: EndStory_EN.ink

2. 确保InkReader在场景中只有一个实例

3. 测试时使用调试菜单验证对话播放

4. 对话内容应该简短，符合游戏节奏

语言切换:
=========

对话会自动根据SeedStatic.isEng的值选择语言:
- true: 播放英文对话 (endStoryChineseEN)
- false: 播放中文对话 (endStoryChineseCN)

注意事项:
=========

1. 确保Ink故事资源已正确编译
2. 对话播放期间其他交互会被暂停
3. 每个任务周期对话只会播放一次
4. 语言切换后需要重新开始任务才会使用新语言的对话

故障排除:
=========

如果对话不播放:
1. 检查Console是否有"No InkReader found"警告
2. 确认故事资源已正确分配
3. 验证SeedStatic.isEng的值是否正确
4. 使用调试菜单测试对话功能

性能考虑:
=========

1. InkReader只在Start时查找一次
2. 对话状态检查频率较低
3. 防止重复播放减少不必要的加载

集成说明:
=========

此对话系统与现有系统完美集成:
- 兼容ManageScenes的对话管理
- 不影响TaskManager的核心功能  
- 保持与LoadingManager的场景切换逻辑一致

*/