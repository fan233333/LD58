using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("��������")]
    public AudioClip backgroundMusic;

    [Header("�л�Ŀ������")]
    public AudioClip targetMusic;

    public TaskManager taskManager;
    public float fadeDuration = 0.5f;
    private bool hasSwitched = false;

    private AudioSource audioSource;
    private float remainTime;

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
        // ����������ɺ󲥷�����
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
        // ȡ�������¼�����ֹ�ڴ�й©
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
    /// �л���ָ������
    /// </summary>
    IEnumerator SwitchWithFade()
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
        audioSource.clip = targetMusic;
        audioSource.Play();

        // ����������
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = startVolume;
    }
}