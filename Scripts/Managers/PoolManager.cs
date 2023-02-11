using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 매니져에서 끌어다 쓸 것
/// </summary>
public class PoolManager : MonoBehaviour
{
    [SerializeField] GameObject[] prefabs;
    List<GameObject>[] pools;

    void Awake()
    {
        pools = new List<GameObject>[prefabs.Length];
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<GameObject>();
        }
    }

    public GameObject Get(int index)
    {
        GameObject select = null;

        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                select = item;
                select.SetActive(true);
                break;
            }
        }

        if (!select) // 풀이 비었다면 새로 만들어서 풀에 넣기
        {
            select = Instantiate(prefabs[index], transform);
            pools[index].Add(select);
        }

        return select;
    }
}
