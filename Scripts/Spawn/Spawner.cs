using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    #region 변수
    public static Spawner instance;

    [SerializeField] GameObject enemyGroupShape;
    [SerializeField] int maxEnemyInScene; // 적의 수 최대치 설정
    [SerializeField] int currentEnemyNumbers; // 현재 스폰되어 있는 적의 수

    int level;
    float timer;

    WallManager wallManager;
    EnemyFinder enemyFinder;
    #endregion

    #region 유니티 콜백 함수
    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (GameManager.instance.IsPlayerDead)
            return;
    }
    #endregion

    #region 적의 수
    int GetEnemyNumbers()
    {
        List<Enemy> activeEnemies = new List<Enemy>();
        Enemy[] enemies = Resources.FindObjectsOfTypeAll<Enemy>();

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].gameObject.activeSelf)
            {
                activeEnemies.Add(enemies[i]);
            }
        }

        if (activeEnemies == null)
            return 0;
        return activeEnemies.Count;
    }
    void AddEnemyNumber()
    {
        currentEnemyNumbers++;
    }
    public void SubtractEnemyNumber()
    {
        currentEnemyNumbers--;
    }
    public int GetCurrentEnemyNums()
    {
        return currentEnemyNumbers;
    }
    #endregion

    #region 스폰
    public void Spawn(EnemyData enemyToSpawn, int index)
    {
        //if (currentEnemyNumbers >= maxEnemyInScene)
        //    return;
        GetAvailablePoints();

        GameObject enemy = GameManager.instance.poolManager.GetEnemy(index);
        // 벽 안쪽에서 2 unit 더 안쪽에 스폰

        Vector2 spawnPoint = GetAvailablePoints();
        
        enemy.transform.position = new Vector2(spawnPoint.x, spawnPoint.y);
        enemy.GetComponent<Enemy>().Init(enemyToSpawn);

        AddEnemyNumber();
    }
    public void SpawnEnemyGroup(EnemyData enemyToSpawn, int index, int numberOfEnemies)
    {
        if (currentEnemyNumbers >= maxEnemyInScene)
            return;
        Vector2 spawnPoint = GetAvailablePoints();
        GameObject groupShape = Instantiate(enemyGroupShape, spawnPoint, Quaternion.identity);
        groupShape.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360f));
        Vector2 groupDir = ((Vector2)Player.instance.transform.position - spawnPoint).normalized;

        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject enemy = GameManager.instance.poolManager.GetEnemy(index);
            enemy.transform.position = groupShape.GetComponent<EnemyGroupShape>().SpawnPoints[i].position;
            enemy.GetComponent<Enemy>().Init(enemyToSpawn);
            enemy.GetComponent<Enemy>().IsGrouping = true;
            enemy.GetComponent<Enemy>().GroupDir = groupDir;

            AddEnemyNumber();
        }
        Destroy(groupShape);
    }

    public void SpawnEnemiesToShoot(EnemyData enemyToSpawn, int index, Vector2 start, Vector2 target)
    {
        GameObject enemy = GameManager.instance.poolManager.GetEnemy(index);
        enemy.transform.position = start;
        enemy.GetComponent<Enemy>().Init(enemyToSpawn);
        enemy.GetComponent<Enemy>().SetFlying(target);
    }

    IEnumerator SpawnBossCo(EnemyData enemyToSpawn)
    {
        Vector2 spawnPoint = new GeneralFuctions().GetRandomPositionFrom(Player.instance.transform.position, 3f, 10f);
        GameObject enemy = Instantiate(GameManager.instance.poolManager.GetBoss(enemyToSpawn), GameManager.instance.poolManager.transform);
        enemy.transform.position = spawnPoint;
        EnemyBoss boss = enemy.GetComponent<EnemyBoss>();
        enemy.SetActive(false);

        GameManager.instance.bossWarningPanel.Init(boss.Name);
        yield return new WaitForSecondsRealtime(2f);

        GameManager.instance.poolManager.GetBossSpawnEffect(0, spawnPoint);
        yield return new WaitForSeconds(1.45f);
        enemy.SetActive(true);
        boss.Init(enemyToSpawn);
    }

    public void SpawnBoss(EnemyData enemyToSpawn)
    {
        StartCoroutine(SpawnBossCo(enemyToSpawn));
    }
    #endregion

    #region 스폰 포인트
    Vector2 GetAvailablePoints()
    {
        // 벽 안쪽에서 2 unit 더 안쪽에 스폰
        if (wallManager == null) wallManager = FindObjectOfType<WallManager>();
        float spawnConst = wallManager.GetSpawnAreaConstant();
        float offset = 2f;

        Vector2 position = new Equation().GetSpawnablePos(spawnConst, offset);

        return position;
    }
    #endregion
}
