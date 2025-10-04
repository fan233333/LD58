using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StatisticsUI : MonoBehaviour
{
    [Header("UI���")]
    public Text statisticsText; // ͳ���ı���ʾ
    public GameObject itemCountPrefab; // ��Ʒ����UIԤ����
    public Transform itemCountContainer; // ��Ʒ��������

    [Header("��ʾ����")]
    public bool autoUpdate = true; // �Զ�������ʾ
    public float updateInterval = 0.5f; // ���¼��

    private Dictionary<string, Text> itemCountTexts = new Dictionary<string, Text>();
    private float updateTimer = 0f;

    void Start()
    {
        // ע��ͳ�Ƹ����¼�
        ItemStatistics.Instance.onStatisticsUpdated.AddListener(UpdateUI);

        // ��ʼ��UI
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
        // ���ʹ��Ԥ���巽ʽ����ʼ����������Ʒ�ļ�����ʾ
        if (itemCountPrefab != null && itemCountContainer != null)
        {
            // �������
            foreach (Transform child in itemCountContainer)
            {
                Destroy(child.gameObject);
            }
            itemCountTexts.Clear();

            // ������������Ʒ�ļ�����ʾ
            string[] types = { "Բ��", "������", "������", "����", "����", "������" };
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
        // ����ͳ���ı�
        if (statisticsText != null)
        {
            statisticsText.text = ItemStatistics.Instance.GetStatisticsString();
        }

        // ���¸�������Ʒ����
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
            case "Circle": return "Բ��";
            case "Triangle": return "������";
            case "Square": return "������";
            case "Diamond": return "����";
            case "Star": return "����";
            case "Hexagon": return "������";
            default: return typeKey;
        }
    }

    // �ֶ�ˢ��UI
    public void RefreshUI()
    {
        UpdateUI();
    }

    void OnDestroy()
    {
        // ȡ���¼�ע��
        if (ItemStatistics.Instance != null)
        {
            ItemStatistics.Instance.onStatisticsUpdated.RemoveListener(UpdateUI);
        }
    }
}