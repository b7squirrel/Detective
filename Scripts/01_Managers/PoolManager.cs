using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] GameObject[] enemies;
    [SerializeField] List<GameObject>[] enemyPools;

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
    /// 폴더 초기화
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
    /// 레벨의 Stage Asset Manager에서 적들의 종류를 가져옴
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
    /// 무한 모드용 - 직접 적 목록을 전달받음
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

        if (select == null)
        {
            select = Instantiate(enemies[index], enemyFolder.transform);
            pool.Add(select);
        }

        return select;
    }
    #endregion

    #region GetMisc
    // effects, weapons, sounds는 key를 이용해서 pooling
    public GameObject GetMisc(GameObject prefab)
    {
        if (miscPools == null) miscPools = new Dictionary<string, List<GameObject>>();

        string poolingTag = prefab.GetComponent<PoolingKey>().Key;
        int maxNum = prefab.GetComponent<PoolingKey>().maxNum;

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