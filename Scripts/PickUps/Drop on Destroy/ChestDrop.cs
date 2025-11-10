using UnityEngine;

/// <summary>
/// 상자 전용 드롭 시스템
/// 플레이어 체력에 따라 힐링 아이템 우선 드롭
/// </summary>
public class ChestDrop : DropOnDestroy
{
    [SerializeField] private int exp;
    [SerializeField] private int hp;

    [Header("Debug")]
    [SerializeField] private bool isDebuggingOn;
    [SerializeField] private int dropItemIndex;

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

        // 체력이 30% 이하로 내려가면 무조건 힐링 아이템 드롭
        toDrop = SelectItemBasedOnPlayerHealth(ref itemIndex, toDrop);

        // 무더기 드롭 체크
        if (dropItemProperty[itemIndex].isMultipleDropable && CheckMultiDropChance())
        {
            int multiNum = dropItemProperty[itemIndex].numMultiple + UnityEngine.Random.Range(0, 9);
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
            Debug.LogWarning("ChestDrop, drop Item Prefab이 null입니다.");
            return;
        }

        // 알 스폰 가능 여부 체크
        if (!CanSpawnEgg(toDrop))
        {
            return;
        }

        // 디버깅 모드
        if (isDebuggingOn)
        {
            toDrop = dropItemProperty[dropItemIndex].Item;
        }

        // 아이템 스폰
        SpawnItem(toDrop, itemIndex);
    }

    /// <summary>
    /// 플레이어 체력에 따라 힐링 아이템 우선 선택
    /// </summary>
    private GameObject SelectItemBasedOnPlayerHealth(ref int itemIndex, GameObject defaultItem)
    {
        Character character = Player.instance.GetComponent<Character>();
        float healthRatio = (float)character.GetCurrentHP() / (float)character.MaxHealth;

        // 체력이 30% 이하면 힐링 아이템 찾기
        if (healthRatio < 0.3f)
        {
            for (int i = 0; i < dropItemProperty.Count; i++)
            {
                if (dropItemProperty[i].Item.GetComponent<HealPickUpObject>() != null)
                {
                    itemIndex = i;
                    return dropItemProperty[i].Item;
                }
            }
        }

        return defaultItem;
    }

    protected override void SpawnItem(GameObject toDrop, int itemIndex)
    {
        bool isGem = IsGem(toDrop);
        int itemExp = exp;

        // 보석이라면 무조건 드롭 (상자에서 나오는 보석)
        if (isGem)
        {
            GemPickUpObject gemPick = toDrop.GetComponent<GemPickUpObject>();
            if (gemPick != null)
            {
                itemExp = gemPick.ExpAmount;
            }
        }

        SpawnManager.instance.SpawnObject(transform.position, toDrop, isGem, itemExp);
    }

    protected override int GetExperienceAmount(GameObject toDrop)
    {
        GemPickUpObject gemPick = toDrop.GetComponent<GemPickUpObject>();
        if (gemPick != null)
        {
            return gemPick.ExpAmount;
        }
        return exp;
    }
}