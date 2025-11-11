using UnityEngine;

/// <summary>
/// 적 전용 드롭 시스템
/// </summary>
public class EnemyDrop : DropOnDestroy
{
    [SerializeField] private int exp;

    public override void CheckDrop()
    {
        if (IsDropListEmpty()) return;

        // dropAllItemList가 true면 모든 아이템을 드롭
        if (dropAllItemList)
        {
            DropAllItems();
            return;
        }

        // 랜덤 아이템 선택
        int itemIndex = SelectRandomItemIndex();
        GameObject toDrop = dropItemProperty[itemIndex].Item;

        // 무더기 드롭 체크
        if (dropItemProperty[itemIndex].isMultipleDropable && CheckMultiDropChance())
        {
            int multiNum = dropItemProperty[itemIndex].numMultiple + UnityEngine.Random.Range(4, 9);
            multiNum = Mathf.Max(1, multiNum);
            DropMultipleObjects(toDrop, multiNum);
            return;
        }

        // 스페셜 드롭 체크
        if (dropItemProperty[itemIndex].hasSpecialItem && CheckSpecialDropChance())
        {
            if (dropItemProperty[itemIndex].SpecialDrop != null)
            {
                toDrop = dropItemProperty[itemIndex].SpecialDrop;
            }
        }

        if (toDrop == null)
        {
            Debug.LogWarning("EnemyDrop, drop Item Prefab이 null입니다.");
            return;
        }

        // 알 스폰 가능 여부 체크
        if (!CanSpawnEgg(toDrop))
        {
            return;
        }

        // 아이템 스폰
        SpawnItem(toDrop, itemIndex);
    }

    protected override void SpawnItem(GameObject toDrop, int itemIndex)
    {
        bool isGem = IsGem(toDrop);
        int itemExp = GetExperienceAmount(toDrop);

        // 보석이라면 확률에 따라 드롭
        if (isGem)
        {
            float randomDrop = UnityEngine.Random.Range(0f, 1f);
            if (randomDrop > StaticValues.GemDropRate)
                return;
        }

        SpawnManager.instance.SpawnObject(transform.position, toDrop, isGem, itemExp);
    }

    protected override int GetExperienceAmount(GameObject toDrop)
    {
        // 적의 경험치 보상
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            return enemy.ExperienceReward;
        }

        // 보석의 경험치
        GemPickUpObject gemPick = toDrop.GetComponent<GemPickUpObject>();
        if (gemPick != null)
        {
            return gemPick.ExpAmount;
        }

        return exp;
    }
}