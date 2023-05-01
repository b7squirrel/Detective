using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] GameObject[] enemies;
    [SerializeField] List<GameObject> bossPrefabs;
    [SerializeField] GameObject[] bossSpawnEffectPrefabs;
    List<GameObject>[] enemyPools;

    StageAssetManager stageAssetManager;

    Dictionary<string, List<GameObject>> miscPools;

    void Start()
    {
        InitEnemyPools();
    }

    void InitEnemyPools()
    {
        stageAssetManager = FindAnyObjectByType<StageAssetManager>();

        this.enemies = new GameObject[stageAssetManager.enemies.Length];

        for (int i = 0; i < this.enemies.Length; i++)
        {
            this.enemies[i] = stageAssetManager.enemies[i];
        }

        enemyPools = new List<GameObject>[this.enemies.Length];
        for (int i = 0; i < enemyPools.Length; i++)
        {
            enemyPools[i] = new List<GameObject>();
        }
    }

    #region GetEnemy
    public GameObject GetEnemy(int index)
    {
        GameObject select = null;

        foreach (GameObject item in enemyPools[index])
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
            enemyPools[index].Add(select);
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
        // 보스 등장 이펙트
        GameObject effect = Instantiate(bossSpawnEffectPrefabs[index], spawnPos, Quaternion.identity);
        effect.transform.parent = transform;
        return effect;
    }
    #endregion

    #region GetMisc
    // effects, weapons, sounds는 tag를 이용해서 pooling
    public GameObject GetMisc(GameObject prefab)
    {
        GameObject select = null;

        if (miscPools == null) miscPools = new Dictionary<string, List<GameObject>>();

        string poolingTag = prefab.GetComponent<PoolingKey>().Key;

        if (miscPools.ContainsKey(poolingTag)) // 해당 key의 pool이 있다면
        {
            foreach (var item in miscPools[poolingTag])
            {
                if (!item.activeSelf)
                {
                    select = item;
                    select.SetActive(true);
                    return select;
                }
            }
        }
        else // 해당 key의 pool이 없다면
        {
            List<GameObject> go = new List<GameObject>();

            select = Instantiate(prefab, transform);
            go.Add(select);
            miscPools.Add(poolingTag, go);
            return select;
        }

        // pool안의 오브젝트가 모두 사용중이라면 
        select = Instantiate(prefab, transform);
        miscPools[poolingTag].Add(select);

        return select;
    }
    #endregion
}
