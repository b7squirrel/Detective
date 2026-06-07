using System;
using System.Collections;
using UnityEngine;

public enum FieldBuffType
{
    SpeedBoost,
    DamageBoost,
    DoubleExp,
    DoubleCoin
}

public class FieldItemEffect : MonoBehaviour
{
    public static FieldItemEffect instance;
    [SerializeField] float stopDuration;
    [SerializeField] float invincibaleDuration;
    [SerializeField] CountdownTimer stopCounterUI;
    [SerializeField] InvincibleCounterUI invincibleCounterUI;
    [Header("폭탄 설정")]
    [SerializeField] int bombDamage;
    [SerializeField] float bombRadius = 5f;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] GameObject bombHitEffect;
    [SerializeField] GameObject bombExplosionEffect;
    [SerializeField] GameObject damageIndicatorPrefab;
    [SerializeField] float indicatorDisplayTime = 0.5f;
    [SerializeField] GameObject itemDieEffect;
    ISpawnController spawnController;

    Coroutine coStopWatch, coInvincible;
    bool isStoppedWithStopwatch = false;

    // =============================================
    // 임시 버프 시스템
    // =============================================
    const float MAX_MULTIPLIER = 4f;

    // 경험치 배율 (1 = 기본, 2~최대 = 버프)
    public float ExpMultiplier { get; private set; } = 1f;

    // DoubleCoin은 배율 대신 추가 드롭 방식. 활성화 여부만 관리.
    public bool IsDoubleCoin { get; private set; } = false;

    // 최대 배율 도달 여부 (ChestDrop 드롭 차단용)
    public bool IsExpAtMax => ExpMultiplier >= MAX_MULTIPLIER;
    public bool IsCoinAtMax => IsDoubleCoin; // 이미 활성화 중이면 상자에서 추가 드롭 안 함

    // 경험치 버프 활성화 여부
    public bool IsDoubleExp => ExpMultiplier > 1f;

    // SpeedBoost / DamageBoost 중첩 방지
    bool isSpeedBoostActive = false;
    bool isDamageBoostActive = false;
    float currentSpeedBoostValue = 0f;
    int currentDamageBoostValue = 0;

    // UI 연동 이벤트
    public event Action<FieldBuffType, float> OnBuffApplied;  // 버프 시작/갱신 (타입, 지속시간)
    public event Action<FieldBuffType> OnBuffExpired;          // 버프 종료 (타입)

    Coroutine coSpeedBoost, coDamageBoost, coDoubleExp, coDoubleCoin;

    /// <summary>
    /// 버프 아이템 픽업 시 호출.
    /// DoubleExp / DoubleCoin: 획득할 때마다 배율 +1 중첩(최대 4배).
    ///   타이머는 항상 마지막 획득 시점 기준으로 리셋.
    ///   타이머가 끝나면 배율이 한 번에 1로 초기화.
    /// SpeedBoost / DamageBoost: 수치 중첩 없이 타이머만 갱신.
    /// </summary>
    public void ApplyBuff(FieldBuffType buffType, float duration, float value)
    {
        switch (buffType)
        {
            case FieldBuffType.SpeedBoost:
                if (coSpeedBoost != null) StopCoroutine(coSpeedBoost);
                coSpeedBoost = StartCoroutine(SpeedBoostCo(duration, value));
                OnBuffApplied?.Invoke(buffType, duration);
                break;
            case FieldBuffType.DamageBoost:
                if (coDamageBoost != null) StopCoroutine(coDamageBoost);
                coDamageBoost = StartCoroutine(DamageBoostCo(duration, (int)value));
                OnBuffApplied?.Invoke(buffType, duration);
                break;
            case FieldBuffType.DoubleExp:
                if (ExpMultiplier < MAX_MULTIPLIER)
                    ExpMultiplier += 1f;
                if (coDoubleExp != null) StopCoroutine(coDoubleExp);
                coDoubleExp = StartCoroutine(DoubleExpCo(duration));
                OnBuffApplied?.Invoke(buffType, duration);
                Logger.Log($"[FieldBuff] 경험치 배율 → {ExpMultiplier}배, 타이머 {duration}초 리셋");
                break;
            case FieldBuffType.DoubleCoin:
                // 추가 드롭 방식이므로 배율 중첩 없이 활성화만 관리. 타이머만 갱신.
                IsDoubleCoin = true;
                if (coDoubleCoin != null) StopCoroutine(coDoubleCoin);
                coDoubleCoin = StartCoroutine(DoubleCoinCo(duration));
                OnBuffApplied?.Invoke(buffType, duration);
                Logger.Log($"[FieldBuff] 동전 추가 드롭 버프 시작(갱신), 타이머 {duration}초 리셋");
                break;
        }
    }

    IEnumerator SpeedBoostCo(float duration, float value)
    {
        Character character = Player.instance.GetComponent<Character>();

        if (isSpeedBoostActive)
        {
            Logger.Log($"[FieldBuff] 속도 버프 타이머 리셋 ({duration}초)");
        }
        else
        {
            isSpeedBoostActive = true;
            currentSpeedBoostValue = value;
            character.MoveSpeed += value;
            Logger.Log($"[FieldBuff] 속도 버프 시작 +{value}, {duration}초");
        }

        yield return new WaitForSeconds(duration);

        character.MoveSpeed -= currentSpeedBoostValue;
        isSpeedBoostActive = false;
        currentSpeedBoostValue = 0f;
        coSpeedBoost = null;
        OnBuffExpired?.Invoke(FieldBuffType.SpeedBoost);
        Logger.Log("[FieldBuff] 속도 버프 종료");
    }

    IEnumerator DamageBoostCo(float duration, int value)
    {
        Character character = Player.instance.GetComponent<Character>();

        if (isDamageBoostActive)
        {
            Logger.Log($"[FieldBuff] 데미지 버프 타이머 리셋 ({duration}초)");
        }
        else
        {
            isDamageBoostActive = true;
            currentDamageBoostValue = value;
            character.AddDamageBonus(value);
            Logger.Log($"[FieldBuff] 데미지 버프 시작 +{value}, {duration}초");
        }

        yield return new WaitForSeconds(duration);

        character.AddDamageBonus(-currentDamageBoostValue);
        isDamageBoostActive = false;
        currentDamageBoostValue = 0;
        coDamageBoost = null;
        OnBuffExpired?.Invoke(FieldBuffType.DamageBoost);
        Logger.Log("[FieldBuff] 데미지 버프 종료");
    }

    // 타이머만 관리. 배율은 ApplyBuff에서 이미 올렸으므로 여기선 종료 시 1로 초기화만.
    IEnumerator DoubleExpCo(float duration)
    {
        yield return new WaitForSeconds(duration);
        ExpMultiplier = 1f;
        coDoubleExp = null;
        OnBuffExpired?.Invoke(FieldBuffType.DoubleExp);
        Logger.Log("[FieldBuff] 경험치 버프 종료 → 1배로 초기화");
    }

    IEnumerator DoubleCoinCo(float duration)
    {
        yield return new WaitForSeconds(duration);
        IsDoubleCoin = false;
        coDoubleCoin = null;
        OnBuffExpired?.Invoke(FieldBuffType.DoubleCoin);
        Logger.Log("[FieldBuff] 동전 추가 드롭 버프 종료");
    }
    // =============================================

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        spawnController = FindObjectOfType<StageEvenetManager>() as ISpawnController;

        if (spawnController == null)
            spawnController = FindObjectOfType<InfiniteStageManager>() as ISpawnController;

        if (spawnController == null)
            Logger.LogWarning("[FieldItemEffect] No spawn controller found!");

        stopCounterUI.gameObject.SetActive(false);
    }

    #region 시간정지
    public void StopEnemies()
    {
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
        if (allEnemies == null) return;
        if (coStopWatch != null) StopCoroutine(coStopWatch);
        coStopWatch = StartCoroutine(StopEnemiesCo(allEnemies, stopDuration));
        stopCounterUI.StartTimer(stopDuration);
    }

    IEnumerator StopEnemiesCo(EnemyBase[] _allEnemies, float _stopDuration)
    {
        if (spawnController != null)
        {
            spawnController.PauseSpawn(true);
            Logger.Log("[FieldItemEffect] 스폰이 정지되었습니다.");
        }
        else
        {
            Logger.LogWarning("[FieldItemEffect] 어떤 종류의 Spawn Controller도 없습니다.");
        }

        stopCounterUI.gameObject.SetActive(true);

        for (int i = 0; i < _allEnemies.Length; i++)
        {
            if (_allEnemies[i] != null)
                _allEnemies[i].PauseEnemy();
        }

        isStoppedWithStopwatch = true;
        yield return new WaitForSeconds(_stopDuration);

        if (spawnController != null)
        {
            spawnController.PauseSpawn(false);
            Logger.Log("[FieldItemEffect] Spawn resumed");
        }

        EnemyBase[] allCurrentEnemies = FindObjectsOfType<EnemyBase>();
        for (int i = 0; i < allCurrentEnemies.Length; i++)
        {
            if (allCurrentEnemies[i] != null && allCurrentEnemies[i].gameObject.activeSelf)
                allCurrentEnemies[i].ResumeEnemy();
        }

        isStoppedWithStopwatch = false;
        stopCounterUI.gameObject.SetActive(false);
    }

    public bool IsStopedWithStopwatch() => isStoppedWithStopwatch;
    #endregion

    #region 무적
    public void SetPlayerInvincible()
    {
        if (coInvincible != null) StopCoroutine(coInvincible);
        coInvincible = StartCoroutine(PlayerInvincibleCo());
    }

    IEnumerator PlayerInvincibleCo()
    {
        GameManager.instance.IsPlayerInvincible = true;
        GameManager.instance.IsPlayerItemInvincible = true;
        invincibleCounterUI.gameObject.SetActive(true);

        Animator counterAnim = invincibleCounterUI.GetComponent<Animator>();
        int remainingTime = Mathf.CeilToInt(invincibaleDuration);
        invincibleCounterUI.SetCountNumber(remainingTime);

        while (remainingTime > 0)
        {
            yield return new WaitForSeconds(1f);
            remainingTime--;
            invincibleCounterUI.SetCountNumber(remainingTime);
            counterAnim.SetTrigger("Pop");
        }

        GameManager.instance.IsPlayerInvincible = false;
        GameManager.instance.IsPlayerItemInvincible = false;
        invincibleCounterUI.gameObject.SetActive(false);
    }
    #endregion

    #region 폭탄
    public void Explode(Vector2 _pos)
    {
        if (damageIndicatorPrefab != null)
            StartCoroutine(ShowBombIndicator(_pos));

        GameObject effect = GameManager.instance.poolManager.GetMisc(bombExplosionEffect);
        effect.transform.position = _pos;

        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(_pos, bombRadius, enemyLayer);

        if (enemiesInRange.Length == 0)
        {
            Logger.Log("[FieldItemEffect] 폭탄 범위 내에 적이 없습니다.");
            return;
        }

        int damagedEnemies = 0;
        for (int i = 0; i < enemiesInRange.Length; i++)
        {
            Idamageable enemy = enemiesInRange[i].GetComponent<Idamageable>();
            if (enemy != null && enemiesInRange[i].gameObject.activeSelf)
            {
                PostMessage(bombDamage, enemiesInRange[i].transform.position);
                enemy.TakeDamage(bombDamage, 0, 0, _pos, bombHitEffect);
                damagedEnemies++;
            }
        }

        Logger.Log($"[FieldItemEffect] 폭탄으로 {damagedEnemies}마리의 적에게 데미지를 입혔습니다.");
    }

    IEnumerator ShowBombIndicator(Vector2 _pos)
    {
        GameObject indicator = GameManager.instance.poolManager.GetMisc(damageIndicatorPrefab);
        DamageIndicator damageIndicator = indicator.GetComponent<DamageIndicator>();
        if (damageIndicator != null)
            damageIndicator.Init(bombRadius, _pos);

        yield return new WaitForSeconds(indicatorDisplayTime);
        indicator.SetActive(false);
    }

    void PostMessage(int damage, Vector3 targetPosition)
    {
        MessageSystem.instance.PostMessage(damage.ToString(), targetPosition, false);
    }
    #endregion

    #region 모든 적 제거
    public void RemoveAllEnemy()
    {
        EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
        if (allEnemies == null) return;
        foreach (var item in allEnemies)
            item.DieOnBossEvent();
    }
    #endregion

    #region 모든 보석 제거
    public void RemoveAllGems()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 200f);
        foreach (var item in hits)
        {
            Collectable collectable = item.GetComponent<Collectable>();
            if (collectable != null)
            {
                GameObject effect = GameManager.instance.poolManager.GetMisc(itemDieEffect);
                if (effect != null) effect.transform.position = collectable.transform.position;
                collectable.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region 모든 상자 제거
    public void RemoveAllChests()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 200f);
        foreach (var item in hits)
        {
            DestructableObject destructableObject = item.GetComponent<DestructableObject>();
            if (destructableObject != null)
            {
                GameObject effect = GameManager.instance.poolManager.GetMisc(itemDieEffect);
                if (effect != null) effect.transform.position = destructableObject.transform.position;
                destructableObject.gameObject.SetActive(false);
            }
        }
    }
    #endregion
}