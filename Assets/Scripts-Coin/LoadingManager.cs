using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 加载界面管理器，处理飞船飞向星球的动画和场景加载
/// </summary>
public class LoadingManager : MonoBehaviour
{
    [Header("UI元素")]
    [Tooltip("进度条")]
    public Slider progressBar;
    [Tooltip("进度百分比文本")]
    public TextMeshProUGUI progressText;
    [Tooltip("加载提示文本")]
    public TextMeshProUGUI loadingTipText;
    [Tooltip("光年显示文本")]
    public TextMeshProUGUI lightYearText;
    
    [Header("飞船动画设置")]
    [Tooltip("飞船图像/对象")]
    public Transform spaceship;
    [Tooltip("飞船起始位置")]
    public Transform startPoint;
    [Tooltip("飞船目标位置（星球位置）")]
    public Transform endPoint;
    [Tooltip("飞船飞行轨迹曲线")]
    public AnimationCurve flightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("飞船轻微摇摆幅度")]
    public float spaceshipBobAmount = 0.05f;
    [Tooltip("飞船摇摆速度")]
    public float spaceshipBobSpeed = 2f;
    
    [Header("星球动画设置")]
    [Tooltip("星球图像/对象")]
    public Transform planet;
    [Tooltip("星球旋转速度")]
    public float planetRotationSpeed = 10f;
    
    [Header("背景效果")]
    [Tooltip("星星粒子系统")]
    public ParticleSystem starsParticles;
    [Tooltip("背景图像")]
    public Image backgroundImage;
    [Tooltip("其他装饰元素")]
    public GameObject[] decorativeElements;
    
    [Header("加载设置")]
    [Tooltip("最小加载时间（秒）")]
    public float minimumLoadTime = 3f;
    [Tooltip("最大加载时间（秒）")]
    public float maximumLoadTime = 8f;
    
    [Header("加载提示文本 - 中文")]
    [Tooltip("通用加载提示 - 中文")]
    public string[] generalLoadingTipsCN = {
        "正在穿越星际空间...",
        "计算飞行轨道...",
        "同步时空坐标...",
        "激活生命支持系统...",
        "扫描目标星球..."
    };
    
    [Tooltip("任务完成后的提示 - 中文")]
    public string[] completionLoadingTipsCN = {
        "任务完成！前往下一个星球...",
        "收集完成，寻找新的目标...",
        "准备探索新世界...",
        "成功！继续星际之旅...",
        "向着未知宇宙前进..."
    };
    
    [Tooltip("重试任务的提示 - 中文")]
    public string[] retryLoadingTipsCN = {
        "重新校准设备...",
        "分析失败原因...",
        "准备再次尝试...",
        "调整策略中...",
        "不要放弃，再试一次！"
    };

    [Header("加载提示文本 - 英文")]
    [Tooltip("通用加载提示 - 英文")]
    public string[] generalLoadingTipsEN = {
        "Traveling through interstellar space...",
        "Calculating flight trajectory...",
        "Synchronizing space-time coordinates...",
        "Activating life support systems...",
        "Scanning target planet..."
    };
    
    [Tooltip("任务完成后的提示 - 英文")]
    public string[] completionLoadingTipsEN = {
        "Mission complete! Heading to next planet...",
        "Collection completed, searching for new targets...",
        "Preparing to explore new worlds...",
        "Success! Continuing the interstellar journey...",
        "Advancing into the unknown universe..."
    };
    
    [Tooltip("重试任务的提示 - 英文")]
    public string[] retryLoadingTipsEN = {
        "Recalibrating equipment...",
        "Analyzing failure causes...",
        "Preparing to try again...",
        "Adjusting strategy...",
        "Don't give up, try once more!"
    };

    [Header("其他UI文本")]
    [Tooltip("加载中文本 - 中文/英文")]
    public string loadingTextCN = "加载中...";
    public string loadingTextEN = "Loading...";
    
    [Tooltip("完成文本 - 中文/英文")]
    public string completeTextCN = "完成";
    public string completeTextEN = "Complete";
    
    [Header("光年文本显示设置")]
    [Tooltip("逐字显示速度（秒/字符）")]
    public float lightYearTypingSpeed = 0.05f;
    [Tooltip("是否启用逐字显示效果")]
    public bool enableTypingEffect = true;
    [Tooltip("特殊光年阈值（超过此值显示神秘信息）")]
    public int mysteryThreshold = 8;

    // 属性：根据语言设置返回对应的提示文本
    public string[] CurrentGeneralTips => SeedStatic.isEng ? generalLoadingTipsEN : generalLoadingTipsCN;
    public string[] CurrentCompletionTips => SeedStatic.isEng ? completionLoadingTipsEN : completionLoadingTipsCN;
    public string[] CurrentRetryTips => SeedStatic.isEng ? retryLoadingTipsEN : retryLoadingTipsCN;
    
    // 其他UI文本属性
    public string CurrentLoadingText => SeedStatic.isEng ? loadingTextEN : loadingTextCN;
    public string CurrentCompleteText => SeedStatic.isEng ? completeTextEN : completeTextCN;
    
    // 私有变量
    private string targetSceneName;
    private float currentProgress = 0f;
    private bool isLoading = false;
    private Vector3 spaceshipBasePosition;
    
    // 光年信息逐字显示相关变量
    private string fullLightYearText = "";
    private bool isTypingLightYear = false;
    private Coroutine lightYearTypingCoroutine;
    
    public static LoadingManager Instance { get; private set; }
    
    void Awake()
    {
        // 简单的单例模式，因为这是加载场景，不需要DontDestroyOnLoad
        Instance = this;
    }
    
    void Start()
    {
        InitializeLoading();
    }
    
    void Update()
    {
        if (!isLoading) return;
        
        UpdateAnimations();
        
        // 检测点击跳过光年打字效果
        if (isTypingLightYear && Input.GetMouseButtonDown(0))
        {
            CompleteLightYearTyping();
        }
    }
    
    /// <summary>
    /// 初始化加载过程
    /// </summary>
    void InitializeLoading()
    {
        // 获取加载数据
        targetSceneName = LoadingData.targetScene;
        
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("No target scene specified in LoadingData!");
            // 如果没有目标场景，默认返回主菜单
            targetSceneName = "MainMenu";
        }
        
        // 调试信息：显示当前语言设置
        Debug.Log($"Loading Manager initialized - Language: {(SeedStatic.isEng ? "EN" : "CN")}, Target: {targetSceneName}");
        
        // 设置UI显示
        SetupUI();
        
        // 开始加载过程
        StartCoroutine(LoadSceneAsync());
    }
    
    /// <summary>
    /// 设置UI显示
    /// </summary>
    void SetupUI()
    {
        // 生成光年信息文本并开始逐字显示
        if (lightYearText != null)
        {
            GenerateLightYearText();
            StartLightYearTypingEffect();
        }
        
        // 选择合适的加载提示（根据语言和加载类型）
        string[] selectedTips = CurrentGeneralTips;
        
        if (LoadingData.isFromTaskCompletion)
        {
            selectedTips = CurrentCompletionTips;
        }
        else if (LoadingData.isFromTaskRetry)
        {
            selectedTips = CurrentRetryTips;
        }
        
        // 显示随机加载提示
        if (loadingTipText != null && selectedTips.Length > 0)
        {
            loadingTipText.text = selectedTips[Random.Range(0, selectedTips.Length)];
        }
        
        // 重置飞船位置
        if (spaceship != null && startPoint != null)
        {
            spaceship.position = startPoint.position;
            spaceshipBasePosition = startPoint.position;
        }
        
        // 激活装饰元素
        foreach (var element in decorativeElements)
        {
            if (element != null)
            {
                element.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// 异步加载场景
    /// </summary>
    IEnumerator LoadSceneAsync()
    {
        isLoading = true;
        float startTime = Time.time;
        
        Debug.Log($"Starting to load scene: {targetSceneName}");
        
        // 开始异步加载
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetSceneName);
        asyncOperation.allowSceneActivation = false;
        
        // 加载过程
        while (!asyncOperation.isDone)
        {
            float elapsedTime = Time.time - startTime;
            
            // 计算真实加载进度 (0-0.9)
            float realProgress = asyncOperation.progress / 0.9f;
            
            // 计算时间进度（确保最小加载时间）
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
            
            // 使用较小的进度值确保最小加载时间
            currentProgress = Mathf.Min(realProgress, timeProgress);
            
            // 添加一些随机性让进度看起来更自然
            if (currentProgress < 0.9f)
            {
                currentProgress += Random.Range(0f, 0.02f) * Time.deltaTime;
                currentProgress = Mathf.Clamp01(currentProgress);
            }
            
            // 更新UI
            UpdateLoadingUI(currentProgress);
            
            // 当真实加载完成且达到最小时间时，准备激活场景
            if (asyncOperation.progress >= 0.9f && elapsedTime >= minimumLoadTime)
            {
                // 如果还没有达到最大加载时间，快速完成
                if (elapsedTime < maximumLoadTime)
                {
                    break;
                }
            }
            
            // 防止加载时间过长
            if (elapsedTime >= maximumLoadTime)
            {
                break;
            }
            
            yield return null;
        }
        
        // 完成加载动画
        yield return StartCoroutine(CompleteLoadingAnimation());
        
        // 激活场景
        asyncOperation.allowSceneActivation = true;
        
        isLoading = false;
        
        // 清理加载数据
        LoadingData.Reset();
    }
    
    /// <summary>
    /// 完成加载动画
    /// </summary>
    IEnumerator CompleteLoadingAnimation()
    {
        float animationTime = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationTime;
            
            // 快速完成到100%
            currentProgress = Mathf.Lerp(currentProgress, 1f, t);
            UpdateLoadingUI(currentProgress);
            
            yield return null;
        }
        
        currentProgress = 1f;
        UpdateLoadingUI(currentProgress);
        
        // 显示完成文本
        if (loadingTipText != null)
        {
            loadingTipText.text = CurrentCompleteText;
        }
        
        // 短暂停留显示100%
        yield return new WaitForSeconds(0.3f);
    }
    
    /// <summary>
    /// 更新加载UI显示
    /// </summary>
    void UpdateLoadingUI(float progress)
    {
        // 更新进度条
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
        
        // 更新进度文本
        if (progressText != null)
        {
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
    }
    
    /// <summary>
    /// 更新动画效果
    /// </summary>
    void UpdateAnimations()
    {
        // 更新飞船位置和动画
        UpdateSpaceshipAnimation();
        
        // 更新星球旋转
        UpdatePlanetRotation();
    }
    
    /// <summary>
    /// 更新飞船动画
    /// </summary>
    void UpdateSpaceshipAnimation()
    {
        if (spaceship == null || startPoint == null || endPoint == null) return;
        
        // 根据进度计算飞船位置
        float curveValue = flightCurve.Evaluate(currentProgress);
        spaceshipBasePosition = Vector3.Lerp(startPoint.position, endPoint.position, curveValue);
        
        // 添加轻微的摇摆效果
        Vector3 bobOffset = Vector3.up * Mathf.Sin(Time.time * spaceshipBobSpeed) * spaceshipBobAmount;
        
        // 设置飞船最终位置
        spaceship.position = spaceshipBasePosition + bobOffset;
        
        // 可选：让飞船朝向移动方向
        // if (currentProgress > 0.01f && currentProgress < 0.99f)
        // {
        //     Vector3 direction = (endPoint.position - startPoint.position).normalized;
        //     if (direction != Vector3.zero)
        //     {
        //         spaceship.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        //     }
        // }
    }
    
    /// <summary>
    /// 更新星球旋转
    /// </summary>
    void UpdatePlanetRotation()
    {
        if (planet != null)
        {
            planet.Rotate(0, 0, planetRotationSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// 公开方法：立即完成加载（用于测试）
    /// </summary>
    [ContextMenu("Force Complete Loading")]
    public void ForceCompleteLoading()
    {
        if (isLoading)
        {
            StopAllCoroutines();
            currentProgress = 1f;
            UpdateLoadingUI(currentProgress);
            SceneManager.LoadScene(targetSceneName);
        }
    }
    
    /// <summary>
    /// 调试方法：显示当前语言设置和文本
    /// </summary>
    [ContextMenu("Debug Language Settings")]
    public void DebugLanguageSettings()
    {
        Debug.Log($"=== Loading Manager Language Debug ===");
        Debug.Log($"Current Language: {(SeedStatic.isEng ? "English" : "Chinese")}");
        Debug.Log($"Light Year Label: {(SeedStatic.isEng ? "Light Year" : "光年")}");
        Debug.Log($"Loading Text: {CurrentLoadingText}");
        Debug.Log($"Complete Text: {CurrentCompleteText}");
        
        Debug.Log($"General Tips Count: {CurrentGeneralTips.Length}");
        if (CurrentGeneralTips.Length > 0)
        {
            Debug.Log($"Sample General Tip: {CurrentGeneralTips[0]}");
        }
        
        Debug.Log($"Completion Tips Count: {CurrentCompletionTips.Length}");
        if (CurrentCompletionTips.Length > 0)
        {
            Debug.Log($"Sample Completion Tip: {CurrentCompletionTips[0]}");
        }
        
        Debug.Log($"Retry Tips Count: {CurrentRetryTips.Length}");
        if (CurrentRetryTips.Length > 0)
        {
            Debug.Log($"Sample Retry Tip: {CurrentRetryTips[0]}");
        }
    }
    
    /// <summary>
    /// 调试方法：切换语言（仅用于测试）
    /// </summary>
    [ContextMenu("Toggle Language (Test Only)")]
    public void ToggleLanguageForTest()
    {
        SeedStatic.isEng = !SeedStatic.isEng;
        Debug.Log($"Language switched to: {(SeedStatic.isEng ? "English" : "Chinese")}");
        
        // 如果正在加载，重新设置U
        if (isLoading)
        {
            SetupUI();
        }
    }
    
    /// <summary>
    /// 生成光年信息文本
    /// </summary>
    void GenerateLightYearText()
    {
        int currentLY = LoadingData.currentLightYear;
        int targetLY = LoadingData.targetLightYear;
        
        // 使用可配置的特殊阈值
        int actualMysteryThreshold = mysteryThreshold > 0 ? mysteryThreshold : LoadingData.mysteryLightYearThreshold;

        if (currentLY >= actualMysteryThreshold)
        {
            // 超过特殊阈值，显示神秘信息
            if (SeedStatic.isEng)
            {
                fullLightYearText = $"Traveled {currentLY} light years, ？？？？？？ light years remaining？？";
            }
            else
            {
                fullLightYearText = $"已经飞越了{currentLY}光年，还剩？？？？？？光年？？";
            }
        }
        else
        {
            // 正常显示
            int remaining = Mathf.Max(0, targetLY - currentLY);
            if (SeedStatic.isEng)
            {
                fullLightYearText = $"Traveled {currentLY} light years, {remaining} light years remaining";
            }
            else
            {
                fullLightYearText = $"已经飞越了{currentLY}光年，还剩{remaining}光年";
            }
        }
        
        Debug.Log($"Generated light year text: {fullLightYearText}");
    }
    
    /// <summary>
    /// 开始光年文本的逐字显示效果
    /// </summary>
    void StartLightYearTypingEffect()
    {
        if (!enableTypingEffect)
        {
            // 如果不启用逐字显示，直接显示完整文本
            if (lightYearText != null)
            {
                lightYearText.text = fullLightYearText;
            }
            return;
        }
        
        if (lightYearTypingCoroutine != null)
        {
            StopCoroutine(lightYearTypingCoroutine);
        }
        
        lightYearTypingCoroutine = StartCoroutine(TypeLightYearText());
    }
    
    /// <summary>
    /// 逐字显示光年文本的协程
    /// </summary>
    IEnumerator TypeLightYearText()
    {
        isTypingLightYear = true;
        lightYearText.text = "";
        
        for (int i = 0; i <= fullLightYearText.Length; i++)
        {
            if (lightYearText != null)
            {
                lightYearText.text = fullLightYearText.Substring(0, i);
            }
            
            yield return new WaitForSeconds(lightYearTypingSpeed);
        }
        
        isTypingLightYear = false;
        lightYearTypingCoroutine = null;
    }
    
    /// <summary>
    /// 立即完成光年文本显示（可用于跳过动画）
    /// </summary>
    public void CompleteLightYearTyping()
    {
        if (isTypingLightYear && lightYearTypingCoroutine != null)
        {
            StopCoroutine(lightYearTypingCoroutine);
            lightYearTypingCoroutine = null;
            isTypingLightYear = false;
            
            if (lightYearText != null)
            {
                lightYearText.text = fullLightYearText;
            }
        }
    }
    
    /// <summary>
    /// 调试方法：测试不同光年数的显示效果
    /// </summary>
    [ContextMenu("Test Light Year Display")]
    public void TestLightYearDisplay()
    {
        // 测试正常光年显示
        LoadingData.currentLightYear = 50;
        LoadingData.targetLightYear = 100;
        GenerateLightYearText();
        Debug.Log($"Normal display test: {fullLightYearText}");
        
        // 测试神秘光年显示
        LoadingData.currentLightYear = 120;
        LoadingData.targetLightYear = 100;
        GenerateLightYearText();
        Debug.Log($"Mystery display test: {fullLightYearText}");
        
        // 重新设置实际的加载数据（使用当前实际的光年数据）
        LoadingData.currentLightYear = SeedStatic.lightYear;
        LoadingData.targetLightYear = mysteryThreshold > 0 ? mysteryThreshold : 100;
        GenerateLightYearText();
        StartLightYearTypingEffect();
    }
    
    /// <summary>
    /// 调试方法：重新开始光年文本显示
    /// </summary>
    [ContextMenu("Restart Light Year Display")]
    public void RestartLightYearDisplay()
    {
        GenerateLightYearText();
        StartLightYearTypingEffect();
    }
    
    /// <summary>
    /// 调试方法：跳过光年打字效果
    /// </summary>
    [ContextMenu("Skip Light Year Typing")]
    public void SkipLightYearTyping()
    {
        CompleteLightYearTyping();
    }
}