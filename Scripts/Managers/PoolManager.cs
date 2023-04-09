using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] GameObject[] prefabs;
    [SerializeField] List<GameObject> bossPrefabs;
    [SerializeField] GameObject[] bossSpawnEffectPrefabs;
    List<GameObject>[] pools;

    void Awake()
    {
        pools = new List<GameObject>[prefabs.Length];
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
            select = Instantiate(prefabs[index], transform);
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
