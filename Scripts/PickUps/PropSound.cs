using UnityEngine;

/// <summary>
/// 아이템이 드롭되는 순간 사운드, 착지 사운드 플레이
/// </summary>
public class PropSound : MonoBehaviour
{
    [SerializeField] AudioClip initSound;
    [SerializeField] AudioClip landingSound;

    void OnEnable()
    {
        PlayInitSound();
    }

    public void PlayInitSound()
    {
        SoundManager.instance.PlaySoundWith(initSound, 1f, true, 0f);
    }
    public void PlayLandingSound() 
    {
        SoundManager.instance.PlaySoundWith(landingSound, 1f, true, 0f);
    }

}