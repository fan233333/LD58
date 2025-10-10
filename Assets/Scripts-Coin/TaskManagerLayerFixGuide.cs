/*
TaskManager 图层遮挡问题修复说明
============================

问题描述：
对话被图层挡住了，无法正常显示对话界面

修复方案：
=========

现在的流程是：
1. 玩家点击触发结局对话
2. PlayEndStory() 自动隐藏 img1 和 img2，防止图层遮挡
3. 播放结局对话（此时图片被隐藏，对话界面清晰可见）
4. 对话结束后，CheckStoryCompletion() 检测到对话完成
5. 调用 ShowEndingImages() 重新显示图片并开始切换动画
6. 图片切换完成后，玩家可以点击返回主菜单

代码修改要点：
=============

1. PlayEndStory() 方法：
   - 播放对话前隐藏 img1 和 img2
   - 防止图层遮挡对话界面

2. CheckStoryCompletion() 方法：
   - 检测对话结束
   - 调用 ShowEndingImages() 重新显示图片

3. ShowEndingImages() 方法（新增）：
   - 重新激活图片
   - 设置透明度
   - 开始图片切换动画

4. Update() 方法简化：
   - 点击时只调用 PlayEndStory()
   - 图片显示逻辑移至对话结束后

用户体验：
=========

修复后的用户体验：
1. 点击屏幕 → 图片消失，对话开始
2. 阅读对话内容（没有图层遮挡）
3. 对话结束 → 图片重新出现并开始切换
4. 图片切换完成 → 可以点击返回主菜单

技术细节：
=========

图层管理：
- 对话播放时：img1.SetActive(false), img2.SetActive(false)
- 对话结束时：重新激活并开始切换动画

状态管理：
- isEndStoryPlaying：控制对话播放状态
- CheckStoryCompletion()：持续监测对话结束
- ShowEndingImages()：处理对话后的图片显示

调试提示：
=========

如果仍有问题，检查以下项目：
1. InkReader 的 Canvas 层级设置
2. img1 和 img2 的 Canvas 层级设置  
3. 确保对话UI的 Canvas 排序层高于图片
4. 使用 Debug 方法测试各个阶段

Console 输出说明：
- "Playing end story" - 对话开始播放
- "End story completed" - 对话播放完成
- "Showing ending images after story completion" - 开始显示结局图片

这个修复确保了对话界面不会被图片遮挡，提供了清晰的用户体验。
*/