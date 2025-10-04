using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StatisticsUI : MonoBehaviour
{
    [Header("UI组件")]
    public Text statisticsText; // 统计文本显示
    public GameObject itemCountPrefab; // 物品计数UI预制体
    public Transform itemCountContainer; // 物品计数容器

    [Header("显示设置")]
    public bool autoUpdate = true; // 自动更新显示
    public float updateInterval = 0.5f; // 更新间隔

    private Dictionary<string, Text> itemCountTexts = new Dictionary<string, Text>();
    private float updateTimer = 0f;

    void Start()
    {
        // 注册统计更新事件
        ItemStatistics.Instance.onStatisticsUpdated.AddListener(UpdateUI);

        // 初始化UI
        InitializeUI();
        UpdateUI();
    }

    void Update()
    {
        if (autoUpdate)
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateUI();
            }
        }
    }

    void InitializeUI()
    {
        // 如果使用预制体方式，初始化各类型物品的计数显示
        if (itemCountPrefab != null && itemCountContainer != null)
        {
            // 清空容器
            foreach (Transform child in itemCountContainer)
            {
                Destroy(child.gameObject);
            }
            itemCountTexts.Clear();

            // 创建各类型物品的计数显示
            string[] types = { "圆形", "三角形", "正方形", "菱形", "星星", "六边形" };
            string[] typeKeys = { "Circle", "Triangle", "Square", "Diamond", "Star", "Hexagon" };

            for (int i = 0; i < types.Length; i++)
            {
                GameObject itemUI = Instantiate(itemCountPrefab, itemCountContainer);
                Text itemText = itemUI.GetComponentInChildren<Text>();

                if (itemText != null)
                {
                    itemCountTexts[typeKeys[i]] = itemText;
                    itemText.text = $"{types[i]}: 0";
                }
            }
        }
    }

    void UpdateUI()
    {
        // 更新统计文本
        if (statisticsText != null)
        {
            statisticsText.text = ItemStatistics.Instance.GetStatisticsString();
        }

        // 更新各类型物品计数
        foreach (var pair in itemCountTexts)
        {
            int count = ItemStatistics.Instance.GetItemCount(pair.Key);
            string typeName = GetTypeDisplayName(pair.Key);
            pair.Value.text = $"{typeName}: {count}";
        }
    }

    string GetTypeDisplayName(string typeKey)
    {
        switch (typeKey)
        {
            case "Circle": return "圆形";
            case "Triangle": return "三角形";
            case "Square": return "正方形";
            case "Diamond": return "菱形";
            case "Star": return "星星";
            case "Hexagon": return "六边形";
            default: return typeKey;
        }
    }

    // 手动刷新UI
    public void RefreshUI()
    {
        UpdateUI();
    }

    void OnDestroy()
    {
        // 取消事件注册
        if (ItemStatistics.Instance != null)
        {
            ItemStatistics.Instance.onStatisticsUpdated.RemoveListener(UpdateUI);
        }
    }
}