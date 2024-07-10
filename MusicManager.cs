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

    bool isMuted;
    [SerializeField] float defautVolume = .5f;

    private void Awake()
    {
        instance = this;
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
            volume = defautVolume;
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
        isMuted = !isMuted;
        if (isMuted)
        {
            audioSource.volume = 0f;
        }
        else
        {
            audioSource.volume = defautVolume;
        }
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }

    public void SetState(bool _state)
    {
        isMuted = _state;
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.mute = isMuted;
    }
}
