/*
GameStart 目标光年修复说明
========================

问题描述：
在ReloadCurrentLevelWithLoading方法中，SetLoadingData的最后一个参数
（targetLightYear）被硬编码为100，这是不正确的。

修复方案：
=========

1. 问题分析：
   - SetLoadingData(sceneName, currentLightYear, numScene, isCompletion, isRetry, targetLightYear)
   - 最后一个参数应该是"还需要达到的目标光年"，而不是固定值100
   - 这个值应该从TaskManager.highLightYear获取

2. 修复实现：
   
   修复前：
   ```csharp
   LoadingData.SetLoadingData(currentSceneName, SeedStatic.lightYear, SeedStatic.numScene, false, true, 100);
   ```
   
   修复后：
   ```csharp
   int targetLightYear = GetTargetLightYear();
   LoadingData.SetLoadingData(currentSceneName, SeedStatic.lightYear, SeedStatic.numScene, false, true, targetLightYear);
   ```

3. GetTargetLightYear方法：

   功能：
   - 动态获取当前场景中TaskManager的highLightYear值
   - 如果找不到TaskManager，返回默认值100
   - 提供警告信息帮助调试

   实现逻辑：
   ```csharp
   private int GetTargetLightYear()
   {
       TaskManager taskManager = FindObjectOfType<TaskManager>();
       if (taskManager != null)
       {
           return taskManager.highLightYear;
       }
       
       Debug.LogWarning("TaskManager not found, using default target light year: 100");
       return 100;
   }
   ```

修复意义：
=========

1. 准确性：
   - 现在会使用正确的目标光年值
   - 不同关卡可能有不同的目标光年
   - 确保Loading界面显示正确的进度信息

2. 灵活性：
   - 支持动态配置的目标光年
   - 不依赖硬编码值
   - 适应不同关卡的设计需求

3. 健壮性：
   - 提供默认值作为后备方案
   - 包含调试信息帮助问题排查
   - 不会因为找不到TaskManager而崩溃

使用场景：
=========

正确的目标光年获取对以下场景很重要：

1. Loading界面的光年进度显示：
   - 显示"已飞越XX光年，还剩XX光年"
   - 需要知道正确的目标值来计算剩余光年

2. 神秘模式的触发条件：
   - 当接近目标光年时可能需要特殊显示
   - 需要准确的目标值来判断

3. 游戏结局的触发：
   - 当达到目标光年时触发结局
   - 必须使用正确的目标值

技术细节：
=========

FindObjectOfType<TaskManager>()的使用：
- 在当前场景中查找TaskManager组件
- 返回找到的第一个TaskManager实例
- 如果场景中没有TaskManager，返回null

性能考虑：
- FindObjectOfType是相对较慢的操作
- 但在重载关卡时调用频率不高
- 可以考虑缓存TaskManager引用以优化性能

扩展建议：
=========

1. 添加TaskManager引用缓存：
   ```csharp
   private TaskManager cachedTaskManager;
   ```

2. 支持多个TaskManager的情况：
   - 如果场景中有多个TaskManager
   - 可以通过标签或名称来区分

3. 添加配置文件支持：
   - 将目标光年配置存储在ScriptableObject中
   - 支持更灵活的关卡配置

这个修复确保了GameStart中的重载功能使用正确的目标光年值，
提高了游戏的准确性和一致性。
*/