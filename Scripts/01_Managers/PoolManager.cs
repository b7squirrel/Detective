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
    GameObject temp; // 임시 폴더를 매번 생성하지 않게 하기 위해서

    EnemyFinder enemyFinder;

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

        enemyFinder = FindObjectOfType<EnemyFinder>();
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

        for (int i = 0;i < enemyPools[index].Count;i++)
        {
            if (!enemyPools[index][i].activeSelf)
            {
                select = enemyPools[index][i];
                select.SetActive(true);
                break;
            }
        }

        if (!select)
        {
            select = Instantiate(enemies[index], enemyFolder.transform);
            enemyPools[index].Add(select);
        }

        //enemyFinder.AddEnemyToList(select.transform);
        return select;
    }
    #endregion

    #region GetMisc
    // effects, weapons, sounds는 tag를 이용해서 pooling
    public GameObject GetMisc(GameObject prefab)
    {
        GameObject select = null;

        if (miscPools == null) miscPools = new Dictionary<string, List<GameObject>>();

        string poolingTag = prefab.GetComponent<PoolingKey>().Key;
        int maxNum = prefab.GetComponent<PoolingKey>().maxNum;

        if (miscPools.ContainsKey(poolingTag)) // 해당 key의 pool이 있다면
        {
            foreach (var item in miscPools[poolingTag])
            {
                if (!item.activeSelf)
                {
                    select = item;
                    select.SetActive(true);
                    return select;
                }
            }
        }
        else // 해당 key의 pool이 없다면
        {
            List<GameObject> go = new List<GameObject>();

            GameObject folder = new GameObject();
            folder.transform.position = Vector3.zero;
            folder.transform.parent = transform;
            folder.name = poolingTag;
            itemFolders.Add(folder);

            select = Instantiate(prefab, folder.transform);
            go.Add(select);
            miscPools.Add(poolingTag, go);
            return select;
        }

        // pool안의 오브젝트가 모두 사용중이라면 
        // pooling 태그 이름이 같은 폴더를 찾아서 자식으로 넣어줌
        if(miscPools[poolingTag].Count < maxNum || maxNum == 0) // maxNum == 0이면 갯수 제한 없음.
        {
            for (int i = 0; i < itemFolders.Count; i++)
            {
                if (itemFolders[i].name == poolingTag)
                {
                    temp = itemFolders[i];
                }
            }
            select = Instantiate(prefab, temp.transform);
            miscPools[poolingTag].Add(select);
            return select;
        }
        return null;
    }
    #endregion

    #region Get Gems
    public GameObject GetGem(GameObject gem)
    {
        
        // GameObject gemToUI = Instantiate(gem, transform);
        // return gemToUI;
        
        GameObject select = null;

        if (miscPools == null) miscPools = new Dictionary<string, List<GameObject>>();

        string poolingTag = gem.GetComponent<PoolingKey>().Key;

        if (miscPools.ContainsKey(poolingTag)) // 해당 key의 pool이 있다면
        {
            foreach (var item in miscPools[poolingTag])
            {
                if (!item.activeSelf)
                {
                    select = item;
                    select.SetActive(true);
                    return select;
                }
            }
        }
        else // 해당 key의 pool이 없다면
        {
            List<GameObject> go = new List<GameObject>();

            GameObject folder = new GameObject();
            folder.transform.position = Vector3.zero;
            folder.transform.parent = transform;
            folder.name = poolingTag;
            itemFolders.Add(folder);

            select = Instantiate(gem, folder.transform);
            go.Add(select);
            miscPools.Add(poolingTag, go);

            return select;
        }

        // pool안의 오브젝트가 모두 사용중이라면 
        // pooling 태그 이름이 같은 폴더를 찾아서 자식으로 넣어줌

        for (int i = 0; i < itemFolders.Count; i++)
        {
            if (itemFolders[i].name == poolingTag)
            {
                temp = itemFolders[i];
            }
        }
        select = Instantiate(gem, temp.transform);
        miscPools[poolingTag].Add(select);

        return select;
    }
    #endregion
}
