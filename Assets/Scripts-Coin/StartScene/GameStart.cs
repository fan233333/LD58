using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameStart : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI lightYearText; // 显示光年的文本组件
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateLightYearDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLightYearDisplay();
    }

    /// <summary>
    /// 更新光年显示文本
    /// </summary>
    void UpdateLightYearDisplay()
    {
        if (lightYearText != null)
        {
            string lightYearInfo = SeedStatic.isEng 
                ? $"Currently traveled {SeedStatic.lightYear} light years"
                : $"目前行驶了{SeedStatic.lightYear}光年";
            
            lightYearText.text = lightYearInfo;
        }
    }

    public void OnClick()
    {
        SceneManager.LoadScene(1);
    }

    public void Retry()
    {
        SeedStatic.lightYear = 1;
        SceneManager.LoadScene(1);
    }

    public void Back()
    {
        SeedStatic.lightYear = 1;
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// 重载当前关卡，保持当前光年不变
    /// </summary>
    public void ReloadCurrentLevel()
    {
        // 获取当前场景名称
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // 保持当前光年不变，重新加载当前场景
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// 重载当前关卡使用加载界面，保持当前光年
    /// </summary>
    public void ReloadCurrentLevelWithLoading()
    {
        // 获取当前场景名称
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // 使用加载界面重载当前关卡，保持光年不变
        try
        {
            // 获取目标光年（TaskManager中的highLightYear值）
            int targetLightYear = GetTargetLightYear();
            LoadingData.SetLoadingData(currentSceneName, SeedStatic.lightYear, SeedStatic.numScene, false, true, targetLightYear);
            SceneManager.LoadScene("LoadingScene");
        }
        catch (System.Exception)
        {
            // 如果LoadingData不可用，直接重载当前场景
            Debug.LogWarning("LoadingData not available, using direct scene reload");
            ReloadCurrentLevel();
        }
    }

    /// <summary>
    /// 智能重载关卡 - 根据场景类型选择合适的重载方式
    /// </summary>
    public void SmartReloadLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // 如果当前是游戏场景，使用带加载界面的重载
        if (IsGameScene(currentSceneName))
        {
            ReloadCurrentLevelWithLoading();
        }
        else
        {
            // 如果是菜单场景等，直接重载
            ReloadCurrentLevel();
        }
    }

    /// <summary>
    /// 判断是否为游戏场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <returns>是否为游戏场景</returns>
    private bool IsGameScene(string sceneName)
    {
        // 定义游戏场景的名称模式
        string[] gameSceneNames = { "GameScene", "Game", "Level", "Task", "Play" };
        
        foreach (string pattern in gameSceneNames)
        {
            if (sceneName.Contains(pattern))
            {
                return true;
            }
        }
        
        // 也可以通过场景索引判断（假设索引1以上都是游戏场景）
        Scene currentScene = SceneManager.GetActiveScene();
        return currentScene.buildIndex > 0;
    }

    /// <summary>
    /// 获取目标光年值（从TaskManager中获取highLightYear）
    /// </summary>
    /// <returns>目标光年值</returns>
    private int GetTargetLightYear()
    {
        // 尝试从场景中查找TaskManager组件
        TaskManager taskManager = FindObjectOfType<TaskManager>();
        if (taskManager != null)
        {
            return taskManager.highLightYear;
        }
        
        // 如果找不到TaskManager，返回默认值100
        Debug.LogWarning("TaskManager not found, using default target light year: 100");
        return 100;
    }
}
