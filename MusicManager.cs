using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    public AudioClip MusicOnStart;

    AudioSource audioSource;
    AudioClip switchTo;

    float volume = 1f;
    [SerializeField] float timeToSwitch;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        Play(MusicOnStart, true);
    }

    public void Play(AudioClip music, bool interrupt = false)
    {
        if (interrupt == true)
        {
            volume = 1f;
            audioSource.volume = volume;
            audioSource.clip = music;
            audioSource.Play();
        }
        else
        {
            switchTo = music;
            StartCoroutine(SmoothSwitchMusic());
        }
    }
    public void Stop()
    {
        audioSource.Stop();
    }

    IEnumerator SmoothSwitchMusic()
    {
        while (volume > 0f)
        {
            volume -= Time.deltaTime / timeToSwitch;
            if (volume < 0f)
            {
                volume = 0f;
            }
            audioSource.volume= volume;
            yield return new WaitForEndOfFrame();
        }

        Play(switchTo, true);
    }
}
