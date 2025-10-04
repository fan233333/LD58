using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemStatistics : MonoBehaviour
{
    [System.Serializable]
    public class ItemCollectionEvent : UnityEvent<string, int> { }

    [Header("ͳ����ʾ")]
    public bool showDebugLog = true; // �Ƿ��ڿ���̨��ʾͳ����Ϣ

    [Header("�¼�")]
    public ItemCollectionEvent onItemCollected; // ��Ʒ�ռ��¼�
    public UnityEvent onStatisticsUpdated; // ͳ�Ƹ����¼�

    // ��Ʒͳ���ֵ�
    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    // ���ռ���Ʒ����
    private int totalItemsCollected = 0;

    // ����ģʽ
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
        // ȷ��ֻ��һ��ʵ��
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject); // �糡��������
    }

    // ��¼�ռ�����Ʒ
    public void RecordItem(CollectibleItem item)
    {
        if (item == null) return;

        string typeKey = item.GetTypeKey();
        int value = item.value;

        // ����ͳ��
        if (itemCounts.ContainsKey(typeKey))
        {
            itemCounts[typeKey] += value;
        }
        else
        {
            itemCounts[typeKey] = value;
        }

        totalItemsCollected += value;

        // �����¼�
        onItemCollected?.Invoke(typeKey, value);
        onStatisticsUpdated?.Invoke();

        // ������Ϣ
        if (showDebugLog)
        {
            Debug.Log($"�ռ���Ʒ: {item.GetTypeName()} (��ֵ: {value})");
            Debug.Log($"��ǰ��Ʒ����: {GetItemCount(item.GetTypeKey())}");
            Debug.Log($"��ǰͳ��: {GetStatisticsString()}");
        }
    }

    // ��ȡ�ض�������Ʒ������
    public int GetItemCount(string typeKey)
    {
        return itemCounts.ContainsKey(typeKey) ? itemCounts[typeKey] : 0;
    }

    // ��ȡ�ض�������Ʒ��������ʹ��ö�٣�
    public int GetItemCount(CollectibleItem.ItemType itemType)
    {
        return GetItemCount(itemType.ToString());
    }

    // ��ȡ����Ʒ����
    public int GetTotalItemsCollected()
    {
        return totalItemsCollected;
    }

    // ��ȡ�������͵�ͳ��
    public Dictionary<string, int> GetAllItemCounts()
    {
        return new Dictionary<string, int>(itemCounts);
    }

    // ��ȡ��ʽ����ͳ���ַ���
    public string GetStatisticsString()
    {
        if (itemCounts.Count == 0)
            return "��δ�ռ��κ���Ʒ";

        string result = $"�ܹ��ռ��� {totalItemsCollected} ����Ʒ:\n";

        foreach (var pair in itemCounts)
        {
            string typeName = GetTypeDisplayName(pair.Key);
            result += $"- {typeName}: {pair.Value}��\n";
        }

        return result;
    }

    // ��ȡ������ʾ����
    private string GetTypeDisplayName(string typeKey)
    {
        switch (typeKey)
        {
            case "Circle": return "Բ��";
            case "Triangle": return "������";
            case "Square": return "������";
            case "Diamond": return "����";
            case "Star": return "����";
            case "Hexagon": return "������";
            default: return typeKey;
        }
    }

    // ����ͳ��
    public void ResetStatistics()
    {
        itemCounts.Clear();
        totalItemsCollected = 0;
        onStatisticsUpdated?.Invoke();

        if (showDebugLog)
            Debug.Log("��Ʒͳ��������");
    }

    // ����ͳ�ƣ�PlayerPrefs��
    public void SaveStatistics()
    {
        foreach (var pair in itemCounts)
        {
            PlayerPrefs.SetInt($"ItemCount_{pair.Key}", pair.Value);
        }
        PlayerPrefs.SetInt("TotalItemsCollected", totalItemsCollected);
        PlayerPrefs.Save();

        if (showDebugLog)
            Debug.Log("��Ʒͳ���ѱ���");
    }

    // ����ͳ�ƣ�PlayerPrefs��
    public void LoadStatistics()
    {
        ResetStatistics();

        // ����������
        totalItemsCollected = PlayerPrefs.GetInt("TotalItemsCollected", 0);

        // ���ظ���������
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
            Debug.Log($"��Ʒͳ���Ѽ���: {GetStatisticsString()}");
    }
}