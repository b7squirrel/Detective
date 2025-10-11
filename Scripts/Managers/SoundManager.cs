using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// SoundManager의 3중 안전장치
/// 쿨다운 체크: 같은 소리가 너무 빠르게 재생되지 않도록
/// 동시 재생 제한: 같은 소리가 동시에 3개 이상 재생 안 됨
/// 재생 횟수 제한: 짧은 시간(0.3초) 내 최대 5회까지만
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] GameObject audioSourcePrefab;
    [SerializeField] int initialAudioSourceCount = 10;
    [SerializeField] float soundCooldown = 0.2f;
    [SerializeField] int maxPlayCountPerClip = 5;
    [SerializeField] float resetTime = .3f;
    [SerializeField] int maxSimultaneousSounds = 3; // 동시 재생 제한 추가

    List<AudioSource> audioSourcePool;
    Dictionary<string, int> soundPlayCount;
    Dictionary<string, float> lastPlayedTime;
    Dictionary<string, AudioSource> loopingSounds;

    AudioClip singleSound;
    bool isMuted;
    bool isPlayingHurtSound;

    void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        if (audioSourcePool != null)
        {
            CleanupAudioSourcePool();
        }

        audioSourcePool = new List<AudioSource>();
        soundPlayCount = new Dictionary<string, int>();
        lastPlayedTime = new Dictionary<string, float>();
        loopingSounds = new Dictionary<string, AudioSource>();

        for (int i = 0; i < initialAudioSourceCount; i++)
        {
            CreateNewAudioSource();
        }

        Debug.Log($"SoundManager 초기화 완료: {audioSourcePool.Count}개 AudioSource 생성");
    }

    void CleanupAudioSourcePool()
    {
        if (audioSourcePool == null) return;

        audioSourcePool = audioSourcePool.Where(source => source != null).ToList();

        var keysToRemove = new List<string>();
        foreach (var kvp in loopingSounds)
        {
            if (kvp.Value == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (string key in keysToRemove)
        {
            loopingSounds.Remove(key);
        }
    }

    AudioSource GetAudioSourceFromPool()
    {
        CleanupAudioSourcePool();

        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        return CreateNewAudioSource();
    }

    AudioSource CreateNewAudioSource()
    {
        try
        {
            if (audioSourcePrefab == null)
            {
                Debug.LogError("AudioSource Prefab이 설정되지 않았습니다.");
                return null;
            }

            GameObject go = Instantiate(audioSourcePrefab, transform);
            if (go == null)
            {
                Debug.LogError("AudioSource GameObject 생성 실패");
                return null;
            }

            go.transform.localPosition = Vector2.zero;
            AudioSource audioSource = go.GetComponent<AudioSource>();

            if (audioSource == null)
            {
                Debug.LogError("AudioSource 컴포넌트를 찾을 수 없습니다.");
                Destroy(go);
                return null;
            }

            if (audioSourcePool == null)
            {
                audioSourcePool = new List<AudioSource>();
            }

            audioSourcePool.Add(audioSource);
            return audioSource;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AudioSource 생성 중 오류: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 특정 클립이 현재 재생 중인 개수 확인
    /// </summary>
    int GetPlayingCount(AudioClip audioClip)
    {
        if (audioSourcePool == null || audioClip == null) return 0;

        int count = 0;
        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == audioClip)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 특정 사운드 클립 재생 (동시 재생 제한 포함)
    /// </summary>
    public void Play(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다.");
            return;
        }

        // 동시 재생 개수 체크
        if (GetPlayingCount(audioClip) >= maxSimultaneousSounds)
        {
            return;
        }

        if (!CanPlaySound(audioClip)) return;

        AudioSource audioSource = GetAudioSourceFromPool();
        if (audioSource == null)
        {
            Debug.LogWarning("사용 가능한 AudioSource를 찾을 수 없습니다.");
            return;
        }

        try
        {
            audioSource.clip = audioClip;
            audioSource.volume = 1f;
            audioSource.pitch = 1f;
            audioSource.loop = false;
            audioSource.mute = isMuted;
            audioSource.Play();

            UpdateSoundPlayInfo(audioClip);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"사운드 재생 중 오류: {e.Message}");
        }
    }

    bool CanPlaySound(AudioClip audioClip)
    {
        if (audioClip == null) return false;

        string clipName = audioClip.name;

        if (lastPlayedTime == null) lastPlayedTime = new Dictionary<string, float>();
        if (soundPlayCount == null) soundPlayCount = new Dictionary<string, int>();

        if (lastPlayedTime.ContainsKey(clipName) && Time.time - lastPlayedTime[clipName] > resetTime)
        {
            soundPlayCount[clipName] = 0;
        }

        if (soundPlayCount.ContainsKey(clipName) && soundPlayCount[clipName] >= maxPlayCountPerClip)
        {
            return false;
        }

        return true;
    }

    void UpdateSoundPlayInfo(AudioClip audioClip)
    {
        if (audioClip == null) return;

        string clipName = audioClip.name;

        if (soundPlayCount == null) soundPlayCount = new Dictionary<string, int>();
        if (lastPlayedTime == null) lastPlayedTime = new Dictionary<string, float>();

        if (!soundPlayCount.ContainsKey(clipName))
        {
            soundPlayCount[clipName] = 0;
        }
        soundPlayCount[clipName]++;

        lastPlayedTime[clipName] = Time.time;
    }

    public void PlaySoundWith(AudioClip _audioClip, float _volume, bool _pitch, float _coolDown)
    {
        if (_audioClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다.");
            return;
        }

        string clipName = _audioClip.name;
        if (lastPlayedTime == null) lastPlayedTime = new Dictionary<string, float>();

        // 쿨다운 체크를 먼저 수행
        if (lastPlayedTime.ContainsKey(clipName) && Time.time - lastPlayedTime[clipName] < _coolDown)
            return;

        // 동시 재생 개수 체크
        if (GetPlayingCount(_audioClip) >= maxSimultaneousSounds)
        {
            return;
        }

        AudioSource audioSource = GetAudioSourceFromPool();
        if (audioSource == null)
        {
            Debug.LogWarning("사용 가능한 AudioSource를 찾을 수 없습니다.");
            return;
        }

        try
        {
            audioSource.clip = _audioClip;
            audioSource.volume = Mathf.Clamp01(_volume);
            audioSource.pitch = 1f;

            if (_pitch) audioSource.pitch = Random.Range(0.95f, 1.05f);

            audioSource.loop = false;
            audioSource.mute = isMuted;
            audioSource.Play();

            lastPlayedTime[clipName] = Time.time;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"사운드 재생 중 오류: {e.Message}");
        }
    }

    public AudioSource PlayLoop(AudioClip audioClip, float volume = 1f)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다.");
            return null;
        }

        string clipName = audioClip.name;

        if (loopingSounds == null) loopingSounds = new Dictionary<string, AudioSource>();

        if (loopingSounds.ContainsKey(clipName))
        {
            AudioSource existingSource = loopingSounds[clipName];
            if (existingSource != null && existingSource.isPlaying)
            {
                return existingSource;
            }
            else
            {
                loopingSounds.Remove(clipName);
            }
        }

        AudioSource audioSource = GetAudioSourceFromPool();
        if (audioSource == null) return null;

        try
        {
            audioSource.clip = audioClip;
            audioSource.volume = Mathf.Clamp01(volume);
            audioSource.pitch = 1f;
            audioSource.loop = true;
            audioSource.mute = isMuted;
            audioSource.Play();

            loopingSounds[clipName] = audioSource;

            return audioSource;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"루프 사운드 재생 중 오류: {e.Message}");
            return null;
        }
    }

    public void StopLoop(AudioClip audioClip)
    {
        if (audioClip == null) return;

        string clipName = audioClip.name;

        if (loopingSounds == null) return;

        if (loopingSounds.ContainsKey(clipName))
        {
            AudioSource audioSource = loopingSounds[clipName];
            if (audioSource != null)
            {
                try
                {
                    audioSource.Stop();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"루프 사운드 중단 중 오류: {e.Message}");
                }
            }

            loopingSounds.Remove(clipName);
        }
    }

    public void StopAllLoops()
    {
        if (loopingSounds == null) return;

        foreach (var audioSource in loopingSounds.Values)
        {
            if (audioSource != null)
            {
                try
                {
                    audioSource.Stop();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"루프 사운드 중단 중 오류: {e.Message}");
                }
            }
        }

        loopingSounds.Clear();
    }

    #region 사운드 일시 정지/재생
    public void PauseSound(AudioClip audioClip)
    {
        if (audioClip == null || audioSourcePool == null) return;

        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == audioClip)
            {
                try
                {
                    audioSource.Pause();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"사운드 일시정지 중 오류: {e.Message}");
                }
            }
        }
    }

    public void ResumeSound(AudioClip audioClip)
    {
        if (audioClip == null || audioSourcePool == null) return;

        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource != null && audioSource.clip == audioClip && !audioSource.isPlaying)
            {
                try
                {
                    audioSource.UnPause();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"사운드 재개 중 오류: {e.Message}");
                }
            }
        }
    }

    public void SetState(bool _state)
    {
        isMuted = !_state;

        if (audioSourcePool == null) return;

        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource != null)
            {
                try
                {
                    audioSource.mute = isMuted;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"AudioSource mute 설정 중 오류: {e.Message}");
                }
            }
        }
    }
    #endregion

    public void StopAllSounds()
    {
        if (audioSourcePool == null) return;

        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                try
                {
                    audioSource.Stop();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"사운드 중단 중 오류: {e.Message}");
                }
            }
        }

        StopAllLoops();
    }

    void OnDestroy()
    {
        StopAllSounds();

        if (instance == this)
        {
            instance = null;
        }
    }
}