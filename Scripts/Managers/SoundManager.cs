using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] GameObject audioSourcePrefab;
    [SerializeField] int audioSourceCount;
    [SerializeField] float reductionRate;
    List<AudioSource> audioSources;
    AudioClip singleSound; // 한 번만 재생되는 사운드를 리스트에서 빼기 위해

    private bool isMuted; // 현재 Mute 상태를 추적하기 위한 변수

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (singleSound == null)
            return;
        RemoveSingleAudio(singleSound);
    }

    public void Init()
    {
        audioSources = new List<AudioSource>();

        for (int i = 0; i < audioSourceCount; i++)
        {
            GameObject go = Instantiate(audioSourcePrefab, transform);
            go.transform.localPosition = Vector2.zero;
            audioSources.Add(go.GetComponent<AudioSource>());
        }
    }

    public void Play(AudioClip audioClip)
    {
        AudioSource audioSource = GetAudio();
        if (audioSource == null) return;
        audioSource.clip = audioClip;
        audioSource.pitch = UnityEngine.Random.Range(1f, 1.1f);
        audioSource.mute = isMuted; // 현재 Mute 상태에 따라 설정

        audioSource.Play();
    }

    public void PlaySingle(AudioClip audioClip)
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].clip == null)
                continue;
            if (audioSources[i].clip.name == audioClip.name)
            {
                return; // 클립이 이미 재생 중이면 함수 종료
            }
        }

        Play(audioClip); // 클립이 재생 중이지 않으면 Play 함수 호출
        singleSound = audioClip; // 현재 재생 중인 클립을 singleSound에 저장
    }

    AudioSource GetAudio()
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i] == null) return null;
            if (audioSources[i].isPlaying == false)
            {
                audioSources[i].volume = 1f;
                audioSources[i].volume = Mathf.Pow(reductionRate, i);
                return audioSources[i];
            }
        }

        return audioSources[0];
    }

    void RemoveSingleAudio(AudioClip audioClip)
    {
        int index = GetIndex(audioClip);
        if (index == -1) return;

        if (audioSources[index].isPlaying) return;

        audioSources[index].clip = null;
        singleSound = null;
    }

    int GetIndex(AudioClip audioClip)
    {
        if (audioClip == null) return -1;
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].clip == null) continue;
            if (audioSources[i].clip.name == audioClip.name)
                return i;
        }
        return -1;
    }
    // 추가: 독립적인 AudioSource 객체 생성 및 관리
    public void PlayAtPosition(AudioClip audioClip, Vector3 position)
    {
        GameObject tempAudioSource = new GameObject("TempAudio");
        tempAudioSource.transform.position = position;
        AudioSource audioSource = tempAudioSource.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.mute = isMuted; // 현재 Mute 상태에 따라 설정
        audioSource.Play();
        Destroy(tempAudioSource, audioClip.length);
    }

    // 사운드를 Mute/Unmute 하는 메서드 추가
    public void ToggleMute()
    {
        isMuted = !isMuted;

        foreach (var audioSource in audioSources)
        {
            if (audioSource != null)
            {
                audioSource.mute = isMuted;
            }
        }
    }

    public void SetState(bool _state)
    {
        isMuted = _state;
        foreach (var audioSource in audioSources)
        {
            if (audioSource != null)
            {
                audioSource.mute = isMuted;
            }
        }
    }
}