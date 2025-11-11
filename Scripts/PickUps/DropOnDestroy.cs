using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DropItemProperty
{
    public GameObject Item;
    public GameObject SpecialDrop;
    public bool isMultipleDropable;
    public bool hasSpecialItem;
    public int numMultiple; // 몇 개를 한꺼번에 생성할 것인지.
}

/// <summary>
/// 드롭 로직의 기본 클래스
/// </summary>
public abstract class DropOnDestroy : MonoBehaviour
{
    [SerializeField] protected List<DropItemProperty> dropItemProperty;
    [SerializeField] protected float multiDropRate; // 무더기를 드롭할 확률
    [SerializeField] protected float specialDropRate; // 스페셜 드롭할 확률
    [SerializeField] protected bool dropAllItemList; // 리스트의 아이템을 모두 드롭할 것인지. 서브 보스처럼

    public abstract void CheckDrop();

    /// <summary>
    /// 알 스폰 가능 여부 체크 (하위 클래스에서 오버라이드 가능)
    /// </summary>
    protected virtual bool CanSpawnEgg(GameObject item)
    {
        return true; // 기본적으로는 허용
    }

    /// <summary>
    /// 리스트의 모든 아이템 드롭
    /// </summary>
    protected void DropAllItems()
    {
        for (int i = 0; i < dropItemProperty.Count; i++)
        {
            GameObject toDrop = dropItemProperty[i].Item;
            
            // 스페셜 드롭 확률 체크
            if (dropItemProperty[i].hasSpecialItem && CheckSpecialDropChance())
            {
                if (dropItemProperty[i].SpecialDrop != null)
                {
                    toDrop = dropItemProperty[i].SpecialDrop;
                }
            }

            if (toDrop == null)
            {
                Debug.LogWarning($"DropOnDestroy, dropItemProperty[{i}]의 Item이 null입니다.");
                continue;
            }

            // 알 스폰 가능 여부 체크
            if (!CanSpawnEgg(toDrop))
            {
                continue;
            }

            // 멀티플 드롭 체크
            if (dropItemProperty[i].isMultipleDropable && CheckMultiDropChance())
            {
                int multiNum = dropItemProperty[i].numMultiple + UnityEngine.Random.Range(4, 10);
                multiNum = Math.Max(1, multiNum);
                DropMultipleObjects(toDrop, multiNum);
                continue;
            }

            // 아이템 스폰 (하위 클래스에서 구현)
            SpawnItem(toDrop, i);
        }
    }

    /// <summary>
    /// 무더기 드롭
    /// </summary>
    protected void DropMultipleObjects(GameObject toDrop, int numberOfDrops)
    {
        int exp = GetExperienceAmount(toDrop);
        GameManager.instance.fieldItemSpawner.SpawnMultipleObjects(numberOfDrops, toDrop, transform.position, exp);
    }

    /// <summary>
    /// 경험치 양 계산 (하위 클래스에서 오버라이드 가능)
    /// </summary>
    protected virtual int GetExperienceAmount(GameObject toDrop)
    {
        GemPickUpObject gemPick = toDrop.GetComponent<GemPickUpObject>();
        if (gemPick != null)
        {
            return gemPick.ExpAmount;
        }
        return 0;
    }

    /// <summary>
    /// 단일 아이템 스폰 (하위 클래스에서 구현)
    /// </summary>
    protected abstract void SpawnItem(GameObject toDrop, int itemIndex);

    /// <summary>
    /// 무더기 드롭 확률 체크
    /// </summary>
    protected bool CheckMultiDropChance()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        return randomValue <= multiDropRate;
    }

    /// <summary>
    /// 스페셜 드롭 확률 체크
    /// </summary>
    protected bool CheckSpecialDropChance()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        return randomValue <= specialDropRate;
    }

    /// <summary>
    /// 랜덤 아이템 인덱스 선택
    /// </summary>
    protected int SelectRandomItemIndex()
    {
        return UnityEngine.Random.Range(0, dropItemProperty.Count);
    }

    /// <summary>
    /// 보석인지 확인
    /// </summary>
    protected bool IsGem(GameObject item)
    {
        Collectable collectable = item.GetComponent<Collectable>();
        return collectable != null && collectable.IsGem;
    }

    /// <summary>
    /// 아이템 리스트가 비어있는지 확인
    /// </summary>
    protected bool IsDropListEmpty()
    {
        if (dropItemProperty.Count <= 0)
        {
            Debug.LogWarning("DropOnDestroy, dropItemProperty 리스트가 비어 있습니다.");
            return true;
        }
        return false;
    }
}