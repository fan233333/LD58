using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class TaskItem
{
    public string itemName;
    public int requiredAmount;
    public int maxNumber;
    [HideInInspector] public int currentAmount;
}

public class TaskManager : MonoBehaviour
{
    [Header("��������")]
    public List<TaskItem> taskItems = new List<TaskItem>();
    public float timeLimit = 60f; // ����ʱ�����ƣ��룩

    [Header("UI����")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI numSceneText;
    public GameObject successPanel;
    public GameObject failPanel;
    public string nextSceneName;

    private float currentTime;
    private bool isTaskActive = false;
    private int totalItemsCollected = 0;
    private int totalItemsRequired = 0;

    void Start()
    {
        StartTask();
        
    }

    void Update()
    {
        if (isTaskActive)
        {
            UpdateTimer();
        }
    }

    public void StartTask()
    {
        // ������Ҫ������Ʒ����
        totalItemsRequired = 0;
        foreach (var item in taskItems)
        {
            totalItemsRequired += item.requiredAmount;
            item.currentAmount = 0; // ���õ�ǰ����
            //float scale = (float)item.requiredAmount / item.maxNumber;
            //Vector3 newScale = new Vector3(scale, scale, 1f);
            //ScaleContainer(item.Container, newScale);
        }

        currentTime = timeLimit;
        isTaskActive = true;
        totalItemsCollected = 0;

        UpdateUI();
        numSceneText.text = $"{SeedStatic.numScene}";

        // ���ؽ�����
        if (successPanel) successPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);
    }

    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            TaskFailed();
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        // ���¼�ʱ����ʾ
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // ʱ�����ʱ���ɫ
            if (currentTime < 30f)
            {
                timerText.color = Color.red;
            }
        }

        // ������Ʒ������ʾ
        if (itemCountText)
        {
            itemCountText.text = $"{totalItemsCollected}/{totalItemsRequired}";
        }
    }

    public void ItemCollected(string itemType)
    {
        if (!isTaskActive) return;

        totalItemsCollected = 0;
        // �����ض���Ʒ���ռ�����
        foreach (var item in taskItems)
        {
            if (item.itemName == itemType)
            {
                item.currentAmount++;
            }
            if(item.requiredAmount >= item.currentAmount)
            {
                totalItemsCollected += item.currentAmount;
            }
            else
            {
                totalItemsCollected += item.requiredAmount;
            }
                
        }

        UpdateUI();

        // ��������Ƿ����
        bool completed = true;
        foreach (var item in taskItems)
        {
            if (item.currentAmount < item.requiredAmount)
            {
                completed = false;
            }
        }

        if (completed)
        {
            TaskCompleted();
        }
    }

    void TaskCompleted()
    {
        isTaskActive = false;
        Debug.Log("������ɣ�");

        if (successPanel)
        {
            successPanel.SetActive(true);
        }

        SeedStatic.numScene++;

        // �ӳ��л�����һ������
        StartCoroutine(LoadNextSceneAfterDelay(2f));
    }

    void TaskFailed()
    {
        isTaskActive = false;
        Debug.Log("����ʧ�ܣ�");
        SeedStatic.tileSeed = Random.Range(1, 10000);
        SeedStatic.objectSeed = Random.Range(1, 10000);
        Debug.Log(SeedStatic.tileSeed);
        Debug.Log(SeedStatic.objectSeed);
        SceneManager.LoadScene(nextSceneName);

        if (failPanel)
        {
            failPanel.SetActive(true);
        }
    }

    IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SeedStatic.tileSeed = Random.Range(1, 10000);
            SeedStatic.objectSeed = Random.Range(1, 10000);
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // �ֶ����¿�ʼ��������UI��ť��
    public void RestartTask()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ����Ƿ�������Ʒ���ռ����
    public bool IsAllItemsCollected()
    {
        foreach (var item in taskItems)
        {
            if (item.currentAmount < item.requiredAmount)
                return false;
        }
        return true;
    }

    // ��ȡ�ض���Ʒ���ռ�����
    public string GetItemProgress(string itemName)
    {
        foreach (var item in taskItems)
        {
            if (item.itemName == itemName)
            {
                return $"{item.currentAmount}/{item.requiredAmount}";
            }
        }
        return "0/0";
    }

    public float GetItemScale(string itemName)
    {
        foreach(var item in taskItems)
        {
            if(item.itemName == itemName)
            {
                return Mathf.Sqrt((float)item.maxNumber / item.requiredAmount);
            }
        }
        return 1f;
    }

    //public void ScaleWithFixedBottom(GameObject obj, int n, int maxNumber)
    //{
    //    Debug.Log($"Scale:{n}");
    //    Debug.Log($"Scale:{maxNumber}");
    //    float scale = 1.0f * n / maxNumber;
    //    Debug.Log($"Scale:{scale}");
    //    // Ӧ������
    //    obj.transform.localScale = new Vector3(scale, scale, 1f);

    //}

    //public void ScaleContainer(GameObject obj, Vector3 newScale)
    //{
    //    // ��¼����ǰ������λ��
    //    Vector3 originalWorldPosition = obj.transform.position;
    //    Debug.Log(originalWorldPosition);

    //    // Ӧ��������
    //    obj.transform.localScale = newScale;
    //    Debug.Log(obj.transform.localScale);

    //    // �ָ�����λ��
    //    obj.transform.position = originalWorldPosition;
    //    Debug.Log(obj.transform.position);
    //}

}