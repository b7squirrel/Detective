using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] GameObject[] enemies;
    [SerializeField] List<GameObject> bossPrefabs;
    [SerializeField] GameObject[] bossSpawnEffectPrefabs;
    List<GameObject>[] pools;

    StageAssetManager stageAssetManager;

    void Start()
    {
        stageAssetManager = FindAnyObjectByType<StageAssetManager>();

        this.enemies = new GameObject[stageAssetManager.enemies.Length];

        for (int i = 0; i < this.enemies.Length; i++)
        {
            this.enemies[i] = stageAssetManager.enemies[i];
        }

        pools = new List<GameObject>[this.enemies.Length];
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<GameObject>();
        }
    }

    public GameObject Get(int index)
    {
        GameObject select = null;

        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                select = item;
                select.SetActive(true);
                break;
            }
        }

        if (!select) 
        {
            select = Instantiate(enemies[index], transform);
            pools[index].Add(select);
        }

        return select;
    }

    public GameObject GetBoss(EnemyData enemyData)
    {
        GameObject boss = null;
        boss = bossPrefabs.Find(x => x.GetComponent<EnemyBase>().Name == enemyData.Name);
        return boss;
    }
    public GameObject GetBossSpawnEffect(int index, Vector2 spawnPos)
    {
        GameObject effect = Instantiate(bossSpawnEffectPrefabs[index], spawnPos, Quaternion.identity);
        effect.transform.parent = transform;
        return effect;
    }
}
