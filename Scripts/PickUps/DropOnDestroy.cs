using System.Collections.Generic;
using UnityEngine;

public class DropOnDestroy : MonoBehaviour
{
    [SerializeField] List<GameObject> dropItemPrefab; // 맞을 때마다 떨어트리는 아이템
    [SerializeField] GameObject dropLastItemPrefab; // 파괴될 때 떨어트리는 아이템
    [SerializeField][Range(0f, 1f)] float chance = 1f;
    [SerializeField] int exp;
    [SerializeField] int hp;

    bool isQuiting;

    //void OnApplicationQuit()
    //{
    //    isQuiting = true;
    //}

    public void CheckDrop()
    {
        //if (isQuiting) return;

        if (dropItemPrefab.Count <= 0)
        {
            Debug.LogWarning("DropOnDestory, dropItemPrefab 리스트가 비어 있습니다.");
            return;
        }

        bool isGem = false;

        if (Random.value < chance)
        {
            GameObject toDrop = dropItemPrefab[Random.Range(0, dropItemPrefab.Count)];
            if (toDrop == null)
            {
                Debug.LogWarning("DropOnDestroy, drop Item Prefab이 null입니다.");
                return;
            }

            if (GetComponent<Enemy>() != null)
            {
                exp = GetComponent<Enemy>().ExperienceReward;
            }

            if(toDrop.GetComponent<Collectable>() != null)
            {
                isGem = toDrop.GetComponent<Collectable>().IsGem;
            }
            SpawnManager.instance.SpawnObject(transform.position, toDrop, isGem, exp);
        }
    }

    /// <summary>
    /// 체력이 있는 오브젝트가 파괴될 때 맞을 때마다 떨어트렸던 것과 다른 아이템을 떨어트림
    /// </summary>
    public void DropLastItem()
    {
        GameObject toDrop = dropLastItemPrefab;
        if (toDrop == null)
        {
            Debug.LogWarning("DropOnDestroy, drop Last Item Prefab이 null입니다.");
            return;
        }
        SpawnManager.instance.SpawnObject(transform.position, toDrop, false, 0);
    }
}
