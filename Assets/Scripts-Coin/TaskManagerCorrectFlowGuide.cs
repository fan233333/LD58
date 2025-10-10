/*
TaskManager 正确的结局流程说明
==========================

修复后的完整流程：
================

1. 任务失败且达到目标光年 (SeedStatic.lightYear >= highLightYear)
   ↓
2. TaskFailed() 被调用
   - 只显示第一张图 (img1)
   - 设置 img1 的透明度为 1f
   - 激活 img1.gameObject
   - 不激活 img2，不播放对话
   ↓
3. 等待用户点击屏幕
   ↓
4. 用户点击 → PlayEndStory() 被调用
   - 隐藏 img1 和 img2 (防止图层遮挡)
   - 开始播放结局对话
   ↓
5. 对话播放中
   - 图片被隐藏，对话界面清晰可见
   - isEndStoryPlaying = true
   ↓
6. 对话结束 → CheckStoryCompletion() 检测到
   - isEndStoryPlaying = false
   - 调用 ShowEndingImages()
   ↓
7. ShowEndingImages() 执行
   - 重新激活 img1 和 img2
   - 设置透明度
   - 开始 FadeTransition() 动画
   ↓
8. FadeTransition() 执行
   - img1 透明度从 1 → 0
   - img2 保持透明度 1
   - 动画结束后设置 isImg2 = true
   ↓
9. 等待用户再次点击返回主菜单

关键修复点：
===========

1. TaskFailed() 方法现在只显示第一张图：
   ```csharp
   // 只显示第一张图，等待用户点击
   SetImageAlpha(img1, 1f);
   img1.gameObject.SetActive(true);
   // 不激活img2，不播放对话
   ```

2. 保持原有的点击逻辑：
   - 第一次点击：播放对话
   - 对话结束：自动显示图片并切换
   - 第二次点击：返回主菜单

3. 图层管理：
   - 对话播放时：隐藏所有图片
   - 对话结束时：重新显示图片

用户体验：
=========

现在用户看到的是：
1. 任务失败 → 看到第一张结局图片
2. 点击屏幕 → 图片消失，对话开始
3. 阅读对话 → 清晰可见，无遮挡
4. 对话结束 → 图片重现并开始切换动画
5. 动画完成 → 点击返回主菜单

调试验证：
=========

Console 输出顺序应该是：
1. "Reached ending condition - LightYear: X, Target: Y"
2. [用户点击]
3. "PlayEndStory called"
4. "Playing end story: [story_name]"
5. "End story completed"
6. "Showing ending images after story completion"

如果第一张图仍然不显示，检查：
1. img1 是否在Inspector中正确分配
2. img1 的初始状态（可能默认被隐藏）
3. Canvas 层级设置
4. TaskFailed() 是否被正确调用

这个修复确保了正确的视觉流程：先图片，后对话，再图片切换。
*/