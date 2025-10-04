using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemStatistics : MonoBehaviour
{
    [System.Serializable]
    public class ItemCollectionEvent : UnityEvent<string, int> { }

    [Header("统计显示")]
    public bool showDebugLog = true; // 是否在控制台显示统计信息

    [Header("事件")]
    public ItemCollectionEvent onItemCollected; // 物品收集事件
    public UnityEvent onStatisticsUpdated; // 统计更新事件

    // 物品统计字典
    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    // 总收集物品数量
    private int totalItemsCollected = 0;

    // 单例模式
    private static ItemStatistics _instance;
    public static ItemStatistics Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ItemStatistics>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ItemStatistics");
                    _instance = obj.AddComponent<ItemStatistics>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        // 确保只有一个实例
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject); // 跨场景不销毁
    }

    // 记录收集的物品
    public void RecordItem(CollectibleItem item)
    {
        if (item == null) return;

        string typeKey = item.GetTypeKey();
        int value = item.value;

        // 更新统计
        if (itemCounts.ContainsKey(typeKey))
        {
            itemCounts[typeKey] += value;
        }
        else
        {
            itemCounts[typeKey] = value;
        }

        totalItemsCollected += value;

        // 触发事件
        onItemCollected?.Invoke(typeKey, value);
        onStatisticsUpdated?.Invoke();

        // 调试信息
        if (showDebugLog)
        {
            Debug.Log($"收集物品: {item.GetTypeName()} (价值: {value})");
            Debug.Log($"当前物品数量: {GetItemCount(item.GetTypeKey())}");
            Debug.Log($"当前统计: {GetStatisticsString()}");
        }
    }

    // 获取特定类型物品的数量
    public int GetItemCount(string typeKey)
    {
        return itemCounts.ContainsKey(typeKey) ? itemCounts[typeKey] : 0;
    }

    // 获取特定类型物品的数量（使用枚举）
    public int GetItemCount(CollectibleItem.ItemType itemType)
    {
        return GetItemCount(itemType.ToString());
    }

    // 获取总物品数量
    public int GetTotalItemsCollected()
    {
        return totalItemsCollected;
    }

    // 获取所有类型的统计
    public Dictionary<string, int> GetAllItemCounts()
    {
        return new Dictionary<string, int>(itemCounts);
    }

    // 获取格式化的统计字符串
    public string GetStatisticsString()
    {
        if (itemCounts.Count == 0)
            return "尚未收集任何物品";

        string result = $"总共收集了 {totalItemsCollected} 个物品:\n";

        foreach (var pair in itemCounts)
        {
            string typeName = GetTypeDisplayName(pair.Key);
            result += $"- {typeName}: {pair.Value}个\n";
        }

        return result;
    }

    // 获取类型显示名称
    private string GetTypeDisplayName(string typeKey)
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

    // 重置统计
    public void ResetStatistics()
    {
        itemCounts.Clear();
        totalItemsCollected = 0;
        onStatisticsUpdated?.Invoke();

        if (showDebugLog)
            Debug.Log("物品统计已重置");
    }

    // 保存统计（PlayerPrefs）
    public void SaveStatistics()
    {
        foreach (var pair in itemCounts)
        {
            PlayerPrefs.SetInt($"ItemCount_{pair.Key}", pair.Value);
        }
        PlayerPrefs.SetInt("TotalItemsCollected", totalItemsCollected);
        PlayerPrefs.Save();

        if (showDebugLog)
            Debug.Log("物品统计已保存");
    }

    // 加载统计（PlayerPrefs）
    public void LoadStatistics()
    {
        ResetStatistics();

        // 加载总数量
        totalItemsCollected = PlayerPrefs.GetInt("TotalItemsCollected", 0);

        // 加载各类型数量
        string[] types = { "Circle", "Triangle", "Square", "Diamond", "Star", "Hexagon" };
        foreach (string type in types)
        {
            int count = PlayerPrefs.GetInt($"ItemCount_{type}", 0);
            if (count > 0)
            {
                itemCounts[type] = count;
            }
        }

        onStatisticsUpdated?.Invoke();

        if (showDebugLog)
            Debug.Log($"物品统计已加载: {GetStatisticsString()}");
    }
}