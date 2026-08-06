using System.Collections;
using UnityEngine;

public class TutorialConfetti : MonoBehaviour
{
    [SerializeField] ParticleSystem confettiEffect;
    public void PlayConfetti()
    {
        if (confettiEffect != null)
        {
            confettiEffect.gameObject.SetActive(true);
            confettiEffect.Play();

            // ✅ 파티클 재생 시간 후 자동 비활성화
            StartCoroutine(DeactivateAfterPlay(confettiEffect));
        }
    }
    IEnumerator DeactivateAfterPlay(ParticleSystem ps)
    {
        yield return new WaitForSeconds(ps.main.duration);
        ps.gameObject.SetActive(false);
    }
}
