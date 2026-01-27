using UnityEngine;

/// <summary>
/// 상자 전용 드롭 시스템
/// 플레이어 체력에 따라 힐링 아이템 우선 드롭
/// MultiPropCreator 지원 추가
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

        // 랜덤 아이템 선택
        int itemIndex = SelectRandomItemIndex();
        GameObject toDrop = dropItemProperty[itemIndex].Item;

        // 체력이 30% 이하로 내려가면 무조건 힐링 아이템 드롭
        toDrop = SelectItemBasedOnPlayerHealth(ref itemIndex, toDrop);

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

        // ⭐ MultiPropCreator 여부 체크 후 처리
        if (IsMultiPropCreator(toDrop))
        {
            SpawnMultiPropCreator(toDrop);
        }
        else
        {
            // 일반 아이템 스폰
            SpawnItem(toDrop, itemIndex);
        }
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

    /// <summary>
    /// ⭐ MultiPropCreator인지 확인
    /// </summary>
    private bool IsMultiPropCreator(GameObject prefab)
    {
        return prefab.GetComponent<MultiPropCreator>() != null;
    }

    /// <summary>
    /// ⭐ MultiPropCreator 스폰 (풀링 + Initialize)
    /// </summary>
    private void SpawnMultiPropCreator(GameObject creatorPrefab)
    {
        // 풀에서 가져오기
        GameObject creator = GameManager.instance.poolManager.GetMisc(creatorPrefab);
        
        if (creator == null)
        {
            Logger.LogError("[ChestDrop] MultiPropCreator를 풀에서 가져올 수 없습니다!");
            return;
        }

        creator.SetActive(true);

        // Initialize 메서드로 명시적 초기화
        MultiPropCreator script = creator.GetComponent<MultiPropCreator>();
        if (script != null)
        {
            script.Initialize(transform.position);
            Logger.Log($"[ChestDrop] MultiPropCreator 스폰 at {transform.position}");
        }
        else
        {
            Logger.LogError("[ChestDrop] MultiPropCreator 컴포넌트를 찾을 수 없습니다!");
        }
    }

    protected override void SpawnItem(GameObject toDrop, int itemIndex)
    {
        bool isGem = IsGem(toDrop);
        int itemExp = exp;

        // 보석이라면 경험치 정보 가져오기
        if (isGem)
        {
            GemPickUpObject gemPick = toDrop.GetComponent<GemPickUpObject>();
            if (gemPick != null)
            {
                itemExp = gemPick.ExpAmount;
            }
        }

        PickupSpawner.Instance.SpawnPickup(transform.position, toDrop, isGem, itemExp);
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