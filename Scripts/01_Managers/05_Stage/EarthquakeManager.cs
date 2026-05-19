using System.Collections;
using UnityEngine;

public class EarthquakeManager : MonoBehaviour
{
    [Header("지진 주기")]
    [SerializeField] float minInterval = 5f;  // 최소 대기 시간
    [SerializeField] float maxInterval = 10f; // 최대 대기 시간

    [Header("지진 지속 시간")]
    [SerializeField] float earthquakeDuration = 2f;

    [Header("지진 예고 시간")]
    [SerializeField] float warningTime = 0.8f; // 사운드 먼저, 그 후 흔들림

    [Header("지진 진동 크기")]
    [SerializeField] float minShakeRange = 0.05f;  // 시작 진폭
    [SerializeField] float maxShakeRange = 0.5f;   // 최대 진폭

    [Header("속도 감소")]
    [SerializeField] float slowFactorDuringQuake = 0.3f; // 지진 중 이동속도 배율

    [Header("사운드")]
    [SerializeField] AudioClip rumbleSound;
    [SerializeField][Range(0f, 1f)] float rumbleVolume = 0.7f;

    Player player;
    Coroutine earthquakeCoroutine;

    public void StartEarthquake()
    {
        player = FindObjectOfType<Player>();

        if (earthquakeCoroutine != null)
            StopCoroutine(earthquakeCoroutine);

        earthquakeCoroutine = StartCoroutine(EarthquakeCo());
    }

    public void StopEarthquake()
    {
        if (earthquakeCoroutine != null)
        {
            StopCoroutine(earthquakeCoroutine);
            earthquakeCoroutine = null;
        }
        player?.ResetSlowDownFactor();
    }

    IEnumerator EarthquakeCo()
    {
        while (true)
        {
            // 다음 지진까지 대기
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            if (GameManager.instance.IsPlayerDead) yield break;
            if (GameManager.instance.IsBossStage) continue;

            // 지진 발생
            yield return StartCoroutine(DoEarthquake());
        }
    }

    IEnumerator DoEarthquake()
    {
        // 사운드 먼저 재생 (지진 예고)
        if (rumbleSound != null)
            SoundManager.instance.PlayLoop(rumbleSound, rumbleVolume);

        yield return new WaitForSeconds(warningTime);

        // 속도 감소
        player?.SetSlowDownFactor(slowFactorDuringQuake);

        // 카메라 쉐이크
        float elapsed = 0f;
        while (elapsed < earthquakeDuration)
        {
            if (!GameManager.instance.IsPaused)
            {
                CameraShake.instance.Shake();
                elapsed += Time.deltaTime;
            }
            yield return null;
        }

        // 지진 종료
        if (rumbleSound != null)
            SoundManager.instance.StopLoop(rumbleSound);

        player?.ResetSlowDownFactor();
    }
}