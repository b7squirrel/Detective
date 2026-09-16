using UnityEngine;

/// <summary>
/// 적 전용 드롭 시스템
/// </summary>
public class EnemyDrop : DropOnDestroy
{
    [SerializeField] private int exp;

    [Header("DoubleCoin 버프")]
    [Tooltip("DoubleCoin 버프 활성화 시 추가로 드롭할 동전 프리팹")]
    [SerializeField] GameObject coinPrefab;
    [Tooltip("추가 드롭할 동전 개수 (min ~ max 랜덤)")]
    [SerializeField] int bonusCoinMin = 3;
    [SerializeField] int bonusCoinMax = 5;

    // ⭐ 추가: 서브보스 확정 드롭
    [Header("서브보스 확정 드롭")]
    [Tooltip("서브보스일 때 기존 랜덤 드롭과 별개로 항상 추가로 드롭할 경험치 보석")]
    [SerializeField] GameObject subBossExpGemPrefab;

    EnemyBase enemyBase; // ⭐ 추가: 매번 GetComponent 하지 않도록 캐싱

    void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    public override void CheckDrop()
    {
        if (IsDropListEmpty()) return;

        // ⭐ 추가: 서브보스면 기존 랜덤 드롭과 무관하게 항상 경험치 보석 하나를 추가로 드롭
        SpawnSubBossExpGem();

        // dropAllItemList가 true면 모든 아이템을 드롭
        if (dropAllItemList)
        {
            DropAllItems();
            SpawnBonusCoins(); // 보너스 동전 추가
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
            SpawnBonusCoins();
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
        if (!CanSpawnEgg(toDrop)) return;

        // 아이템 스폰
        SpawnItem(toDrop, itemIndex);
        SpawnBonusCoins(); // 보너스 동전 추가
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

        PickupSpawner.Instance.SpawnPickup(transform.position, toDrop, isGem, itemExp);
    }

    protected override int GetExperienceAmount(GameObject toDrop)
    {
        // 적의 경험치 보상
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
            return enemy.ExperienceReward;

        // 보석의 경험치
        GemPickUpObject gemPick = toDrop.GetComponent<GemPickUpObject>();
        if (gemPick != null)
            return gemPick.ExpAmount;

        return exp;
    }

    /// <summary>
    /// 서브보스라면 기존 랜덤 드롭 로직과 무관하게 확정으로 경험치 보석 하나를 추가 드롭
    /// </summary>
    void SpawnSubBossExpGem()
    {
        if (subBossExpGemPrefab == null) return;
        if (enemyBase == null || !enemyBase.IsSubBoss) return;

        int itemExp = GetExperienceAmount(subBossExpGemPrefab);
        PickupSpawner.Instance.SpawnPickup(transform.position, subBossExpGemPrefab, true, itemExp);

        Logger.Log($"[EnemyDrop] 서브보스 확정 경험치 보석 드롭 - exp: {itemExp}");
    }

    /// <summary>
    /// DoubleCoin 버프 활성화 중일 때 추가 동전 스폰
    /// </summary>
    void SpawnBonusCoins()
    {
        if (coinPrefab == null) return;
        if (FieldItemEffect.instance == null) return;
        if (!FieldItemEffect.instance.IsDoubleCoin) return;

        int count = Random.Range(bonusCoinMin, bonusCoinMax + 1);
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.5f;
            PickupSpawner.Instance.SpawnPickup(
                transform.position + (Vector3)offset,
                coinPrefab,
                false,
                0
            );
        }

        Logger.Log($"[EnemyDrop] DoubleCoin 보너스 동전 {count}개 드롭");
    }
}