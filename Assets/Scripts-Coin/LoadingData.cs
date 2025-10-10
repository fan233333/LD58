using UnityEngine;

/// <summary>
/// 静态类，用于在场景间传递加载数据
/// </summary>
public static class LoadingData
{
    /// <summary>
    /// 目标场景名称
    /// </summary>
    public static string targetScene;
    
    /// <summary>
    /// 当前光年数
    /// </summary>
    public static int currentLightYear;
    
    /// <summary>
    /// 当前场景计数
    /// </summary>
    public static int currentSceneCount;
    
    /// <summary>
    /// 是否来自任务完成（用于显示不同的加载提示）
    /// </summary>
    public static bool isFromTaskCompletion;
    
    /// <summary>
    /// 是否来自任务失败重试
    /// </summary>
    public static bool isFromTaskRetry;
    
    /// <summary>
    /// 目标光年数（游戏目标）
    /// </summary>
    public static int targetLightYear = 100;
    
    /// <summary>
    /// 特殊光年阈值（超过此值显示神秘信息）
    /// </summary>
    public static int mysteryLightYearThreshold = 100;
    
    /// <summary>
    /// 设置加载数据
    /// </summary>
    /// <param name="sceneName">目标场景名</param>
    /// <param name="lightYear">当前光年数</param>
    /// <param name="sceneCount">场景计数</param>
    /// <param name="fromCompletion">是否来自任务完成</param>
    /// <param name="fromRetry">是否来自重试</param>
    /// <param name="targetLY">目标光年数</param>
    public static void SetLoadingData(string sceneName, int lightYear, int sceneCount, bool fromCompletion = false, bool fromRetry = false, int targetLY = 100)
    {
        targetScene = sceneName;
        currentLightYear = lightYear;
        currentSceneCount = sceneCount;
        isFromTaskCompletion = fromCompletion;
        isFromTaskRetry = fromRetry;
        targetLightYear = targetLY;
        
        Debug.Log($"Loading data set: {sceneName}, LY: {lightYear}/{targetLY}, Scene: {sceneCount}, Completion: {fromCompletion}, Retry: {fromRetry}");
    }
    
    /// <summary>
    /// 重置加载数据
    /// </summary>
    public static void Reset()
    {
        targetScene = "";
        currentLightYear = 0;
        currentSceneCount = 0;
        isFromTaskCompletion = false;
        isFromTaskRetry = false;
    }
}