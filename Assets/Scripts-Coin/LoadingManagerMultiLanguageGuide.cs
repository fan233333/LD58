/// <summary>
/// LoadingManager 多语言使用指南
/// 
/// 这个脚本展示了如何在LoadingManager中使用多语言功能
/// 按照与ManageScenes相同的模式实现
/// </summary>

/*
=== 多语言实现说明 ===

1. 语言控制机制：
   - 使用 SeedStatic.isEng 来判断当前语言
   - true = 英文，false = 中文

2. 文本配置方式：
   - 每种文本都有CN和EN两个版本的数组
   - 通过属性（Property）来自动选择对应语言的文本

3. 支持的多语言文本类型：
   
   a) 加载提示文本：
      - generalLoadingTipsCN/EN: 通用加载提示
      - completionLoadingTipsCN/EN: 任务完成提示
      - retryLoadingTipsCN/EN: 重试任务提示
   
   b) UI标签文本：
      - loadingTextCN/EN: "加载中..." / "Loading..."
      - completeTextCN/EN: "完成" / "Complete"
      - 光年标签: 动态生成 "光年" / "Light Year"

4. 属性使用：
   - CurrentGeneralTips: 当前语言的通用提示
   - CurrentCompletionTips: 当前语言的完成提示
   - CurrentRetryTips: 当前语言的重试提示
   - CurrentLoadingText: 当前语言的加载文本
   - CurrentCompleteText: 当前语言的完成文本

5. 在Unity Inspector中的配置：
   
   [Header("加载提示文本 - 中文")]
   - 配置所有中文版本的提示文本数组
   
   [Header("加载提示文本 - 英文")]
   - 配置所有英文版本的提示文本数组
   
   [Header("其他UI文本")]
   - 配置其他UI标签的中英文版本

6. 运行时的工作流程：
   
   a) InitializeLoading():
      - 检查当前语言设置
      - 记录调试信息
   
   b) SetupUI():
      - 根据 SeedStatic.isEng 选择对应语言的光年标签
      - 根据加载类型和语言选择合适的提示文本
      - 显示随机选择的提示文本
   
   c) CompleteLoadingAnimation():
      - 显示对应语言的"完成"文本

7. 调试功能：
   
   - [ContextMenu("Debug Language Settings")]: 显示当前语言设置
   - [ContextMenu("Toggle Language (Test Only)")]: 切换语言（仅测试用）
   - [ContextMenu("Force Complete Loading")]: 强制完成加载

8. 实际使用示例：
   
   // 在代码中的使用方式
   string tip = CurrentGeneralTips[Random.Range(0, CurrentGeneralTips.Length)];
   loadingTipText.text = tip;
   
   // 动态UI文本
   string lightYearLabel = SeedStatic.isEng ? "Light Year" : "光年";
   lightYearText.text = $"{lightYearLabel}: {LoadingData.currentLightYear:F1}";

9. 与其他系统的集成：
   
   - 与ManageScenes的故事系统保持一致的语言设置
   - 与TaskManager的场景切换逻辑协调工作
   - 自动响应SeedStatic.isEng的变化

10. 扩展建议：
    
    - 可以添加更多语言支持（如日文、韩文等）
    - 可以将文本配置外部化到JSON或XML文件
    - 可以添加字体切换功能以支持不同语言的字体需求

=== 配置清单 ===

在Unity Inspector中需要配置的项目：
□ generalLoadingTipsCN[]: 中文通用提示（至少5条）
□ completionLoadingTipsCN[]: 中文完成提示（至少5条）
□ retryLoadingTipsCN[]: 中文重试提示（至少5条）
□ generalLoadingTipsEN[]: 英文通用提示（至少5条）
□ completionLoadingTipsEN[]: 英文完成提示（至少5条）
□ retryLoadingTipsEN[]: 英文重试提示（至少5条）
□ loadingTextCN: "加载中..."
□ loadingTextEN: "Loading..."
□ completeTextCN: "完成"
□ completeTextEN: "Complete"

测试步骤：
1. 在Inspector中配置所有文本数组
2. 运行游戏并触发场景切换
3. 使用右键菜单"Debug Language Settings"查看当前设置
4. 使用右键菜单"Toggle Language"测试语言切换
5. 验证所有文本都正确显示对应语言版本

*/

using UnityEngine;

public class LoadingManagerMultiLanguageGuide : MonoBehaviour
{
    [TextArea(20, 30)]
    public string guideText = @"
请参考脚本顶部的注释，了解如何配置和使用LoadingManager的多语言功能。

主要步骤：
1. 在Inspector中配置所有中英文文本数组
2. 确保SeedStatic.isEng正确反映当前语言设置
3. 测试不同场景切换情况下的文本显示
4. 使用调试功能验证语言切换效果

这个系统与ManageScenes的多语言实现保持一致，
确保整个游戏的语言体验统一。
    ";
}