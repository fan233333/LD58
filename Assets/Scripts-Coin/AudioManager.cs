using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("��������")]
    public AudioClip backgroundMusic;

    [Header("�л�Ŀ������")]
    public AudioClip targetMusic1;
    public AudioClip targetMusic2;

    public TaskManager taskManager;
    public static bool isActive;
    public float fadeDuration = 0.5f;
    private bool hasSwitched = false;

    private AudioSource audioSource;
    private float remainTime;
    private string menuSceneName = "MainMenu";


    void Awake()
    {
        // ����ģʽ��ȷ��ֻ��һ����Ƶ������
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
            audioSource.loop = true; // ѭ������
        }

        // ���ĳ��������¼�
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ����Ƿ��ǲ˵�����
        if (scene.name == menuSceneName)
        {
            // ����ǲ˵������������Լ�
            DestroyAudioManager();
        }
        else
        {
            // �����������ű�������
            PlayBackgroundMusic();
        }
    }

    void DestroyAudioManager()
    {
        // ȡ�������¼�
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // ���ٵ���ʵ��
        if (Instance == this)
        {
            Instance = null;
        }

        // ������Ϸ����
        Destroy(gameObject);
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
        // ȡ�������¼�����ֹ�ڴ�й©
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

private void Update()
{
    if (taskManager == null)
    {
        taskManager = GameObject.Find("TaskManager").GetComponent<TaskManager>();
        return;
    }
    
    if (!isActive) return;
    
    remainTime = taskManager.GetRemainTime();
    
    // 使用当前播放的音乐来判断是否需要切换
    AudioClip currentClip = audioSource.clip;
    AudioClip targetClip = null;
    
    if (remainTime < 30)
    {
        targetClip = targetMusic2;  // 高度紧张音乐
    }
    else if (remainTime < 90)
    {
        targetClip = targetMusic1;  // 中等紧张音乐
    }
    else
    {
        targetClip = backgroundMusic; // 正常背景音乐
    }

        // 只有当目标音乐与当前音乐不同时才切换
        if (targetClip != null && currentClip != targetClip)
        {
            StartCoroutine(SwitchWithFade(targetClip));
            Debug.Log($"切换音乐，剩余时间: {remainTime}秒");
            Debug.Log($"当前音乐: {currentClip?.name}, 目标音乐: {targetClip.name}");
            Debug.Log(audioSource.volume);
    }
}




    /// <summary>
    /// �л���ָ������
    /// </summary>
    IEnumerator SwitchWithFade(AudioClip target)
    {
        hasSwitched = true;

        // ������ǰ����
        float startVolume = audioSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        // �л�����
        audioSource.clip = target;
        audioSource.Play();

        // ����������
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0.8f;
    }
}