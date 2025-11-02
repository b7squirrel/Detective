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
        if (initSound == null) return;
        SoundManager.instance.PlaySoundWith(initSound, 1f, true, 0.4f);
    }
    
    // 랜딩 사운드는 shadowHeight 함수에서 유니티 이벤트로 실행하고 있음
    public void PlayLandingSound()
    {
        if (landingSound == null) return;
        SoundManager.instance.PlaySoundWith(landingSound, 1f, true, 0f);
    }

}