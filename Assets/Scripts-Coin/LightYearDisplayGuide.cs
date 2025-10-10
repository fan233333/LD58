/// <summary>
/// LoadingManager 光年显示系统使用指南
/// 
/// 这个系统实现了动态光年信息显示，包括逐字显示效果和神秘光年模式
/// </summary>

/*
=== 光年显示系统功能说明 ===

1. 系统组成：
   - LoadingData: 存储光年相关数据
   - LoadingManager: 处理光年文本生成和显示效果
   - TaskManager: 传递光年目标信息

2. 光年信息格式：
   
   a) 正常模式（当前光年 < 神秘阈值）：
      中文: "已经飞越了XX.X光年，还剩XX.X光年"
      英文: "Traveled XX.X light years, XX.X light years remaining"
   
   b) 神秘模式（当前光年 >= 神秘阈值）：
      中文: "已经飞越了XX.X光年，还剩？？？？？？光年？？"
      英文: "Traveled XX.X light years, ？？？？？？ light years remaining？？"

3. 逐字显示效果：
   - 可配置的打字速度 (lightYearTypingSpeed)
   - 可以通过鼠标点击跳过打字效果
   - 可以完全禁用打字效果 (enableTypingEffect = false)

4. 配置参数：
   
   [Header("光年文本显示设置")]
   - lightYearTypingSpeed: 逐字显示速度（秒/字符），默认0.05秒
   - enableTypingEffect: 是否启用逐字显示，默认true
   - mysteryThreshold: 特殊光年阈值，默认100光年

5. 数据流程：
   
   TaskManager → LoadingData → LoadingManager
   
   a) TaskManager调用:
      LoadingData.SetLoadingData(sceneName, currentLY, sceneCount, completion, retry, targetLY)
   
   b) LoadingData存储:
      - currentLightYear: 当前光年数
      - targetLightYear: 目标光年数  
      - mysteryLightYearThreshold: 神秘阈值
   
   c) LoadingManager处理:
      - GenerateLightYearText(): 生成显示文本
      - StartLightYearTypingEffect(): 开始逐字显示
      - TypeLightYearText(): 执行逐字显示协程

6. 交互功能：
   - 鼠标点击: 跳过打字效果，立即显示完整文本
   - 自动完成: 打字效果完成后自动停止

7. 调试工具：
   - [ContextMenu("Test Light Year Display")]: 测试不同光年数的显示
   - [ContextMenu("Restart Light Year Display")]: 重新开始显示
   - [ContextMenu("Skip Light Year Typing")]: 跳过打字效果

8. 使用示例：

   // 在TaskManager中设置目标光年
   public int targetLightYear = 100;
   
   // 调用加载界面时传递目标光年
   LoadingData.SetLoadingData(nextScene, SeedStatic.lightYear, sceneCount, true, false, targetLightYear);

9. 自定义配置：

   // 调整打字速度
   loadingManager.lightYearTypingSpeed = 0.03f; // 更快
   
   // 禁用打字效果
   loadingManager.enableTypingEffect = false;
   
   // 设置神秘阈值
   loadingManager.mysteryThreshold = 150f;

10. 多语言支持：
    - 自动根据 SeedStatic.isEng 切换中英文显示
    - 保持与游戏其他系统的语言一致性

=== 实现细节 ===

核心算法：
```csharp
// 文本生成逻辑
if (currentLY >= mysteryThreshold) {
    // 显示神秘文本
    fullLightYearText = "已经飞越了{current}光年，还剩？？？？？？光年？？";
} else {
    // 显示正常文本
    float remaining = Max(0, targetLY - currentLY);
    fullLightYearText = "已经飞越了{current}光年，还剩{remaining}光年";
}

// 逐字显示逻辑
for (int i = 0; i <= fullText.Length; i++) {
    displayText = fullText.Substring(0, i);
    yield return new WaitForSeconds(typingSpeed);
}
```

性能考虑：
- 使用协程实现逐字显示，避免阻塞主线程
- 提供跳过功能，改善用户体验
- 可配置的显示速度，适应不同需求

错误处理：
- 空文本检查
- 协程安全停止
- UI组件null检查

=== 测试清单 ===

□ 正常光年显示 (< 100光年)
□ 神秘光年显示 (>= 100光年)  
□ 中文显示效果
□ 英文显示效果
□ 逐字打字效果
□ 鼠标点击跳过功能
□ 不同打字速度测试
□ 禁用打字效果测试
□ 自定义神秘阈值测试
□ 场景切换时的光年传递
□ 多次加载的文本更新

*/

using UnityEngine;

public class LightYearDisplayGuide : MonoBehaviour
{
    [TextArea(20, 30)]
    public string guideText = @"
光年显示系统使用指南：

1. 基本配置：
   - 在LoadingManager中设置lightYearTypingSpeed控制打字速度
   - 设置enableTypingEffect控制是否启用逐字显示
   - 设置mysteryThreshold控制神秘模式触发阈值

2. 显示模式：
   - 正常模式：显示已飞越光年和剩余光年
   - 神秘模式：已飞越光年正常显示，剩余光年显示问号

3. 交互功能：
   - 点击屏幕可跳过打字效果
   - 自动根据当前语言设置显示中英文

4. 调试测试：
   - 使用右键菜单测试不同光年数的显示效果
   - 测试语言切换和打字效果

这个系统与游戏的光年进度完全集成，
提供沉浸式的星际旅行体验！
    ";
}