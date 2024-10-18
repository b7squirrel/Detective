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
        // 피치 변화 없이 재생, 반드시 재생
        SoundManager.instance.Play(idleSound);
    }
}
