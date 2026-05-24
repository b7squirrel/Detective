using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] GameObject[] enemies;
    [SerializeField] List<GameObject>[] enemyPools;

    [Header("Sub Bosses")]
    GameObject[] subBossEnemies;
    List<GameObject>[] subBossPools;
    GameObject subBossFolder;

    StageAssetManager stageAssetManager;

    Dictionary<string, List<GameObject>> miscPools;
    GameObject enemyFolder;
    List<GameObject> itemFolders;
    Dictionary<string, GameObject> folderDict; // itemFolders 선형 탐색 대신 O(1) Dictionary 사용

    [Header("Gems")]
    [SerializeField] Sprite[] gemSprites;

    [Header("Egg Box")]
    [SerializeField] GameObject eggBox;

    #region 초기화
    /// <summary>
    /// 폴더 초기화 — StageManager.Start()에서 가장 먼저 호출
    /// </summary>
    public void InitPools()
    {
        enemyFolder = new GameObject();
        enemyFolder.name = "Enemies";
        enemyFolder.transform.position = Vector3.zero;
        enemyFolder.transform.parent = transform;

        itemFolders = new List<GameObject>();
        folderDict = new Dictionary<string, GameObject>();
    }

    /// <summary>
    /// 레벨의 Stage Asset Manager에서 적들의 종류를 가져옴.
    /// stageAssetManager.Init() 이후에 호출해야 함.
    /// </summary>
    public void InitEnemyPools()
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

    /// <summary>
    /// SubBoss 전용 풀 초기화 — InitEnemyPools() 이후에 호출
    /// </summary>
    public void InitSubBossPools()
    {
        StageAssetManager sam = FindAnyObjectByType<StageAssetManager>();
        if (sam == null || sam.subBossEnemies == null || sam.subBossEnemies.Length == 0)
        {
            Logger.LogError("[PoolManager] SubBoss enemies not assigned in StageAssetManager!");
            return;
        }

        subBossEnemies = sam.subBossEnemies;
        subBossPools = new List<GameObject>[subBossEnemies.Length];

        for (int i = 0; i < subBossPools.Length; i++)
            subBossPools[i] = new List<GameObject>();

        subBossFolder = new GameObject { name = "SubBosses" };
        subBossFolder.transform.position = Vector3.zero;
        subBossFolder.transform.parent = transform;

        Logger.Log($"[PoolManager] SubBoss pools initialized: {subBossEnemies.Length} types");
    }

    /// <summary>
    /// 스테이지 시작 전에 Enemy 풀을 미리 생성.
    /// 실제 소환 시 Instantiate 없이 SetActive(true)만 하므로 스파이크 방지.
    /// 적 프리팹은 1종류(index 0)이며 animController만 교체되어 LV1~LV5로 사용됨.
    /// InitEnemyPools() 이후에 호출해야 함.
    /// </summary>
    public void WarmUpEnemyPools(int totalCount)
    {
        if (enemies == null || enemies.Length == 0)
        {
            Logger.LogError("[PoolManager] enemies 배열이 비어있습니다!");
            return;
        }

        // 적 프리팹은 1종류 (index 0)
        for (int i = 0; i < totalCount; i++)
        {
            GameObject obj = Instantiate(enemies[0], enemyFolder.transform);
            obj.SetActive(false);
            enemyPools[0].Add(obj);
        }

        Logger.Log($"[PoolManager] Enemy WarmUp 완료: {totalCount}개");
    }

    /// <summary>
    /// 스테이지 시작 전에 SubBoss 풀을 미리 생성.
    /// 동시에 1마리만 등장하지만 여유분 확보를 위해 countPerType = 2 권장.
    /// InitSubBossPools() 이후에 호출해야 함.
    /// </summary>
    public void WarmUpSubBossPools(int countPerType)
    {
        if (subBossEnemies == null) return;

        for (int i = 0; i < subBossEnemies.Length; i++)
        {
            for (int j = 0; j < countPerType; j++)
            {
                GameObject obj = Instantiate(subBossEnemies[i], subBossFolder.transform);
                obj.SetActive(false);
                subBossPools[i].Add(obj);
            }
        }

        Logger.Log($"[PoolManager] SubBoss WarmUp 완료: {subBossEnemies.Length}종 × {countPerType}개");
    }

    /// <summary>
    /// 무한 모드용 — 직접 적 목록을 전달받음.
    /// 일반 스테이지의 InitEnemyPools() 대신 사용.
    /// </summary>
    public void InitInfiniteEnemyPools(GameObject[] infiniteEnemies)
    {
        if (infiniteEnemies == null || infiniteEnemies.Length == 0)
        {
            Logger.LogError("[PoolManager] Infinite enemies array is null or empty!");
            return;
        }

        this.enemies = new GameObject[infiniteEnemies.Length];

        for (int i = 0; i < this.enemies.Length; i++)
        {
            this.enemies[i] = infiniteEnemies[i];
        }

        enemyPools = new List<GameObject>[this.enemies.Length];
        for (int i = 0; i < enemyPools.Length; i++)
        {
            enemyPools[i] = new List<GameObject>();
        }

        Logger.Log($"[PoolManager] Infinite enemy pools initialized with {this.enemies.Length} types");
    }
    #endregion

    #region GetEnemy
    public GameObject GetEnemy(int index)
    {
        GameObject select = null;
        List<GameObject> pool = enemyPools[index];

        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeSelf)
            {
                select = pool[i];
                select.SetActive(true);
                break;
            }
        }

        // WarmUp이 충분하다면 이 분기는 거의 실행되지 않음
        if (select == null)
        {
            select = Instantiate(enemies[index], enemyFolder.transform);
            pool.Add(select);
        }

        return select;
    }

    /// <summary>
    /// SubBoss 전용 GetEnemy — enemyPools[]와 완전히 분리
    /// </summary>
    public GameObject GetSubBossEnemy(int index)
    {
        if (subBossPools == null || index >= subBossPools.Length)
        {
            Logger.LogError($"[PoolManager] SubBoss pool error. index: {index}");
            return null;
        }

        List<GameObject> pool = subBossPools[index];

        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeSelf)
            {
                pool[i].SetActive(true);
                return pool[i];
            }
        }

        // WarmUp이 충분하다면 이 분기는 거의 실행되지 않음
        GameObject newObj = Instantiate(subBossEnemies[index], subBossFolder.transform);
        pool.Add(newObj);
        return newObj;
    }
    #endregion

    #region GetMisc
    // effects, weapons, sounds는 key를 이용해서 pooling
    public GameObject GetMisc(GameObject prefab)
    {
        if (miscPools == null) miscPools = new Dictionary<string, List<GameObject>>();

        PoolingKey key = prefab.GetComponent<PoolingKey>();
        string poolingTag = key.Key;
        int maxNum = key.maxNum;

        if (miscPools.ContainsKey(poolingTag)) // 해당 key의 pool이 있다면
        {
            List<GameObject> pool = miscPools[poolingTag];
            for (int i = 0; i < pool.Count; i++) // foreach 대신 for 사용하여 GC 방지
            {
                if (!pool[i].activeSelf)
                {
                    pool[i].SetActive(true);
                    return pool[i];
                }
            }
        }
        else // 해당 key의 pool이 없다면 새로 생성
        {
            GameObject folder = new GameObject();
            folder.transform.position = Vector3.zero;
            folder.transform.parent = transform;
            folder.name = poolingTag;
            itemFolders.Add(folder);
            folderDict[poolingTag] = folder; // Dictionary에도 등록

            List<GameObject> go = new List<GameObject>();
            GameObject select = Instantiate(prefab, folder.transform);
            go.Add(select);
            miscPools.Add(poolingTag, go);
            return select;
        }

        // pool 안의 오브젝트가 모두 사용 중이라면 새로 생성
        if (miscPools[poolingTag].Count < maxNum || maxNum == 0) // maxNum == 0이면 갯수 제한 없음
        {
            // Dictionary로 O(1) 탐색
            if (folderDict.TryGetValue(poolingTag, out GameObject targetFolder))
            {
                GameObject select = Instantiate(prefab, targetFolder.transform);
                miscPools[poolingTag].Add(select);
                return select;
            }
        }

        return null;
    }
    #endregion

    #region GetGem
    public GameObject GetGem(GameObject gem)
    {
        if (miscPools == null) miscPools = new Dictionary<string, List<GameObject>>();

        string poolingTag = gem.GetComponent<PoolingKey>().Key;

        if (miscPools.ContainsKey(poolingTag)) // 해당 key의 pool이 있다면
        {
            List<GameObject> pool = miscPools[poolingTag];
            for (int i = 0; i < pool.Count; i++) // foreach 대신 for 사용하여 GC 방지
            {
                if (!pool[i].activeSelf)
                {
                    pool[i].SetActive(true);
                    return pool[i];
                }
            }
        }
        else // 해당 key의 pool이 없다면 새로 생성
        {
            GameObject folder = new GameObject();
            folder.transform.position = Vector3.zero;
            folder.transform.parent = transform;
            folder.name = poolingTag;
            itemFolders.Add(folder);
            folderDict[poolingTag] = folder; // Dictionary에도 등록

            List<GameObject> go = new List<GameObject>();
            GameObject select = Instantiate(gem, folder.transform);
            go.Add(select);
            miscPools.Add(poolingTag, go);
            return select;
        }

        // pool 안의 오브젝트가 모두 사용 중이라면 새로 생성
        // Dictionary로 O(1) 탐색
        if (folderDict.TryGetValue(poolingTag, out GameObject gemFolder))
        {
            GameObject select = Instantiate(gem, gemFolder.transform);
            miscPools[poolingTag].Add(select);
            return select;
        }

        return null;
    }
    #endregion
}