using UnityEngine;

/// <summary>
/// 상자 전용 드롭 시스템
/// 플레이어 체력에 따라 힐링 아이템 우선 드롭
/// DoubleExp / DoubleCoin 아이템은 최대 배율(4배) 도달 시 드롭 제외
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

        // 최대 재시도 횟수 (무한루프 방지)
        int maxTries = dropItemProperty.Count + 2;
        GameObject toDrop = null;
        int itemIndex = -1;

        for (int tries = 0; tries < maxTries; tries++)
        {
            int candidateIndex = SelectRandomItemIndex();
            GameObject candidate = dropItemProperty[candidateIndex].Item;

            // 최대 배율 도달한 버프 아이템이면 다시 뽑기
            if (IsBuffAtMax(candidate))
            {
                Logger.Log($"[ChestDrop] {candidate.name} 최대 배율 도달, 재추첨");
                continue;
            }

            itemIndex = candidateIndex;
            toDrop = candidate;
            break;
        }

        // 모든 후보가 막혔을 경우 (모든 아이템이 최대 배율인 극단적 상황)
        if (toDrop == null)
        {
            Logger.LogWarning("[ChestDrop] 드롭 가능한 아이템이 없습니다.");
            return;
        }

        // 체력이 30% 이하로 내려가면 무조건 힐링 아이템 드롭
        toDrop = SelectItemBasedOnPlayerHealth(ref itemIndex, toDrop);

        if (toDrop == null)
        {
            Debug.LogWarning("ChestDrop, drop Item Prefab이 null입니다.");
            return;
        }

        // 알 스폰 가능 여부 체크
        if (!CanSpawnEgg(toDrop)) return;

        // 디버깅 모드
        if (isDebuggingOn)
            toDrop = dropItemProperty[dropItemIndex].Item;

        // MultiPropCreator 여부 체크 후 처리
        if (IsMultiPropCreator(toDrop))
            SpawnMultiPropCreator(toDrop);
        else
            SpawnItem(toDrop, itemIndex);
    }

    /// <summary>
    /// 해당 아이템이 최대 배율에 도달한 DoubleExp / DoubleCoin 버프 아이템인지 확인
    /// </summary>
    private bool IsBuffAtMax(GameObject item)
    {
        if (item == null) return false;
        if (FieldItemEffect.instance == null) return false;

        TemporaryBuffPickUp buff = item.GetComponent<TemporaryBuffPickUp>();
        if (buff == null) return false;

        // TemporaryBuffPickUp의 buffType을 읽기 위해 public getter 사용
        FieldBuffType buffType = buff.GetBuffType();

        if (buffType == FieldBuffType.DoubleExp && FieldItemEffect.instance.IsExpAtMax)
            return true;

        if (buffType == FieldBuffType.DoubleCoin && FieldItemEffect.instance.IsCoinAtMax)
            return true;

        return false;
    }

    /// <summary>
    /// 플레이어 체력에 따라 힐링 아이템 우선 선택
    /// </summary>
    private GameObject SelectItemBasedOnPlayerHealth(ref int itemIndex, GameObject defaultItem)
    {
        Character character = Player.instance.GetComponent<Character>();
        float healthRatio = (float)character.GetCurrentHP() / (float)character.MaxHealth;

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

    private bool IsMultiPropCreator(GameObject prefab)
    {
        return prefab.GetComponent<MultiPropCreator>() != null;
    }

    private void SpawnMultiPropCreator(GameObject creatorPrefab)
    {
        GameObject creator = GameManager.instance.poolManager.GetMisc(creatorPrefab);

        if (creator == null)
        {
            Logger.LogError("[ChestDrop] MultiPropCreator를 풀에서 가져올 수 없습니다!");
            return;
        }

        creator.SetActive(true);

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

        if (isGem)
        {
            GemPickUpObject gemPick = toDrop.GetComponent<GemPickUpObject>();
            if (gemPick != null)
                itemExp = gemPick.ExpAmount;
        }

        PickupSpawner.Instance.SpawnPickup(transform.position, toDrop, isGem, itemExp);
    }

    protected override int GetExperienceAmount(GameObject toDrop)
    {
        GemPickUpObject gemPick = toDrop.GetComponent<GemPickUpObject>();
        if (gemPick != null)
            return gemPick.ExpAmount;
        return exp;
    }
}