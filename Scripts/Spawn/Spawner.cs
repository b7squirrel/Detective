using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    #region 변수
    public static Spawner instance;
    Transform[] spawnPoints;
    List<Transform> availableSpawnPoints;

    [SerializeField] Transform[] spawnObjectPoint; // 오브젝트 스폰 지점
    [SerializeField] GameObject enemyGroupShape;
    [SerializeField] int maxEnemyInScene; // 적의 수 최대치 설정
    [SerializeField] int currentEnemyNumbers; // 현재 스폰되어 있는 적의 수

    int level;
    float timer;

    WallManager wallManager;

    //[SerializeField] int numPoints;
    //[SerializeField] float circleRadius;
    #endregion

    #region 유니티 콜백 함수
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
    #endregion

    #region 적의 수
    int GetEnemyNumbers()
    {
        List<Enemy> activeEnemies = new List<Enemy>();
        Enemy[] enemies = Resources.FindObjectsOfTypeAll<Enemy>();

        for (int i = 0; i < enemies.Length; i++)
        {
            if(enemies[i].gameObject.activeSelf)
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
        if (currentEnemyNumbers >= maxEnemyInScene)
            return;
        GetAvailablePoints();

        GameObject enemy = GameManager.instance.poolManager.GetEnemy(index);
        // 벽 안쪽에서 2 unit 더 안쪽에 스폰 
        if(wallManager == null) wallManager = FindObjectOfType<WallManager>();
        float posX = Random.Range(0, 1f) > .5f ? wallManager.GetSpawnAreaConstant() - 2f : -wallManager.GetSpawnAreaConstant() + 2f;
        float posY = Random.Range(0, 1f) > .5f ? wallManager.GetSpawnAreaConstant() - 2f : -wallManager.GetSpawnAreaConstant() + 2f;
        enemy.transform.position = new Vector2(posX, posY);
        enemy.GetComponent<Enemy>().Init(enemyToSpawn);

        AddEnemyNumber();
    }
    public void SpawnEnemyGroup(EnemyData enemyToSpawn, int index, int numberOfEnemies)
    {
        if (currentEnemyNumbers >= maxEnemyInScene)
            return;
        GetAvailablePoints();
        Vector2 spawnPoint = availableSpawnPoints[Random.Range(1, availableSpawnPoints.Count)].position;
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
    //public Vector2 GetRandomSpawnPoint()
    //{
    //    // 랜덤한 반지름과 각도 생성
    //    float r = circleRadius * Mathf.Sqrt(Random.value);
    //    float theta = Random.value * 2 * Mathf.PI;

    //    // 극좌표를 직교좌표로 변환
    //    float x = r * Mathf.Cos(theta);
    //    float y = r * Mathf.Sin(theta);

    //    return new Vector2(x, y);
    //}

    //public void InitSpawnPoints(int _numPoints, float _radius)
    //{
    //    numPoints = _numPoints;
    //    circleRadius = _radius;
    //}
    #endregion
}
