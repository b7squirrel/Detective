using UnityEngine;

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
        SoundManager.instance.Play(initSound);
    }
    public void PlayLandingSound() 
    {
        SoundManager.instance.Play(landingSound);
    }

}