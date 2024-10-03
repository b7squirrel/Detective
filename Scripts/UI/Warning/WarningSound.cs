using UnityEngine;

public class WarningSound : MonoBehaviour
{
    [SerializeField] AudioClip startingSound;
    [SerializeField] AudioClip closingSound;
    [SerializeField] AudioClip idleSound;

    // 애니메이션 이벤트
    public void PlayStartingSound()
    {
        SoundManager.instance.Play(startingSound);
    }
    public void PlayClosingSound()
    {
        SoundManager.instance.Play(closingSound);
    }
    public void PlayIdleSound()
    {
        SoundManager.instance.PlayLoop(idleSound);
    }
    public void StopIdleSound()
    {
        SoundManager.instance.StopPlaying(idleSound);
    }
}
