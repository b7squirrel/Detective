using System.Collections;
using UnityEngine;

public class LavaVolcanoSpawner : MonoBehaviour
{
    [SerializeField] GameObject volcanoPrefab; // 화산 폭발 프리펩
    [SerializeField] float spawnInterval = 3f; // 스폰 주기 (초)
    [SerializeField] int spawnCountPerInterval = 2; // 한 번에 스폰할 개수

    WallManager wallManager;
    Coroutine spawnCoroutine;

    public void StartSpawning()
    {
        wallManager = FindObjectOfType<WallManager>();

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
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (GameManager.instance.IsPlayerDead) yield break;
            if (GameManager.instance.IsPaused) continue;
            if (GameManager.instance.IsBossStage) continue;

            for (int i = 0; i < spawnCountPerInterval; i++)
            {
                Vector2 spawnPos = GetRandomSpawnPoint();
                GameManager.instance.poolManager.GetMisc(volcanoPrefab).transform.position = spawnPos;
            }
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