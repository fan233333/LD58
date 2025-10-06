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
        public TextAsset story;
        public bool hasBeenPlayed = false;
        public string storyName => story != null ? story.name : "Empty";
    }

    [Header("Multiple Story Thresholds")]
    [SerializeField] private List<StoryThreshold> storyThresholds = new List<StoryThreshold>();

    private InkReader _inkReader;
    private bool _isInitialized = false;
    private string _lastPlayedStory = "";

    // 公共属性
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

        // 按阈值排序，确保从小到大
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

        FindAndSetupInkReader();
        CheckAndPlayStoryByCondition();
    }
    #endregion

    #region Multi-Threshold Story Logic
    private void CheckAndPlayStoryByCondition()
    {
        if (_inkReader == null) return;

        // 获取当前应该播放的故事
        StoryThreshold storyToPlay = GetStoryForCurrentLightYear();

        if (storyToPlay != null && !storyToPlay.hasBeenPlayed)
        {
            PlayStory(storyToPlay);
        }
    }

    private StoryThreshold GetStoryForCurrentLightYear()
    {
        float currentLightYear = SeedStatic.lightYear;

        // 找出所有满足阈值且未播放的故事
        var availableStories = storyThresholds
            .Where(s => currentLightYear >= s.lightYearThreshold && !s.hasBeenPlayed)
            .OrderBy(s => s.lightYearThreshold)
            .ToList();

        if (availableStories.Count == 0)
        {
            Debug.Log($"No available stories for lightYear: {currentLightYear}");
            return null;
        }

        // 策略1：播放阈值最高的满足条件的故事（最新进度）
        StoryThreshold highestStory = availableStories.Last();
        Debug.Log($"Selected highest story: {highestStory.storyName} (Threshold: {highestStory.lightYearThreshold})");

        return highestStory;

        // 策略2：播放阈值最低的满足条件的故事（按顺序）
        // return availableStories.First();
    }

    private void PlayStory(StoryThreshold storyThreshold)
    {
        if (storyThreshold.story != null && _inkReader != null)
        {
            _inkReader.SetAndInitializeStory(storyThreshold.story);
            storyThreshold.hasBeenPlayed = true;
            _lastPlayedStory = storyThreshold.storyName;

            Debug.Log($"Playing story: {storyThreshold.storyName} (LightYear: {SeedStatic.lightYear}, Threshold: {storyThreshold.lightYearThreshold})");
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

    // 手动检查并播放符合条件的最高阈值故事
    public void CheckAndPlayHighestStory()
    {
        StoryThreshold storyToPlay = GetStoryForCurrentLightYear();
        if (storyToPlay != null)
        {
            PlayStory(storyToPlay);
        }
    }

    // 获取当前应该播放的故事（不实际播放）
    public StoryThreshold GetCurrentStoryToPlay()
    {
        return GetStoryForCurrentLightYear();
    }

    // 重置所有故事的播放状态
    public void ResetAllStoryProgress()
    {
        foreach (var story in storyThresholds)
        {
            story.hasBeenPlayed = false;
        }
        _lastPlayedStory = "";
        Debug.Log("All story progress reset");
    }

    // 重置特定阈值故事的播放状态
    public void ResetStoryProgress(float threshold)
    {
        var story = storyThresholds.FirstOrDefault(s => Mathf.Approximately(s.lightYearThreshold, threshold));
        if (story != null)
        {
            story.hasBeenPlayed = false;
            Debug.Log($"Reset story progress for threshold: {threshold}");
        }
    }

    // 添加新的故事阈值
    public void AddStoryThreshold(float threshold, TextAsset storyAsset)
    {
        if (storyThresholds.Any(s => Mathf.Approximately(s.lightYearThreshold, threshold)))
        {
            Debug.LogWarning($"Story threshold {threshold} already exists!");
            return;
        }

        storyThresholds.Add(new StoryThreshold
        {
            lightYearThreshold = threshold,
            story = storyAsset
        });

        // 重新排序
        storyThresholds = storyThresholds.OrderBy(s => s.lightYearThreshold).ToList();
    }

    // 获取故事播放状态
    public bool HasStoryBeenPlayed(float threshold)
    {
        var story = storyThresholds.FirstOrDefault(s => Mathf.Approximately(s.lightYearThreshold, threshold));
        return story?.hasBeenPlayed ?? false;
    }

    // 获取下一个要播放的故事阈值
    public float GetNextStoryThreshold()
    {
        var nextStory = storyThresholds
            .Where(s => !s.hasBeenPlayed && SeedStatic.lightYear < s.lightYearThreshold)
            .OrderBy(s => s.lightYearThreshold)
            .FirstOrDefault();

        return nextStory != null ? nextStory.lightYearThreshold : -1f;
    }

    // 获取所有故事状态（用于UI显示）
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
        Debug.Log($"Last Played Story: {_lastPlayedStory}");

        foreach (var story in storyThresholds)
        {
            string status = story.hasBeenPlayed ? "PLAYED" : "NOT PLAYED";
            Debug.Log($"Story: {story.storyName} | Threshold: {story.lightYearThreshold} | Status: {status}");
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

}
