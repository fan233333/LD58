using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskInfoDisplay : MonoBehaviour
{
    [Header("UI组件引用")]
    public TextMeshProUGUI taskInfoText; // 主要信息显示文本
    
    [Header("任务管理器引用")]
    public TaskManager taskManager;
    
    [Header("显示设置")]
    public bool showDetailedInfo = true; // 是否显示详细信息
    public bool showResourceScale = true; // 是否显示资源大小信息
    public bool showPercentage = true; // 是否显示百分比
    public bool showProgressBar = true; // 是否显示进度条
    public float updateInterval = 0.5f; // 更新间隔（秒）
    
    [Header("进度条设置")]
    [Range(10, 50)]
    public int progressBarLength = 20; // 进度条长度（字符数）
    public string progressBarFillChar = "█"; // 填充字符
    public string progressBarEmptyChar = "░"; // 空白字符
    
    [Header("颜色设置（富文本）")]
    public bool useRichText = true;
    public string completedColor = "#00FF00"; // 已完成颜色
    public string inProgressColor = "#FFFF00"; // 进行中颜色
    public string notStartedColor = "#FF6666"; // 未开始颜色
    public string timeWarningColor = "#FF0000"; // 时间警告颜色
    
    private float updateTimer = 0f;
    private int totalItemsRequired = 0;
    private int totalItemsCollected = 0;
    
    void Start()
    {
        // 如果没有指定TaskManager，尝试自动查找
        if (taskManager == null)
        {
            taskManager = FindObjectOfType<TaskManager>();
        }
        
        // 如果没有指定文本组件，尝试在当前对象上查找
        if (taskInfoText == null)
        {
            taskInfoText = GetComponent<TextMeshProUGUI>();
        }
        
        // 启用富文本
        if (taskInfoText != null && useRichText)
        {
            taskInfoText.richText = true;
        }
        
        // 立即更新一次
        UpdateTaskInfo();
    }
    
    void Update()
    {
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateTaskInfo();
        }
    }
    
    /// <summary>
    /// 更新任务信息显示
    /// </summary>
    void UpdateTaskInfo()
    {
        if (taskManager == null || taskInfoText == null)
            return;
        
        string infoText = "";
        
        // 添加任务标题
        infoText += GetTaskHeader();
        
        // 添加时间信息
        infoText += GetTimeInfo();
        
        // 添加总体进度
        infoText += GetOverallProgress();
        
        // 添加详细资源信息
        if (showDetailedInfo)
        {
            infoText += GetDetailedResourceInfo();
        }
        
        // 添加任务状态
        infoText += GetTaskStatus();
        
        // 更新文本显示
        taskInfoText.text = infoText;
    }
    
    /// <summary>
    /// 获取任务标题信息
    /// </summary>
    string GetTaskHeader()
    {
        string header = "";
        
        if (useRichText)
        {
            header += $"<size=120%><b>关卡 {SeedStatic.numScene} 任务进度</b></size>\n";
            header += $"<color=#888888>光年: {SeedStatic.lightYear:N0}</color>\n\n";
        }
        else
        {
            header += $"=== 关卡 {SeedStatic.numScene} 任务进度 ===\n";
            header += $"光年: {SeedStatic.lightYear:N0}\n\n";
        }
        
        return header;
    }
    
    /// <summary>
    /// 获取时间信息
    /// </summary>
    string GetTimeInfo()
    {
        float remainTime = taskManager.GetRemainTime();
        int minutes = Mathf.FloorToInt(remainTime / 60f);
        int seconds = Mathf.FloorToInt(remainTime % 60f);
        
        string timeText = "";
        string timeColor = remainTime < 30f ? timeWarningColor : "#FFFFFF";
        
        if (useRichText)
        {
            timeText += $"⏰ 剩余时间: <color={timeColor}><b>{minutes:00}:{seconds:00}</b></color>\n\n";
        }
        else
        {
            timeText += $"剩余时间: {minutes:00}:{seconds:00}\n\n";
        }
        
        return timeText;
    }
    
    /// <summary>
    /// 获取总体进度信息
    /// </summary>
    string GetOverallProgress()
    {
        // 计算总体进度
        CalculateTotalProgress();
        
        float overallPercentage = totalItemsRequired > 0 ? (float)totalItemsCollected / totalItemsRequired * 100f : 0f;
        
        string progressText = "";
        
        if (useRichText)
        {
            string progressColor = overallPercentage >= 100f ? completedColor : 
                                 overallPercentage > 0f ? inProgressColor : notStartedColor;
            
            progressText += $"📊 <b>总体进度:</b> <color={progressColor}>{totalItemsCollected}/{totalItemsRequired}</color>";
            
            if (showPercentage)
            {
                progressText += $" <color={progressColor}>({overallPercentage:F1}%)</color>";
            }
            
            progressText += "\n";
            
            if (showProgressBar)
            {
                progressText += GetProgressBar(overallPercentage) + "\n";
            }
        }
        else
        {
            progressText += $"总体进度: {totalItemsCollected}/{totalItemsRequired}";
            
            if (showPercentage)
            {
                progressText += $" ({overallPercentage:F1}%)";
            }
            
            progressText += "\n";
            
            if (showProgressBar)
            {
                progressText += GetProgressBar(overallPercentage, false) + "\n";
            }
        }
        
        progressText += "\n";
        return progressText;
    }
    
    /// <summary>
    /// 获取详细资源信息
    /// </summary>
    string GetDetailedResourceInfo()
    {
        string detailText = "";
        
        if (useRichText)
        {
            detailText += "<b>📦 资源收集详情:</b>\n";
        }
        else
        {
            detailText += "=== 资源收集详情 ===\n";
        }
        
        foreach (var taskItem in taskManager.taskItems)
        {
            float percentage = taskItem.requiredAmount > 0 ? 
                              (float)taskItem.currentAmount / taskItem.requiredAmount * 100f : 0f;
            
            string itemInfo = "";
            
            // 资源名称和进度
            if (useRichText)
            {
                string statusColor = percentage >= 100f ? completedColor :
                                   percentage > 0f ? inProgressColor : notStartedColor;
                
                itemInfo += $"• <color={statusColor}><b>{taskItem.itemName}</b></color>: ";
                itemInfo += $"<color={statusColor}>{taskItem.currentAmount}/{taskItem.requiredAmount}</color>";
                
                if (showPercentage)
                {
                    itemInfo += $" <color={statusColor}>({percentage:F1}%)</color>";
                }
                
                // 显示资源大小信息
                if (showResourceScale)
                {
                    float scale = taskManager.GetItemScale(taskItem.itemName);
                    string scaleColor = scale > 1f ? "#00CCFF" : scale < 1f ? "#FFAA00" : "#CCCCCC";
                    itemInfo += $" <size=80%><color={scaleColor}>[大小: {scale:F2}x]</color></size>";
                }
            }
            else
            {
                itemInfo += $"• {taskItem.itemName}: {taskItem.currentAmount}/{taskItem.requiredAmount}";
                
                if (showPercentage)
                {
                    itemInfo += $" ({percentage:F1}%)";
                }
                
                if (showResourceScale)
                {
                    float scale = taskManager.GetItemScale(taskItem.itemName);
                    itemInfo += $" [大小: {scale:F2}x]";
                }
            }
            
            itemInfo += "\n";
            
            // 添加小进度条
            if (showProgressBar)
            {
                itemInfo += "  " + GetProgressBar(percentage, useRichText, 15) + "\n";
            }
            
            detailText += itemInfo;
        }
        
        return detailText + "\n";
    }
    
    /// <summary>
    /// 获取任务状态信息
    /// </summary>
    string GetTaskStatus()
    {
        string statusText = "";
        
        if (taskManager.IsAllItemsCollected())
        {
            if (useRichText)
            {
                statusText += $"<color={completedColor}><b>✅ 任务已完成！</b></color>\n";
            }
            else
            {
                statusText += "✅ 任务已完成！\n";
            }
        }
        else
        {
            float remainTime = taskManager.GetRemainTime();
            
            if (remainTime <= 0f)
            {
                if (useRichText)
                {
                    statusText += $"<color={timeWarningColor}><b>❌ 任务失败！</b></color>\n";
                }
                else
                {
                    statusText += "❌ 任务失败！\n";
                }
            }
            else
            {
                if (useRichText)
                {
                    statusText += $"<color={inProgressColor}><b>🔄 任务进行中...</b></color>\n";
                }
                else
                {
                    statusText += "🔄 任务进行中...\n";
                }
            }
        }
        
        return statusText;
    }
    
    /// <summary>
    /// 生成进度条
    /// </summary>
    string GetProgressBar(float percentage, bool useColor = true, int length = -1)
    {
        if (length <= 0) length = progressBarLength;
        
        int filledCount = Mathf.RoundToInt(percentage / 100f * length);
        filledCount = Mathf.Clamp(filledCount, 0, length);
        int emptyCount = length - filledCount;
        
        string progressBar = "";
        
        if (useColor && useRichText)
        {
            string barColor = percentage >= 100f ? completedColor :
                            percentage > 0f ? inProgressColor : notStartedColor;
            
            progressBar += $"<color={barColor}>";
        }
        
        progressBar += "[";
        progressBar += new string(progressBarFillChar[0], filledCount);
        progressBar += new string(progressBarEmptyChar[0], emptyCount);
        progressBar += "]";
        
        if (useColor && useRichText)
        {
            progressBar += "</color>";
        }
        
        return progressBar;
    }
    
    /// <summary>
    /// 计算总体进度
    /// </summary>
    void CalculateTotalProgress()
    {
        totalItemsRequired = 0;
        totalItemsCollected = 0;
        
        foreach (var taskItem in taskManager.taskItems)
        {
            totalItemsRequired += taskItem.requiredAmount;
            totalItemsCollected += Mathf.Min(taskItem.currentAmount, taskItem.requiredAmount);
        }
    }
    
    /// <summary>
    /// 手动刷新显示（供外部调用）
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateTaskInfo();
    }
    
    /// <summary>
    /// 设置更新间隔
    /// </summary>
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.1f, interval);
    }
}