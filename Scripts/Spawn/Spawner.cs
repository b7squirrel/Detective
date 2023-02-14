using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner instance;
    Transform[] spawnPoints;
    List<Transform> availableSpawnPoints;

    int level;
    float timer;


    void Awake()
    {
        instance = this;
        spawnPoints = GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        if (GameManager.instance.IsPlayerDead)
            return;
    }

    public void Spawn(EnemyData enemyToSpawn, int index)
    {
        GetAvailablePoints();

        GameObject enemy = GameManager.instance.poolManager.Get(index);
        enemy.transform.position = availableSpawnPoints[Random.Range(1, availableSpawnPoints.Count)].position;
        enemy.GetComponent<Enemy>().Init(enemyToSpawn);
    }

    public void SpawnObject(Vector2 worldPosition, GameObject toSpawn)
    {
        Transform pickUP = Instantiate(toSpawn).transform;
        pickUP.position = worldPosition;
    }

    void GetAvailablePoints()
    {
        if (availableSpawnPoints == null)
        {
            availableSpawnPoints = new List<Transform>();
        }

        availableSpawnPoints.Clear();

        for (int i = 1; i < spawnPoints.Length - 1; i++)
        {
            if (spawnPoints[i].GetComponent<SpawnPoint>().IsAvailable)
            {
                availableSpawnPoints.Add(spawnPoints[i]);
            }
        }
    }
}
