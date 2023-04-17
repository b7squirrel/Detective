using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner instance;
    Transform[] spawnPoints;
    List<Transform> availableSpawnPoints;

    [SerializeField] Transform[] spawnObjectPoint; // 오브젝트 스폰 지점
    [SerializeField] GameObject enemyGroupShape;
    [SerializeField] int maxEnemyInScene;

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
    int GetEnemyNumbers()
    {
        List<Enemy> activeEnemies = new List<Enemy>();
        Enemy[] enemies = Resources.FindObjectsOfTypeAll<Enemy>();
        foreach (var item in enemies)
        {
            if(item.gameObject.activeSelf)
            {
                activeEnemies.Add(item);
            }
        }
        if (activeEnemies == null)
            return 0;
            // Debug.Log("Enemy numbers = " + activeEnemies.Count);
        return activeEnemies.Count;
    }

    public void Spawn(EnemyData enemyToSpawn, int index)
    {
        if (GetEnemyNumbers() > maxEnemyInScene)
            return;
        GetAvailablePoints();

        GameObject enemy = GameManager.instance.poolManager.Get(index);
        // getcomponentChildren으로 받아왔으므로 0부터 하면 Player의 위치까지 포함하게 되므로
        enemy.transform.position = availableSpawnPoints[Random.Range(1, availableSpawnPoints.Count)].position;
        enemy.GetComponent<Enemy>().Init(enemyToSpawn);
    }
    public void SpawnEnemyGroup(EnemyData enemyToSpawn, int index, int numberOfEnemies)
    {
        if (GetEnemyNumbers() > maxEnemyInScene)
            return;
        GetAvailablePoints();
        Vector2 spawnPoint = availableSpawnPoints[Random.Range(1, availableSpawnPoints.Count)].position;
        GameObject groupShape = Instantiate(enemyGroupShape, spawnPoint, Quaternion.identity);
        groupShape.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360f));
        Vector2 groupDir = ((Vector2)Player.instance.transform.position - spawnPoint).normalized;

        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject enemy = GameManager.instance.poolManager.Get(index);
            enemy.transform.position = groupShape.GetComponent<EnemyGroupShape>().SpawnPoints[i].position;
            enemy.GetComponent<Enemy>().Init(enemyToSpawn);
            enemy.GetComponent<Enemy>().IsGrouping = true;
            enemy.GetComponent<Enemy>().GroupDir = groupDir;
        }
        Destroy(groupShape);
    }

    public void SpawnEnemiesToShoot(EnemyData enemyToSpawn, int index, Vector2 start, Vector2 target)
    {
        GameObject enemy = GameManager.instance.poolManager.Get(index);
        enemy.transform.position = start;
        enemy.GetComponent<Enemy>().Init(enemyToSpawn);
        enemy.GetComponent<Enemy>().SetFlying(target);
    }

    IEnumerator SpawnBossCo(EnemyData enemyToSpawn)
    {
        Vector2 spawnPoint = new GeneralFuctions().GetRandomPositionFrom(Player.instance.transform.position, 3f, 10f);
        GameManager.instance.poolManager.GetBossSpawnEffect(0, spawnPoint);
        yield return new WaitForSeconds(.25f);

        GameObject enemy = Instantiate(GameManager.instance.poolManager.GetBoss(enemyToSpawn), GameManager.instance.poolManager.transform) ;
        enemy.transform.position = spawnPoint;
        GameManager.instance.GetComponent<BossHealthBarManager>().ActivateBossHealthBar(); // Init에서 bossHealth바를 참조하므로 Init보다 앞에 위치
        EnemyBoss boss = enemy.GetComponent<EnemyBoss>();
        boss.Init(enemyToSpawn);
    }

    public void SpawnBoss(EnemyData enemyToSpawn)
    {
        StartCoroutine(SpawnBossCo(enemyToSpawn));
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
    public Vector2 GetRandomSpawnPoint()
    {
        int index = Random.Range(1, spawnPoints.Length); // 플레이어가 포함되지 않게 1부터
        Vector2 point = spawnPoints[index].position;
        return point;
    }
}
