using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class TaskItem
{
    public string itemName;
    public int requiredAmount;
    public int maxNumber;
    public Transform Container;
    [HideInInspector] public int currentAmount;
}

public class TaskManager : MonoBehaviour
{
    [Header("任务设置")]
    public List<TaskItem> taskItems = new List<TaskItem>();
    public float timeLimit = 60f; // 任务时间限制（秒）

    [Header("UI引用")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI numSceneText;
    public GameObject successPanel;
    public GameObject failPanel;
    public string nextSceneName;

    [Header("容器生成位置")]
    public List<Transform> transformList = new List<Transform>();

    [Header("容器管理")]
    public ContainerManager containerManager;

    public int minYear;
    public int maxYear;

    public GameObject img1;
    public GameObject img2;
    public float transitionDuration = 10f;
    public int highLightYear = 100;


    private float currentTime;
    private bool isTaskActive = false;
    private int totalItemsCollected = 0;
    private int totalItemsRequired = 0;
    private bool isImg2 = false;

    void Start()
    {
        taskItems = GetComponent<TaskGenerator>().GenerateAndAssign();
        StartTask();
        
    }

    void Update()
    {
        AudioManager.isActive = isTaskActive;
        if (isTaskActive && !ManageScenes.Instance.IsStoryPlaying)
        {
            UpdateTimer();
            //Debug.Log(SeedStatic.lightYear);
            //Debug.Log(highLightYear);
            
        }
        if (!isTaskActive)
        {
            if (SeedStatic.lightYear >= highLightYear)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    SetObjectAlpha(img1, 1f);
                    SetObjectAlpha(img2, 0f);
                    img2.SetActive(true);
                    StartCoroutine(TransitionCoroutine());
                }
                if (isImg2)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        SceneManager.LoadScene(0);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            TaskCompleted();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            currentTime = 0;
        }
    }
    IEnumerator TransitionCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // 淡出A，淡入B
            SetObjectAlpha(img1, 1f - t);
            SetObjectAlpha(img2, t);

            yield return null;
        }

        // 确保最终状态正确
        SetObjectAlpha(img1, 0f);
        SetObjectAlpha(img2, 1f);
        isImg2 = true;
    }

    void SetObjectAlpha(GameObject obj, float alpha)
    {
        // 获取所有Renderer组件并设置透明度
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = renderer.material;
            Color color = material.color;
            color.a = alpha;
            material.color = color;

            // 根据透明度设置渲染模式
            if (alpha < 1f)
            {
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
            else
            {
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
            }
        }
    }

    public void StartTask()
    {
        // 计算需要的总物品数量
        totalItemsRequired = 0;
        int index = 0;
        foreach (var item in taskItems)
        {
            totalItemsRequired += item.requiredAmount;
            item.currentAmount = 0; // 重置当前数量
            //float scale = (float)item.requiredAmount / item.maxNumber;
            //Vector3 newScale = new Vector3(scale, scale, 1f);
            //ScaleContainer(item.Container, newScale);
            item.Container.position = transformList[index].position;
            index++;
        }

        currentTime = timeLimit;
        isTaskActive = true;
        totalItemsCollected = 0;

        UpdateUI();
        numSceneText.text = $"{SeedStatic.numScene}";

        // 隐藏结果面板
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
        // 更新计时器显示
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // 时间紧张时变红色
            if (currentTime < 30f)
            {
                timerText.color = Color.red;
            }
        }

        // 更新物品计数显示
        if (itemCountText)
        {
            itemCountText.text = $"{totalItemsCollected}/{totalItemsRequired}";
        }
    }

    public void ItemCollected(GameObject obj, string itemType)
    {
        if (!isTaskActive) return;

        totalItemsCollected = 0;
        // 更新特定物品的收集计数
        foreach (var item in taskItems)
        {
            if (item.itemName == itemType)
            {
                item.currentAmount++;
                Debug.Log($"itemType:{itemType},{item.currentAmount},{item.requiredAmount}");
                if(item.currentAmount <= item.requiredAmount)
                {
                    Debug.Log($"container Create{itemType}");
                    containerManager.CreateObject(obj, itemType);
                }
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

        // 检查任务是否完成
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
        Debug.Log("任务完成！");

        if (successPanel)
        {
            successPanel.SetActive(true);
        }

        SeedStatic.numScene++;
        SeedStatic.lightYear += Random.Range(minYear, maxYear);


        // 延迟切换到下一个场景
        StartCoroutine(LoadNextSceneAfterDelay(2f));
    }

    void TaskFailed()
    {
        isTaskActive = false;
        Debug.Log("任务失败！");
        //SeedStatic.tileSeed = Random.Range(1, 10000);
        //SeedStatic.objectSeed = Random.Range(1, 10000);
        //SeedStatic.lightYear += Random.Range(minYear, maxYear);
        //Debug.Log(SeedStatic.tileSeed);
        //Debug.Log(SeedStatic.objectSeed);
        //SceneManager.LoadScene(nextSceneName);

        if(SeedStatic.lightYear < highLightYear)
        {
            if (failPanel)
            {
                failPanel.SetActive(true);
            }
        }
        else
        {
            img1.SetActive(true);
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

    // 手动重新开始任务（用于UI按钮）
    public void RestartTask()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 检查是否所有物品都收集完成
    public bool IsAllItemsCollected()
    {
        foreach (var item in taskItems)
        {
            if (item.currentAmount < item.requiredAmount)
                return false;
        }
        return true;
    }

    // 获取特定物品的收集进度
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

    public float GetRemainTime()
    {
        return currentTime;
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
    //    // 应用缩放
    //    obj.transform.localScale = new Vector3(scale, scale, 1f);

    //}

    //public void ScaleContainer(GameObject obj, Vector3 newScale)
    //{
    //    // 记录缩放前的世界位置
    //    Vector3 originalWorldPosition = obj.transform.position;
    //    Debug.Log(originalWorldPosition);

    //    // 应用新缩放
    //    obj.transform.localScale = newScale;
    //    Debug.Log(obj.transform.localScale);

    //    // 恢复世界位置
    //    obj.transform.position = originalWorldPosition;
    //    Debug.Log(obj.transform.position);
    //}

}