using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] GameObject audioSourcePrefab;
    [SerializeField] int audioSourceCount;
    [SerializeField] float reductionRate;
    List<AudioSource> audioSources;

    void Awake()
    {
        instance= this;
    }

    private void Start()
    {
        Init();
    }

    void Init()
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
        audioSource.clip = audioClip;
        
        audioSource.Play();
    }

    AudioSource GetAudio()
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].isPlaying == false)
            {
                audioSources[i].volume = 1f;
                audioSources[i].volume = Mathf.Pow(reductionRate, i);
                return audioSources[i];
            }
        }

        return audioSources[0];
    }
}
