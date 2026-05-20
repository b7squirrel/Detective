using System.Collections;
using UnityEngine;

public class DesertWindManager : MonoBehaviour
{
    [Header("바람 세기")]
    [SerializeField] float minWindStrength = 1f;
    [SerializeField] float maxWindStrength = 3f;

    [Header("바람 지속 시간")]
    [SerializeField] float minWindDuration = 3f;
    [SerializeField] float maxWindDuration = 6f;

    [Header("바람 전환 시간 (서서히 바뀌는 속도)")]
    [SerializeField] float windTransitionTime = 1.5f;

    [Header("파티클")]
    [SerializeField] ParticleSystem windParticle;
    [SerializeField] ParticleSystem sandDustParticle;
    [SerializeField] ParticleSystem sandFogParticle; 

    [Header("사운드")]
    [SerializeField] AudioClip windAmbientSound;
    [SerializeField][Range(0f, 1f)] float windSoundVolume = 0.4f;

    Player player;
    Coroutine windCoroutine;

    Vector2 currentWind = Vector2.zero;
    Vector2 targetWind = Vector2.zero;

    public void StartWind()
    {
        player = FindObjectOfType<Player>();

        if (windCoroutine != null)
            StopCoroutine(windCoroutine);

        windCoroutine = StartCoroutine(WindCo());
        StartCoroutine(WindTransitionCo());

        // ✅ 파티클 재생 추가
        if (windParticle != null && !windParticle.isPlaying)
            windParticle.Play();
        if (sandDustParticle != null && !sandDustParticle.isPlaying)
            sandDustParticle.Play();
        if (sandFogParticle != null && !sandFogParticle.isPlaying)
            sandFogParticle.Play();

        // 바람 소리 루프 재생
        if (windAmbientSound != null)
            SoundManager.instance.PlayLoop(windAmbientSound, windSoundVolume);
    }

    public void StopWind()
    {
        if (windCoroutine != null)
        {
            StopCoroutine(windCoroutine);
            windCoroutine = null;
        }
        currentWind = Vector2.zero;
        player?.SetWindForce(Vector2.zero);

        // ✅ 파티클 정지 추가
        if (windParticle != null) windParticle.Stop();
        if (sandDustParticle != null) sandDustParticle.Stop();
        if (sandFogParticle != null) sandFogParticle.Stop();

        // 바람 소리 정지
        if (windAmbientSound != null)
            SoundManager.instance.StopLoop(windAmbientSound);
    }

    // 일정 주기로 바람 방향/세기를 새로 결정
    IEnumerator WindCo()
    {
        while (true)
        {
            Vector2[] directions = {
            Vector2.right,
            Vector2.left
        };

            Vector2 dir = directions[Random.Range(0, directions.Length)];
            float strength = Random.Range(minWindStrength, maxWindStrength);
            targetWind = dir * strength;

            UpdateParticleDirection(dir);

            float duration = Random.Range(minWindDuration, maxWindDuration);
            yield return new WaitForSeconds(duration);
        }
    }

    // currentWind를 targetWind로 서서히 보간
    IEnumerator WindTransitionCo()
    {
        while (true)
        {
            if (GameManager.instance.IsPaused)
            {
                yield return null;
                continue;
            }

            currentWind = Vector2.Lerp(currentWind, targetWind,
                Time.deltaTime / windTransitionTime);

            player?.SetWindForce(currentWind);

            yield return null;
        }
    }

    void UpdateParticleDirection(Vector2 dir)
    {
        if (windParticle != null)
        {
            var vel = windParticle.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = dir.x > 0 ? 15f : -15f;
            vel.y = 0f;
            vel.z = 0f;
        }

        // 모래 먼지는 바람보다 느리게
        if (sandDustParticle != null)
        {
            var vel = sandDustParticle.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = dir.x > 0 ? 25f : -25f;
            vel.y = 0f;
            vel.z = 0f;
        }
        // 안개는 아주 느리게
        if (sandFogParticle != null)
        {
            var vel = sandFogParticle.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = dir.x > 0 ? 15f : -15f;
            vel.y = 0f;
            vel.z = 0f;
        }
    }
}