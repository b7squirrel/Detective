using System.Collections;
using UnityEngine;

public class FireballManager : MonoBehaviour
{
    [Header("스폰 주기")]
    [SerializeField] float minInterval = 3f;
    [SerializeField] float maxInterval = 6f;

    [Header("한 번에 떨어지는 개수")]
    [SerializeField] int minCount = 1;
    [SerializeField] int maxCount = 3;

    [Header("인디케이터 → 파이어볼 딜레이")]
    [SerializeField] float warningDuration = 1.5f; // 인디케이터 표시 시간

    [Header("프리펩")]
    [SerializeField] GameObject indicatorPrefab;
    [SerializeField] GameObject fireballPrefab;

    WallManager wallManager;
    Coroutine spawnCoroutine;

    public void StartSpawning()
    {
        wallManager = FindObjectOfType<WallManager>();
        Logger.Log("[FireballManager] StartSpawning 호출됨");

        if (wallManager == null)
            Logger.LogError("[FireballManager] WallManager를 찾을 수 없습니다");

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnCo());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnCo()
    {
        Logger.Log("[FireballManager] SpawnCo 시작");
        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            Logger.Log($"[FireballManager] {wait}초 후 파이어볼 스폰");
            yield return new WaitForSeconds(wait); // 하나만 남김

            if (GameManager.instance.IsPlayerDead) yield break;
            if (GameManager.instance.IsPaused) continue;
            if (GameManager.instance.IsBossStage) continue;

            int count = Random.Range(minCount, maxCount + 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = GetRandomSpawnPoint();
                StartCoroutine(SpawnFireballWithWarning(pos));
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    IEnumerator SpawnFireballWithWarning(Vector2 pos)
    {
        FireballIndicator fireballIndicator = null;
        GameObject indicator = GameManager.instance.poolManager.GetMisc(indicatorPrefab);
        if (indicator != null)
        {
            indicator.transform.position = pos;
            fireballIndicator = indicator.GetComponent<FireballIndicator>();
            fireballIndicator.Show(warningDuration);
        }

        yield return new WaitForSeconds(warningDuration);

        GameObject fireball = GameManager.instance.poolManager.GetMisc(fireballPrefab);
        if (fireball != null)
        {
            // 위치는 FallCo 안에서 설정하므로 여기서는 넘기지 않음
            fireball.GetComponent<Fireball>().Init(pos, fireballIndicator);
        }
    }

    Vector2 GetRandomSpawnPoint()
    {
        float spawnConst = wallManager.GetSpawnAreaConstant();
        float offset = 2f;
        return new Vector2(
            Random.Range(-spawnConst + offset, spawnConst - offset),
            Random.Range(-spawnConst + offset, spawnConst - offset)
        );
    }
}