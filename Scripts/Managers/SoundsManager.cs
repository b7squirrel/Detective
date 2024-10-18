using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager instance;

    [SerializeField] GameObject audioSourcePrefab;
    [SerializeField] int initialAudioSourceCount = 10;
    [SerializeField] float soundCooldown = 0.2f; // ���� ��Ÿ�� (�� ����)
    [SerializeField] int maxPlayCountPerClip = 3; // ���尡 �ִ� ����� �� �ִ� Ƚ��
    [SerializeField] float resetTime = 1.0f; // ��� Ƚ�� ���� �ð�

    private List<AudioSource> audioSourcePool;
    private Dictionary<string, int> soundPlayCount; // ���� ��� Ƚ�� ����
    private bool isMuted;

    void Awake()
    {
        instance = this;
        Init();
    }

    /// <summary>
    /// ������ҽ� Ǯ �ʱ�ȭ
    /// </summary>
    void Init()
    {
        audioSourcePool = new List<AudioSource>();
        soundPlayCount = new Dictionary<string, int>();

        for (int i = 0; i < initialAudioSourceCount; i++)
        {
            CreateNewAudioSource();
        }
    }

    /// <summary>
    /// ������ҽ��� Ǯ���� �������� (��� ���� �ƴ� ������ҽ�)
    /// </summary>
    AudioSource GetAudioSourceFromPool()
    {
        foreach (var audioSource in audioSourcePool)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        // Ǯ�� ��� ���� ����� �ҽ��� ������ ���� ����
        return CreateNewAudioSource();
    }

    /// <summary>
    /// ���ο� ������ҽ��� Ǯ�� �߰�
    /// </summary>
    AudioSource CreateNewAudioSource()
    {
        GameObject go = Instantiate(audioSourcePrefab, transform);
        go.transform.localPosition = Vector2.zero;
        AudioSource audioSource = go.GetComponent<AudioSource>();
        audioSourcePool.Add(audioSource);
        return audioSource;
    }

    /// <summary>
    /// Ư�� ���� Ŭ�� ���
    /// </summary>
    public void PlaySound(AudioClip audioClip)
    {
        if (!CanPlaySound(audioClip)) return; // ��Ÿ���̳� ��� Ƚ�� üũ

        AudioSource audioSource = GetAudioSourceFromPool();
        if (audioSource == null) return;

        audioSource.clip = audioClip;
        audioSource.volume = 1f;
        audioSource.pitch = Random.Range(0.95f, 1.05f); // ��ġ ����ȭ
        audioSource.mute = isMuted;
        audioSource.Play();

        UpdateSoundPlayInfo(audioClip); // ��� Ƚ���� �ð� ������Ʈ
    }

    /// <summary>
    /// ���带 ����� �� �ִ��� ���θ� �Ǵ� (��Ÿ��, ��� Ƚ�� ���� ����)
    /// </summary>
    bool CanPlaySound(AudioClip audioClip)
    {
        string clipName = audioClip.name;

        // �ִ� ��� Ƚ�� üũ
        if (soundPlayCount.ContainsKey(clipName) && soundPlayCount[clipName] >= maxPlayCountPerClip)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// ���� ��� �� ���� ������Ʈ (��� Ƚ��, ������ ��� �ð�)
    /// </summary>
    void UpdateSoundPlayInfo(AudioClip audioClip)
    {
        string clipName = audioClip.name;

        // ��� Ƚ�� ����
        if (!soundPlayCount.ContainsKey(clipName))
        {
            soundPlayCount[clipName] = 0;
        }
        soundPlayCount[clipName]++;
    }

    /// <summary>
    /// ���� Mute/Unmute ���
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;

        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource != null)
            {
                audioSource.mute = isMuted;
            }
        }
    }

    /// <summary>
    /// ������Ʈ Ǯ���� ��� ������ ������ҽ��� ��� ���߰� �ʱ�ȭ
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}