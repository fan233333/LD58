using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Phosphorescence.Narration;

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
    [Header("��������")]
    public List<TaskItem> taskItems = new List<TaskItem>();
    public float timeLimit = 60f; // ����ʱ�����ƣ��룩

    [Header("其他根据任务状态调整的物体")]
    public GameObject[] Bottles;
    [Header("图标设置")]
    public List<GameObject> defaultIcons;      // 未完成时的图标
    public List<GameObject> completedIcons;    // 完成时的图标


    [Header("UI����")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI numSceneText;
    public TextMeshProUGUI instructionText;  // 收集资源指令文本
    public GameObject successPanel;
    public GameObject failPanel;
    public string nextSceneName;

    [Header("��������λ��")]
    public List<Transform> transformList = new List<Transform>();

    [Header("��������")]
    public ContainerManager containerManager;

    public int minYear;
    public int maxYear;

    public Image img1;
    public Image img2;
    private float transitionDuration = 3f;
    public int highLightYear = 100;

    [Header("结束对话设置")]
    [Tooltip("中文结束对话")]
    public TextAsset endStoryChineseCN;
    [Tooltip("英文结束对话")]
    public TextAsset endStoryChineseEN;
    
    // 当前对话资源
    public TextAsset CurrentEndStory => SeedStatic.isEng ? endStoryChineseEN : endStoryChineseCN;


    private float currentTime;
    private bool isTaskActive = false;
    private int totalItemsCollected = 0;
    private int totalItemsRequired = 0;
    private bool isImg2 = false;
    private Coroutine currentTransition;
    
    // 对话相关状态
    private bool isEndStoryPlaying = false;
    private bool hasEndStoryPlayed = false;
    private InkReader _inkReader;

    void Start()
    {
        
        foreach(var bottle in Bottles)
        {
            bottle.SetActive(false);
        }
        taskItems = GetComponent<TaskGenerator>().GenerateAndAssign();
        StartTask();
        
        // 初始化InkReader
        FindAndSetupInkReader();
        
    }

    void Update()
    {
        AudioManager.isActive = isTaskActive;
        if (isTaskActive && !ManageScenes.Instance.IsStoryPlaying && !IsEndStoryPlaying)
        {
            UpdateTimer();
            //Debug.Log(SeedStatic.lightYear);
            //Debug.Log(highLightYear);
            
        }
        if (!isTaskActive)
        {
            if (SeedStatic.lightYear >= highLightYear)
            {
                // 持续检查对话是否结束
                CheckStoryCompletion();
                
                if (Input.GetMouseButtonDown(0) && !isImg2)
                {
                    // 播放结束对话（图片会在对话中被隐藏）
                    PlayEndStory();
                }
                if (isImg2)
                {
                    // 只有当对话不在播放时才允许点击返回主菜单
                    if (Input.GetMouseButtonDown(0) && !IsEndStoryPlaying)
                    {
                        // 使用加载界面返回主菜单
                        LoadingData.SetLoadingData("MainMenu", 0, 0, false, false, highLightYear);
                        SceneManager.LoadScene("LoadingScene");
                    }
                }
            }
        }



        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(TaskCompleted());
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            currentTime = 0;
        }
    }

        /// <summary>
    /// 获取任务是否完成
    /// </summary>
    /// <returns>任务完成状态</returns>
    public bool IsTaskCompleted()
    {
        if (!isTaskActive && IsAllItemsCollected())
        {
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 获取任务是否失败
    /// </summary>
    /// <returns>任务失败状态</returns>
    public bool IsTaskFailed()
    {
        return !isTaskActive && !IsAllItemsCollected();
    }
    
    /// <summary>
    /// 获取任务是否激活
    /// </summary>
    /// <returns>任务激活状态</returns>
    public bool IsTaskActive()
    {
        return isTaskActive;
    }
    IEnumerator FadeTransition()
    {
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // ����͸����
            float alphaA = 1f - t;
            //float alphaB = t;

            // Ӧ��͸����
            SetImageAlpha(img1, alphaA);
            //SetImageAlpha(img2, alphaB);

            yield return null;
        }

        // ȷ������״̬��ȷ
        SetImageAlpha(img1, 0f);
        SetImageAlpha(img2, 1f);
        isImg2 = true;
    }

    void SetImageAlpha(Image image, float alpha)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    public void StartTask()
    {
        // ������Ҫ������Ʒ����
        totalItemsRequired = 0;
        int index = 0;
        foreach (var item in taskItems)
        {
            totalItemsRequired += item.requiredAmount;
            item.currentAmount = 0; // ���õ�ǰ����
            //float scale = (float)item.requiredAmount / item.maxNumber;
            //Vector3 newScale = new Vector3(scale, scale, 1f);
            //ScaleContainer(item.Container, newScale);
            item.Container.position = transformList[index].position;
            Bottles[index].SetActive(true);
            Debug.Log(Bottles[index].name + " active");
            Debug.Log($"{item.Container.name},{item.itemName}");
            defaultIcons.Add(item.Container.GetChild(1).GetChild(0).gameObject);
            completedIcons.Add(item.Container.GetChild(1).GetChild(1).gameObject);
            index++;

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

        // 收集资源指令文本
        if (instructionText)
        {
            instructionText.text = SeedStatic.isEng ? "Collect resources to fly to the next planet" : "收集资源以飞往下一个星球";
        }
    }

    public bool CheckItemFull(string itemType)
    {
        foreach(var item in taskItems)
        {
            if(item.itemName == itemType)
            {
                if(item.currentAmount >= item.requiredAmount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }

    public void ItemCollected(GameObject obj, string itemType)
    {
        if (!isTaskActive) return;
        totalItemsCollected = 0;
        // �����ض���Ʒ���ռ�����
        foreach (var item in taskItems)
        {
            if (item.itemName == itemType)
            {
                item.currentAmount++;
                Debug.Log($"itemType:{itemType},{item.currentAmount},{item.requiredAmount}");
                if (item.currentAmount <= item.requiredAmount)
                {
                    Debug.Log($"container Create{itemType}");
                    containerManager.CreateObject(obj, itemType);
                }
            }
            if (item.requiredAmount >= item.currentAmount)
            {
                totalItemsCollected += item.currentAmount;
            }
            else
            {
                totalItemsCollected += item.requiredAmount;
            }
            
            if(item.currentAmount == item.requiredAmount)
            {
                // 禁用默认图标

                defaultIcons[taskItems.IndexOf(item)].SetActive(false);

                
                // 启用完成图标

                completedIcons[taskItems.IndexOf(item)].SetActive(true);

                
                Debug.Log($"任务项 {item.itemName} 完成，图标已更换");
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
            StartCoroutine(TaskCompleted());
        }
    }

    IEnumerator TaskCompleted()
    {
        isTaskActive = false;
        Debug.Log("������ɣ�");

        if (successPanel)
        {
            successPanel.SetActive(true);
        }

        SeedStatic.numScene++;
        SeedStatic.lightYear += Random.Range(minYear, maxYear);

        containerManager.AttrackAll();

        // �ӳ��л�����һ������
        yield return new WaitUntil(() => ContainerManager.isAllAttracted);

        StartCoroutine(LoadNextSceneAfterDelay(5f));
    }

    void TaskFailed()
    {
        isTaskActive = false;
        Debug.Log("����ʧ�ܣ�");
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
            // 到达结局时显示第一张图，等待用户点击
            SetImageAlpha(img1, 1f);
            img1.gameObject.SetActive(true);
        }

    }

    IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SeedStatic.tileSeed = Random.Range(1, 10000);
            SeedStatic.objectSeed = Random.Range(1, 10000);
            
            // 设置加载数据并使用加载界面
            LoadingData.SetLoadingData(nextSceneName, SeedStatic.lightYear, SeedStatic.numScene, true, false, highLightYear);
            SceneManager.LoadScene("LoadingScene");
        }
    }

    // �ֶ����¿�ʼ��������UI��ť��
    public void RestartTask()
    {
        // 使用加载界面重新加载当前场景
        LoadingData.SetLoadingData(SceneManager.GetActiveScene().name, SeedStatic.lightYear, SeedStatic.numScene, false, true, highLightYear);
        SceneManager.LoadScene("LoadingScene");
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

    #region Dialog System
    private void FindAndSetupInkReader()
    {
        _inkReader = FindObjectOfType<InkReader>();

        if (_inkReader == null)
        {
            Debug.LogWarning("No InkReader found in the current scene for TaskManager.");
        }
        else
        {
            Debug.Log($"TaskManager found InkReader: {_inkReader.gameObject.name}");
        }
    }

    public bool IsEndStoryPlaying => _inkReader != null && _inkReader.IsStoryPlaying && isEndStoryPlaying;

    public void PlayEndStory()
    {
        if (CurrentEndStory != null && _inkReader != null && !hasEndStoryPlayed)
        {
            // 在播放对话前隐藏图片，防止图层遮挡
            if (img1 != null) img1.gameObject.SetActive(false);
            if (img2 != null) img2.gameObject.SetActive(false);
            
            _inkReader.SetAndInitializeStory(CurrentEndStory);
            isEndStoryPlaying = true;
            hasEndStoryPlayed = true;
        }
    }

    public void StopEndStory()
    {
        if (_inkReader != null)
        {
            _inkReader.StopStory();
            isEndStoryPlaying = false;
        }
    }

    // 检查对话是否结束
    private void CheckStoryCompletion()
    {
        if (isEndStoryPlaying && _inkReader != null && !_inkReader.IsStoryPlaying)
        {
            isEndStoryPlaying = false;
            ShowEndingImages();
        }
    }

    // 显示结局图片并开始切换
    private void ShowEndingImages()
    {
        if (img1 != null && img2 != null)
        {
            // 重新激活图片
            img1.gameObject.SetActive(true);
            img2.gameObject.SetActive(true);
            
            // 设置透明度
            SetImageAlpha(img1, 1f);
            SetImageAlpha(img2, 1f);
            
            // 开始图片切换动画
            if (currentTransition != null)
                StopCoroutine(currentTransition);
            currentTransition = StartCoroutine(FadeTransition());
        }
    }

    [ContextMenu("Reset End Story Progress")]
    private void ResetEndStoryProgress()
    {
        hasEndStoryPlayed = false;
        isEndStoryPlaying = false;
    }
    #endregion

}