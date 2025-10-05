using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] GameObject audioSourcePrefab;
    [SerializeField] int initialAudioSourceCount = 10;
    [SerializeField] float soundCooldown = 0.2f; // 사운드 쿨타임 (초 단위)
    [SerializeField] int maxPlayCountPerClip = 5; // 사운드가 최대 재생될 수 있는 횟수
    [SerializeField] float resetTime = .3f; // 재생 횟수 리셋 시간

    List<AudioSource> audioSourcePool;
    Dictionary<string, int> soundPlayCount; // 사운드 재생 횟수 추적
    Dictionary<string, float> lastPlayedTime; // 마지막으로 재생된 시간 추적
    Dictionary<string, AudioSource> loopingSounds; // 루프 재생 중인 사운드 추적

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
        if (audioSourcePool != null)
        {
            // 기존 풀 정리
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

    /// <summary>
    /// 파괴된 AudioSource들을 풀에서 제거
    /// </summary>
    void CleanupAudioSourcePool()
    {
        if (audioSourcePool == null) return;

        // 파괴된 AudioSource들을 제거
        audioSourcePool = audioSourcePool.Where(source => source != null).ToList();
        
        // 루프 사운드에서도 파괴된 것들 제거
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

    /// <summary>
    /// 오디오소스를 풀에서 가져오기 (재생 중이 아닌 오디오소스)
    /// </summary>
    AudioSource GetAudioSourceFromPool()
    {
        // 먼저 풀을 정리
        CleanupAudioSourcePool();

        // 사용 가능한 AudioSource 찾기
        foreach (var audioSource in audioSourcePool)
        {
            if (audioSource != null && !audioSource.isPlaying)
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
    /// 특정 사운드 클립 재생
    /// </summary>
    public void Play(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다.");
            return;
        }

        if (!CanPlaySound(audioClip)) return; // 쿨타임이나 재생 횟수 체크

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

            UpdateSoundPlayInfo(audioClip); // 재생 횟수와 시간 업데이트
        }
        catch (System.Exception e)
        {
            Debug.LogError($"사운드 재생 중 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 사운드를 재생할 수 있는지 여부를 판단 (쿨타임, 재생 횟수 제한 적용)
    /// </summary>
    bool CanPlaySound(AudioClip audioClip)
    {
        if (audioClip == null) return false;

        string clipName = audioClip.name;

        // Dictionary 초기화 확인
        if (lastPlayedTime == null) lastPlayedTime = new Dictionary<string, float>();
        if (soundPlayCount == null) soundPlayCount = new Dictionary<string, int>();

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

        return true;
    }

    /// <summary>
    /// 사운드 재생 후 정보 업데이트 (재생 횟수, 마지막 재생 시간)
    /// </summary>
    void UpdateSoundPlayInfo(AudioClip audioClip)
    {
        if (audioClip == null) return;

        string clipName = audioClip.name;

        // Dictionary 초기화 확인
        if (soundPlayCount == null) soundPlayCount = new Dictionary<string, int>();
        if (lastPlayedTime == null) lastPlayedTime = new Dictionary<string, float>();

        // 재생 횟수 증가
        if (!soundPlayCount.ContainsKey(clipName))
        {
            soundPlayCount[clipName] = 0;
        }
        soundPlayCount[clipName]++;

        // 마지막 재생 시간 업데이트
        lastPlayedTime[clipName] = Time.time;
    }

    public void PlaySoundWith(AudioClip _audioClip, float _volume, bool _pitch, float _coolDown)
    {
        if (_audioClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다.");
            return;
        }

        if (!CanPlaySound(_audioClip)) return; // 쿨타임이나 재생 횟수 체크
        
        // 쿨타임 체크
        string clipName = _audioClip.name;
        if (lastPlayedTime == null) lastPlayedTime = new Dictionary<string, float>();
        
        if (lastPlayedTime.ContainsKey(clipName) && Time.time - lastPlayedTime[clipName] < _coolDown)
            return;

        AudioSource audioSource = GetAudioSourceFromPool();
        if (audioSource == null)
        {
            Debug.LogWarning("사용 가능한 AudioSource를 찾을 수 없습니다.");
            return;
        }

        try
        {
            audioSource.clip = _audioClip;
            audioSource.volume = Mathf.Clamp01(_volume); // 볼륨 값 안전하게 제한

            audioSource.pitch = 1f;
            if (_pitch) audioSource.pitch = Random.Range(0.95f, 1.05f); // 피치 랜덤화
            
            audioSource.mute = isMuted;
            audioSource.Play();

            UpdateSoundPlayInfo(_audioClip); // 재생 횟수와 시간 업데이트
        }
        catch (System.Exception e)
        {
            Debug.LogError($"사운드 재생 중 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 오디오 클립을 루프로 재생
    /// </summary>
    public AudioSource PlayLoop(AudioClip audioClip, float volume = 1f)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다.");
            return null;
        }

        string clipName = audioClip.name;
        
        if (loopingSounds == null) loopingSounds = new Dictionary<string, AudioSource>();
        
        // 이미 같은 사운드가 루프 재생 중인지 확인
        if (loopingSounds.ContainsKey(clipName))
        {
            AudioSource existingSource = loopingSounds[clipName];
            // null 체크 추가
            if (existingSource != null && existingSource.isPlaying)
            {
                return existingSource;
            }
            else
            {
                // 파괴된 AudioSource 제거
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
            audioSource.loop = true; // 루프 설정
            audioSource.mute = isMuted;
            audioSource.Play();
            
            // 루프 재생 중인 사운드 목록에 추가
            loopingSounds[clipName] = audioSource;
            
            return audioSource;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"루프 사운드 재생 중 오류: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 특정 오디오 클립의 루프 재생 중단
    /// </summary>
    public void StopLoop(AudioClip audioClip)
    {
        if (audioClip == null) return;
        
        string clipName = audioClip.name;
        
        if (loopingSounds == null) return;
        
        if (loopingSounds.ContainsKey(clipName))
        {
            // 재생 중인 오디오 소스 찾아서 중단
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
            
            // 루프 재생 목록에서 제거
            loopingSounds.Remove(clipName);
        }
    }
    
    /// <summary>
    /// 모든 루프 재생 중인 사운드 중단
    /// </summary>
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
    /// <summary>
    /// 특정 AudioClip이 재생 중이라면 일시 정지합니다.
    /// (Time.timeScale이 0일 때 호출하면 효과적)
    /// </summary>
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

    /// <summary>
    /// 일시 정지된 특정 AudioClip을 다시 재생합니다.
    /// (Time.timeScale이 1로 돌아올 때 호출하면 효과적)
    /// </summary>
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

    /// <summary>
    /// 사운드를 Mute/Unmute 하는 메서드
    /// </summary>
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

    /// <summary>
    /// 모든 사운드 중단
    /// </summary>
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