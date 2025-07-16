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
    public void Spawn(EnemyData enemyToSpawn, int index, bool forceSpawn)
    {
        // 적들이 몰려옵니다의 경우 강제로 스폰
        if (forceSpawn == false)
        {
            if (currentEnemyNumbers >= maxEnemyInScene)
                return;
        }

        // 스폰 가능한 지점 탐색하고 벽 안쪽에서 2 unit 더 안쪽에 스폰
        GameObject enemy = GameManager.instance.poolManager.GetEnemy(index);
        Vector2 spawnPoint = GetAvailablePoints();
        enemy.transform.position = new Vector2(spawnPoint.x, spawnPoint.y);

        // 초기화
        enemy.GetComponent<EnemyBase>().InitEnemy(enemyToSpawn);

        // 적 수 계산
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
            enemy.GetComponent<EnemyBase>().InitEnemy(enemyToSpawn);
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
        enemy.GetComponent<EnemyBase>().InitEnemy(enemyToSpawn);
        enemy.GetComponent<Enemy>().SetFlying(target);
    }

    IEnumerator SpawnBossCo(EnemyData enemyToSpawn)
    {
        // 보스 이름 얻어오기
        StageInfo stageInfo = FindObjectOfType<StageInfo>();
        PlayerDataManager playerDataManager = FindObjectOfType<PlayerDataManager>();
        int stageIndex = playerDataManager.GetCurrentStageNumber();
        string enemyName = stageInfo.GetStageInfo(stageIndex).Title;

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

        // 보스 스폰
        Destroy(landingDoodle);
        yield return new WaitForSeconds(.26f);

        // 스폰 위치 정하기, 보스 프리펩 얻어오기
        GameObject enemy = Instantiate(FindObjectOfType<StageAssetManager>().GetBoss(), GameManager.instance.poolManager.transform);
        enemy.transform.position = spawnPoint;

        // 보스 초기화
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        enemyBase.InitEnemy(enemyToSpawn);
        enemyBase.IsBoss = true;

        // 다른 모든 적들 제거
        GameManager.instance.fieldItemEffect.RemoveAllEnemy();
        GameManager.instance.fieldItemEffect.RemoveAllGems();
        GameManager.instance.fieldItemEffect.RemoveAllChests();

        // 보스 스테이지 변수를 트리거 해서 더 이상 아이템 상자, 알 상자가 스폰되지 않도록
        GameManager.instance.SetBossStage(true);

        // 줄어드는 벽 활성화
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
