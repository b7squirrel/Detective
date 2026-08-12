using System;
using System.Collections;
using UnityEngine;

public class TeleportEffect : MonoBehaviour
{
    [SerializeField] GameObject teleportEffectPrefab;
    [SerializeField] GameObject teleportOutEffectPrefab;
    [SerializeField] GameObject teleportUpEffectPrefab;
    [SerializeField] AudioClip teleportSound;
    [SerializeField] AudioClip teleportOutSound;
    ParticleSystem particleSys;

    public void GenTeleportEffect(Vector2 _spawnPos)
    {
        GameObject teleEffect = Instantiate(teleportEffectPrefab, _spawnPos, Quaternion.identity);
        SoundManager.instance.Play(teleportSound);
    }

    public void GenTeleportOutEffect(Vector2 _spawnPos, Action onVisualHide = null, Action onComplete = null)
    {
        StartCoroutine(GenTeleportOutEffectCo(_spawnPos, onVisualHide, onComplete));
    }
    IEnumerator GenTeleportOutEffectCo(Vector2 _spawnPos, Action onVisualHide, Action onComplete)
    {
        GameObject teleUpEffect = Instantiate(teleportUpEffectPrefab, _spawnPos, Quaternion.identity);
        particleSys = teleUpEffect.GetComponentInChildren<ParticleSystem>();
        particleSys.Play();

        SoundManager.instance.Play(teleportOutSound);
        SoundManager.instance.Play(teleportOutSound);

        yield return new WaitForSeconds(.3f);
        GameObject teleEffect = Instantiate(teleportOutEffectPrefab, _spawnPos, Quaternion.identity);
        CameraShake.instance.Shake();

        TeleportOutAnimEvents animEvents = teleEffect.GetComponent<TeleportOutAnimEvents>();
        if (animEvents != null)
        {
            bool animFinished = false;

            // ⭐ 변경: "거의 끝날 때" 이벤트가 오면 바로 onVisualHide 호출 (완전히 끝날 때까지 안 기다림)
            animEvents.OnAnimationNearlyFinished += () => onVisualHide?.Invoke();
            animEvents.OnAnimationFinished += () => animFinished = true;

            while (!animFinished)
            {
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(.83f);
            // ⭐ 폴백 상황에서는 이벤트가 없으니 애니메이션이 끝난 시점에 바로 숨김 처리
            onVisualHide?.Invoke();
        }

        yield return new WaitForSeconds(.5f);
        particleSys.Stop();

        onComplete?.Invoke();
    }
}