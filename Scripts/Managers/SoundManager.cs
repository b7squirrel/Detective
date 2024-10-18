using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] GameObject audioSourcePrefab;
    [SerializeField] int initialAudioSourceCount = 10;
    [SerializeField] float soundCooldown = 0.2f; // 사운드 쿨타임 (초 단위)
    [SerializeField] int maxPlayCountPerClip = 10; // 사운드가 최대 재생될 수 있는 횟수
    [SerializeField] float resetTime = .3f; // 재생 횟수 리셋 시간

    List<AudioSource> audioSourcePool;
    Dictionary<string, int> soundPlayCount; // 사운드 재생 횟수 추적
    Dictionary<string, float> lastPlayedTime; // 마지막으로 재생된 시간 추적

    AudioClip singleSound; // 한 번만 재생되는 사운드를 리스트에서 빼기 위해
    bool isMuted; // 현재 Mute 상태를 추적하기 위한 변수
    bool isPlayingHurtSound; // hurt Sound가 재생 중이면 재생하지 않기 위한 플래그

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// AudioSource들을 만들어서 리스트로 저장
    /// </summary>
    public void Init()
    {
        audioSourcePool = new List<AudioSource>();
        soundPlayCount = new Dictionary<string, int>();
        lastPlayedTime = new Dictionary<string, float>();

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

        // 풀에 사용 가능한 오디오 소스가 없으면 새로 생성
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
    public void Play(AudioClip audioClip)
    {
        if (!CanPlaySound(audioClip)) return; // 쿨타임이나 재생 횟수 체크

        AudioSource audioSource = GetAudioSourceFromPool();
        if (audioSource == null) return;

        audioSource.clip = audioClip;
        audioSource.volume = 1f;
        //audioSource.pitch = Random.Range(0.95f, 1.05f); // 피치 랜덤화
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

        // 재생 횟수 리셋 처리
        if (lastPlayedTime.ContainsKey(clipName) && Time.time - lastPlayedTime[clipName] > resetTime)
        {
            soundPlayCount[clipName] = 0;
        }

        // 최대 재생 횟수 체크
        if (soundPlayCount.ContainsKey(clipName) && soundPlayCount[clipName] >= maxPlayCountPerClip)
        {
            return false;
        }

        //// 쿨타임 체크
        //if (lastPlayedTime.ContainsKey(clipName) && Time.time - lastPlayedTime[clipName] < soundCooldown)
        //{
        //    return false;
        //}

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

        // 마지막 재생 시간 업데이트
        lastPlayedTime[clipName] = Time.time;
    }

    public void PlaySoundWith(AudioClip _hurtSound, float _volume, bool _pitch)
    {
        AudioSource audioSource = GetAudioSourceFromPool();
        if (audioSource == null) return;

        audioSource.clip = _hurtSound;
        audioSource.volume = .4f;
        audioSource.pitch = Random.Range(0.95f, 1.05f); // 피치 랜덤화
        audioSource.mute = isMuted;
        audioSource.Play();

        UpdateSoundPlayInfo(_hurtSound); // 재생 횟수와 시간 업데이트
    }

    // 사운드를 Mute/Unmute 하는 메서드 추가
    public void SetState(bool _state)
    {
        isMuted = !_state;
        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource != null)
            {
                audioSource.mute = isMuted;
            }
        }
    }
}