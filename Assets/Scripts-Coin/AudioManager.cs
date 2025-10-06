using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("背景音乐")]
    public AudioClip backgroundMusic;

    [Header("切换目标音乐")]
    public AudioClip targetMusic;

    public TaskManager taskManager;
    public float fadeDuration = 0.5f;
    private bool hasSwitched = false;

    private AudioSource audioSource;
    private float remainTime;

    void Awake()
    {
        // 单例模式，确保只有一个音频管理器
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true; // 循环播放
        }

        // 订阅场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 场景加载完成后播放音乐
        PlayBackgroundMusic();
    }

    void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && !audioSource.isPlaying)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
    }

    void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        remainTime = taskManager.GetRemainTime();
        if (remainTime < 30 && !hasSwitched)
        {
            StartCoroutine(SwitchWithFade());
            hasSwitched = true;
        }
    }

    /// <summary>
    /// 切换到指定音乐
    /// </summary>
    IEnumerator SwitchWithFade()
    {
        hasSwitched = true;

        // 淡出当前音乐
        float startVolume = audioSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        // 切换音乐
        audioSource.clip = targetMusic;
        audioSource.Play();

        // 淡入新音乐
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = startVolume;
    }
}