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

    [Header("보스 랜딩 두들 이펙트")]
    [SerializeField] GameObject BossLandingDoodleIndicator;

    int level;
    float timer;

    WallManager wallManager;
    Equation equation = new Equation(); // GetAvailablePoints에서 매번 new 하지 않도록 캐싱
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
    void AddEnemyNumber()
    {
        currentEnemyNumbers++;
    }
    public void SubtractEnemyNumber()
    {
        Logger.Log($"[Spawner] SubtractEnemyNumber 호출 - currentEnemyCount={currentEnemyNumbers}");
        currentEnemyNumbers--;
    }
    public int GetCurrentEnemyNums()
    {
        return currentEnemyNumbers;
    }
    #endregion

    #region 스폰
    public void Spawn(EnemyData enemyToSpawn, int index, bool forceSpawn)
    {
        // 적들이 몰려옵니다의 경우 강제로 스폰
        if (forceSpawn == false)
        {
            if (currentEnemyNumbers >= maxEnemyInScene) // 최대 적 수 제한
                return;
        }

        GameObject enemy = GameManager.instance.poolManager.GetEnemy(index);
        Vector2 spawnPoint = GetAvailablePoints();
        enemy.transform.position = new Vector2(spawnPoint.x, spawnPoint.y);

        enemy.GetComponent<EnemyBase>().InitEnemy(enemyToSpawn);

        AddEnemyNumber();
    }

    /// <summary>
    /// SubBoss 전용 스폰 — subBossPools 사용
    /// </summary>
    public void SpawnSubBossEnemy(EnemyData enemyToSpawn, bool forceSpawn)
    {
        if (!forceSpawn && currentEnemyNumbers >= maxEnemyInScene)
            return;

        // ✅ 이름 매칭 제거, 항상 index 0 사용
        GameObject enemy = GameManager.instance.poolManager.GetSubBossEnemy(0);
        if (enemy == null) return;

        enemy.transform.position = GetAvailablePoints();
        enemy.GetComponent<EnemyBase>().InitEnemy(enemyToSpawn);

        AddEnemyNumber();
    }

    /// <summary>
    /// 무한 모드 전용 스폰 (maxEnemyInScene 제한 무시)
    /// </summary>
    public void SpawnForInfiniteMode(EnemyData enemyToSpawn, int index, bool isBoss = false)
    {
        GameObject enemy = GameManager.instance.poolManager.GetEnemy(index);
        Vector2 spawnPoint = GetAvailablePoints();
        enemy.transform.position = new Vector2(spawnPoint.x, spawnPoint.y);

        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        enemyBase.InitEnemy(enemyToSpawn);

        if (isBoss)
        {
            enemyBase.IsBoss = true;
            Logger.Log($"[Spawner] Boss spawned in infinite mode: {enemyToSpawn.Name}");
        }

        AddEnemyNumber();
    }

    public void SpawnSplit(EnemyData enemyToSpawn, int index, bool forceSpawn, Vector2 spawnPos)
    {
        if (forceSpawn == false)
        {
            if (currentEnemyNumbers >= maxEnemyInScene)
                return;
        }

        GameObject enemy = GameManager.instance.poolManager.GetEnemy(index);
        Vector2 offset = new Vector2(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f));
        enemy.transform.position = spawnPos + offset;

        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        enemyBase.InitEnemy(enemyToSpawn);
        enemyBase.SetIsSplited(true);

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
            Enemy enemyComp = enemy.GetComponent<Enemy>(); // GetComponent 한 번만 호출
            enemy.transform.position = groupShape.GetComponent<EnemyGroupShape>().SpawnPoints[i].position;
            enemyComp.InitEnemy(enemyToSpawn);
            enemyComp.IsGrouping = true;
            enemyComp.GroupDir = groupDir;

            AddEnemyNumber();
        }
        Destroy(groupShape);
    }

    public void SpawnEnemiesToShoot(EnemyData enemyToSpawn, int index, Vector2 start, Vector2 target)
    {
        GameObject enemy = GameManager.instance.poolManager.GetEnemy(index);
        enemy.transform.position = start;
        enemy.GetComponent<EnemyBase>().InitEnemy(enemyToSpawn);
        enemy.GetComponent<Enemy>().SetFlying(target);
        AddEnemyNumber(); // ⭐ 추가
    }

    IEnumerator SpawnBossCo(EnemyData enemyToSpawn)
    {
        // 보스 이름 얻어오기
        StageInfo stageInfo = FindObjectOfType<StageInfo>();
        PlayerDataManager playerDataManager = FindObjectOfType<PlayerDataManager>();
        int stageIndex = playerDataManager.GetCurrentStageNumber();
        string enemyName = stageInfo.GetStageBossName(stageIndex);

        // 보스 경고 메시지
        GameManager.instance.bossWarningPanel.Init(enemyName);
        yield return new WaitForSecondsRealtime(2f);

        // 랜딩 이펙트
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
        worldCenter.z = 0;
        Vector2 spawnPoint = worldCenter;

        GameObject landingDoodle = Instantiate(BossLandingDoodleIndicator, spawnPoint, Quaternion.identity);
        yield return new WaitForSeconds(2f);

        // 텔레포트 이펙트
        GameManager.instance.GetComponent<TeleportEffect>().GenTeleportEffect(spawnPoint);

        Destroy(landingDoodle);
        yield return new WaitForSeconds(.26f);

        // 보스 스폰
        GameObject enemy = Instantiate(FindObjectOfType<StageAssetManager>().GetBoss(), GameManager.instance.poolManager.transform);
        enemy.transform.position = spawnPoint;

        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        enemyBase.InitEnemy(enemyToSpawn);
        enemyBase.IsBoss = true;

        // 보스 등장 시 다른 모든 적, 아이템 제거
        GameManager.instance.fieldItemEffect.RemoveAllEnemy();
        GameManager.instance.fieldItemEffect.RemoveAllGems();
        GameManager.instance.fieldItemEffect.RemoveAllChests();

        // 보스 스테이지 변수를 트리거해서 더 이상 아이템 상자, 알 상자가 스폰되지 않도록
        GameManager.instance.SetBossStage(true);
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
        return equation.GetSpawnablePos(spawnConst, 2f); // Equation 재사용
    }
    #endregion
}