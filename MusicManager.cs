using System.Collections;
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

    public void InitBGM(AudioClip _music)
    {
        MusicOnStart = _music;
        Play(MusicOnStart, true);
    }


    public void Play(AudioClip music, bool interrupt = false)
    {
        if (interrupt == true)
        {
            volume = .5f;
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

    public void ToggleMusic()
    {
        if(audioSource.volume == 0)
        {
            audioSource.volume = .5f;
        }
        else
        {
            audioSource.volume = 0;
        }
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }
}
