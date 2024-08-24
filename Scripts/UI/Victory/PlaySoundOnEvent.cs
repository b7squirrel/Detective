using UnityEngine;

public class PlaySoundOnEvent : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioClip assetUpSound;
    [SerializeField] AudioClip resultPanelUpSound;
    [SerializeField] AudioClip stampSound;
    [SerializeField] AudioClip music;

    public void PlayAssetUpSound()
    {
        SoundManager.instance.Play(assetUpSound);
    }
    public void PlayResultPanelUpSound()
    {
        SoundManager.instance.Play(resultPanelUpSound);
    }
    public void PlayStampSound()
    {
        SoundManager.instance.Play(stampSound);
    }
    public void PlayMusic()
    {
        if (music == null) return;
        SoundManager.instance.Play(music);
    }
}