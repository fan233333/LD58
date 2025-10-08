using Phosphorescence.Narration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManageScenes : MonoBehaviour
{
    #region Singleton
    private static ManageScenes _instance;
    public static ManageScenes Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ManageScenes>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("StoryManager");
                    _instance = go.AddComponent<ManageScenes>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    [System.Serializable]
    public class StoryThreshold
    {
        public float lightYearThreshold;
        //public TextAsset story;
        [Tooltip("���Ĺ���")]
        public TextAsset storyCN;
        [Tooltip("Ӣ�Ĺ���")]
        public TextAsset storyEN;
        public bool hasBeenPlayed = false;
        //public string storyName => story != null ? story.name : "Empty";
        // �����������÷��ض�Ӧ�Ĺ���
        public TextAsset CurrentStory => SeedStatic.isEng ? storyEN : storyCN;

        // �������ƣ�������ʾ�ͼ�¼��
        public string storyName => CurrentStory != null ? CurrentStory.name : "Empty";

        // ����Ƿ��п��õĹ���
        public bool HasValidStory => CurrentStory != null;
    }

    [Header("Multiple Story Thresholds")]
    [SerializeField] private List<StoryThreshold> storyThresholds = new List<StoryThreshold>();

    private InkReader _inkReader;
    private bool _isInitialized = false;
    private string _lastPlayedStory = "";

    // ��������
    public InkReader CurrentInkReader => _inkReader;
    public bool IsStoryPlaying => _inkReader != null && _inkReader.IsStoryPlaying;
    public string LastPlayedStory => _lastPlayedStory;


    #region Unity Lifecycle
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion


    #region Initialization
    private void Initialize()
    {
        if (_isInitialized) return;

        FindAndSetupInkReader();
        _isInitialized = true;

        // ����ֵ����ȷ����С����
        storyThresholds = storyThresholds.OrderBy(s => s.lightYearThreshold).ToList();
    }

    private void FindAndSetupInkReader()
    {
        _inkReader = FindObjectOfType<InkReader>();

        if (_inkReader == null)
        {
            Debug.LogWarning("No InkReader found in the current scene.");
        }
        else
        {
            Debug.Log($"Found InkReader: {_inkReader.gameObject.name}");
        }
    }
    #endregion

    #region Scene Management
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        // 检查是否是主菜单场景，如果是则清零光年
        if (IsMainMenuScene(scene.name))
        {
            ResetLightYearProgress();
        }

        FindAndSetupInkReader();
        CheckAndPlayStoryByCondition();
    }
    #endregion

    #region Multi-Threshold Story Logic
    private void CheckAndPlayStoryByCondition()
    {
        if (_inkReader == null) return;

        // ��ȡ��ǰӦ�ò��ŵĹ���
        StoryThreshold storyToPlay = GetStoryForCurrentLightYear();

        if (storyToPlay != null && !storyToPlay.hasBeenPlayed)
        {
            PlayStory(storyToPlay);
        }
    }

    private StoryThreshold GetStoryForCurrentLightYear()
    {
        float currentLightYear = SeedStatic.lightYear;

        // �ҳ�����������ֵ��δ���ŵĹ���
        var availableStories = storyThresholds
            .Where(s => currentLightYear >= s.lightYearThreshold && !s.hasBeenPlayed)
            .OrderBy(s => s.lightYearThreshold)
            .ToList();

        if (availableStories.Count == 0)
        {
            Debug.Log($"No available stories for lightYear: {currentLightYear}");
            return null;
        }

        // ����1��������ֵ��ߵ����������Ĺ��£����½��ȣ�
        StoryThreshold highestStory = availableStories.Last();
        Debug.Log($"Selected highest story: {highestStory.storyName} (Threshold: {highestStory.lightYearThreshold}, Language: {(SeedStatic.isEng ? "EN" : "CN")})");
        return highestStory;

        // ����2��������ֵ��͵����������Ĺ��£���˳��
        // return availableStories.First();
    }

    private void PlayStory(StoryThreshold storyThreshold)
    {
        //if (storyThreshold.story != null && _inkReader != null)
        //{
        //    _inkReader.SetAndInitializeStory(storyThreshold.story);
        //    storyThreshold.hasBeenPlayed = true;
        //    _lastPlayedStory = storyThreshold.storyName;

        //    Debug.Log($"Playing story: {storyThreshold.storyName} (LightYear: {SeedStatic.lightYear}, Threshold: {storyThreshold.lightYearThreshold})");
        //}
        if (storyThreshold.HasValidStory && _inkReader != null)
        {
            _inkReader.SetAndInitializeStory(storyThreshold.CurrentStory);
            storyThreshold.hasBeenPlayed = true;
            _lastPlayedStory = storyThreshold.storyName;

            Debug.Log($"Playing story: {storyThreshold.storyName} (LightYear: {SeedStatic.lightYear}, Threshold: {storyThreshold.lightYearThreshold}, Language: {(SeedStatic.isEng ? "EN" : "CN")})");
        }
    }
    #endregion

    #region Public Methods
    public void PlayStory(TextAsset storyAsset)
    {
        if (storyAsset != null && _inkReader != null)
        {
            _inkReader.SetAndInitializeStory(storyAsset);
            _lastPlayedStory = storyAsset.name;
        }
    }

    public void StopCurrentStory()
    {
        if (_inkReader != null)
        {
            _inkReader.StopStory();
        }
    }

    // �ֶ���鲢���ŷ��������������ֵ����
    public void CheckAndPlayHighestStory()
    {
        StoryThreshold storyToPlay = GetStoryForCurrentLightYear();
        if (storyToPlay != null)
        {
            PlayStory(storyToPlay);
        }
    }

    // ��ȡ��ǰӦ�ò��ŵĹ��£���ʵ�ʲ��ţ�
    public StoryThreshold GetCurrentStoryToPlay()
    {
        return GetStoryForCurrentLightYear();
    }

    // �������й��µĲ���״̬
    public void ResetAllStoryProgress()
    {
        foreach (var story in storyThresholds)
        {
            story.hasBeenPlayed = false;
        }
        _lastPlayedStory = "";
        Debug.Log("All story progress reset");
    }

    // �����ض���ֵ���µĲ���״̬
    public void ResetStoryProgress(float threshold)
    {
        var story = storyThresholds.FirstOrDefault(s => Mathf.Approximately(s.lightYearThreshold, threshold));
        if (story != null)
        {
            story.hasBeenPlayed = false;
            Debug.Log($"Reset story progress for threshold: {threshold}");
        }
    }

    // �����µĹ�����ֵ
    public void AddStoryThreshold(float threshold, TextAsset storyAssetCN, TextAsset storyAssetEN)
    {
        if (storyThresholds.Any(s => Mathf.Approximately(s.lightYearThreshold, threshold)))
        {
            Debug.LogWarning($"Story threshold {threshold} already exists!");
            return;
        }

        storyThresholds.Add(new StoryThreshold
        {
            lightYearThreshold = threshold,
            storyCN = storyAssetCN,
            storyEN = storyAssetEN
        });

        // ��������
        storyThresholds = storyThresholds.OrderBy(s => s.lightYearThreshold).ToList();
    }

    // ��ȡ���²���״̬
    public bool HasStoryBeenPlayed(float threshold)
    {
        var story = storyThresholds.FirstOrDefault(s => Mathf.Approximately(s.lightYearThreshold, threshold));
        return story?.hasBeenPlayed ?? false;
    }

    // ��ȡ��һ��Ҫ���ŵĹ�����ֵ
    public float GetNextStoryThreshold()
    {
        var nextStory = storyThresholds
            .Where(s => !s.hasBeenPlayed && SeedStatic.lightYear < s.lightYearThreshold)
            .OrderBy(s => s.lightYearThreshold)
            .FirstOrDefault();

        return nextStory != null ? nextStory.lightYearThreshold : -1f;
    }

    // ��ȡ���й���״̬������UI��ʾ��
    public List<StoryThreshold> GetAllStoryStatus()
    {
        return new List<StoryThreshold>(storyThresholds);
    }
    #endregion

    #region Debug and Editor
    [ContextMenu("Debug Story Status")]
    private void DebugStoryStatus()
    {
        Debug.Log($"Current LightYear: {SeedStatic.lightYear}");
        Debug.Log($"Current Language: {(SeedStatic.isEng ? "EN" : "CN")}");
        Debug.Log($"Last Played Story: {_lastPlayedStory}");

        foreach (var story in storyThresholds)
        {
            string status = story.hasBeenPlayed ? "PLAYED" : "NOT PLAYED";
            string available = story.HasValidStory ? "AVAILABLE" : "MISSING";
            Debug.Log($"Story: {story.storyName} | Threshold: {story.lightYearThreshold} | Status: {status} | {available}");
        }

        StoryThreshold nextStory = GetStoryForCurrentLightYear();
        if (nextStory != null)
        {
            Debug.Log($"Next story to play: {nextStory.storyName}");
        }
        else
        {
            Debug.Log("No stories available to play");
        }
    }

    [ContextMenu("Force Check and Play")]
    private void ForceCheckAndPlay()
    {
        CheckAndPlayStoryByCondition();
    }

    [ContextMenu("Reset All Progress")]
    private void EditorResetAllProgress()
    {
        ResetAllStoryProgress();
    }
    #endregion


// 检查是否是主菜单场景的方法
private bool IsMainMenuScene(string sceneName)
{
    // 可以添加多个主菜单场景名称
    string[] mainMenuSceneNames = { "MainMenu", "StartMenu", "Menu", "Main" };
    return mainMenuSceneNames.Any(name => 
        string.Equals(sceneName, name, System.StringComparison.OrdinalIgnoreCase));
}

// 重置光年进度的方法
private void ResetLightYearProgress()
{
    float oldLightYear = SeedStatic.lightYear;
    SeedStatic.lightYear = 0;
    
    Debug.Log($"Light year reset from {oldLightYear} to 0 when returning to main menu");
    
    // 可选：同时重置其他相关数据
    // SeedStatic.numScene = 0;
    
    // 可选：重置故事播放状态
    ResetAllStoryProgress();
}

}
