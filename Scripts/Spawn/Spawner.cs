using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner instance;
    Transform[] spawnPoints;
    List<Transform> availableSpawnPoints;

    [SerializeField] Transform[] spawnObjectPoint;

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
        // getcomponentChildren으로 받아왔으므로 0부터 하면 Player의 위치까지 포함하게 되므로
        enemy.transform.position = availableSpawnPoints[Random.Range(1, availableSpawnPoints.Count)].position; 
        enemy.GetComponent<Enemy>().Init(enemyToSpawn);
    }

    public void SpawnEnemiesToShoot(EnemyData enemyToSpawn, int index, Vector2 start, Vector2 target)
    {
        GameObject enemy = GameManager.instance.poolManager.Get(index);
        enemy.transform.position = start;
        enemy.GetComponent<Enemy>().Init(enemyToSpawn);
        enemy.GetComponent<Enemy>().SetFlying(target);
    }

    public void SpawnBoss(EnemyData enemyToSpawn)
    {
        GetAvailablePoints();
        
        GameObject enemy = Instantiate(GameManager.instance.poolManager.GetBoss(enemyToSpawn), GameManager.instance.poolManager.transform) ;
        enemy.GetComponent<EnemyBoss>().Init(enemyToSpawn);
    }

    public void SpawnObject(GameObject toSpawn)
    {
        Transform pickUP = Instantiate(toSpawn).transform;
        pickUP.position = spawnObjectPoint[Random.Range(0, spawnObjectPoint.Length)].position;
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
