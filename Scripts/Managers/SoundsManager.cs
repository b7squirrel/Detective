using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager instance;

    [SerializeField] GameObject audioSourcePrefab;
    [SerializeField] int initialAudioSourceCount = 10;
    [SerializeField] float soundCooldown = 0.2f; // 사운드 쿨타임 (초 단위)
    [SerializeField] int maxPlayCountPerClip = 3; // 사운드가 최대 재생될 수 있는 횟수
    [SerializeField] float resetTime = 1.0f; // 재생 횟수 리셋 시간

    private List<AudioSource> audioSourcePool;
    private Dictionary<string, int> soundPlayCount; // 사운드 재생 횟수 추적
    private bool isMuted;

    void Awake()
    {
        instance = this;
        Init();
    }

    /// <summary>
    /// 오디오소스 풀 초기화
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
    /// 오디오소스를 풀에서 가져오기 (재생 중이 아닌 오디오소스)
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

        // 풀에 재생 중인 오디오 소스가 없으면 새로 생성
        return CreateNewAudioSource();
    }

    /// <summary>
    /// 새로운 오디오소스를 풀에 추가
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
    /// 특정 사운드 클립 재생
    /// </summary>
    public void PlaySound(AudioClip audioClip)
    {
        if (!CanPlaySound(audioClip)) return; // 쿨타임이나 재생 횟수 체크

        AudioSource audioSource = GetAudioSourceFromPool();
        if (audioSource == null) return;

        audioSource.clip = audioClip;
        audioSource.volume = 1f;
        audioSource.pitch = Random.Range(0.95f, 1.05f); // 피치 랜덤화
        audioSource.mute = isMuted;
        audioSource.Play();

        UpdateSoundPlayInfo(audioClip); // 재생 횟수와 시간 업데이트
    }

    /// <summary>
    /// 사운드를 재생할 수 있는지 여부를 판단 (쿨타임, 재생 횟수 제한 적용)
    /// </summary>
    bool CanPlaySound(AudioClip audioClip)
    {
        string clipName = audioClip.name;

        // 최대 재생 횟수 체크
        if (soundPlayCount.ContainsKey(clipName) && soundPlayCount[clipName] >= maxPlayCountPerClip)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 사운드 재생 후 정보 업데이트 (재생 횟수, 마지막 재생 시간)
    /// </summary>
    void UpdateSoundPlayInfo(AudioClip audioClip)
    {
        string clipName = audioClip.name;

        // 재생 횟수 증가
        if (!soundPlayCount.ContainsKey(clipName))
        {
            soundPlayCount[clipName] = 0;
        }
        soundPlayCount[clipName]++;
    }

    /// <summary>
    /// 사운드 Mute/Unmute 토글
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
    /// 오브젝트 풀에서 사용 가능한 오디오소스를 모두 멈추고 초기화
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