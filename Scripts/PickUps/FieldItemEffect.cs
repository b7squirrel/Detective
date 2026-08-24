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
    public float ExpMultiplier { get; private set; } = 1f;
    public bool IsDoubleCoin { get; private set; } = false;
    public bool IsExpAtMax => ExpMultiplier >= MAX_MULTIPLIER;
    public bool IsCoinAtMax => IsDoubleCoin;
    public bool IsDoubleExp => ExpMultiplier > 1f;

    bool isSpeedBoostActive = false;
    bool isDamageBoostActive = false;
    float currentSpeedBoostValue = 0f;
    int currentDamageBoostValue = 0;
    public int DamageBoostStack { get; private set; } = 0;

    public event Action<FieldBuffType, float> OnBuffApplied;
    public event Action<FieldBuffType> OnBuffExpired;

    Coroutine coSpeedBoost, coDamageBoost, coDoubleExp, coDoubleCoin;

    public void ApplyBuff(FieldBuffType buffType, float duration, float value)
    {
        switch (buffType)
        {
            case FieldBuffType.SpeedBoost:
                if (coSpeedBoost != null) StopCoroutine(coSpeedBoost);
                coSpeedBoost = StartCoroutine(SpeedBoostCo(duration, value));
                OnBuffApplied?.Invoke(buffType, duration);
                // ⭐ 변경: FieldMessageType 사용
                MessageSystem.instance.PostBuffMessage(LocalizationManager.Game.buffSpeedBoost, MessageSystem.instance.GetBuffColor(FieldMessageType.SpeedBoost));
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
                var g = LocalizationManager.Game;
                string expMsg = $"{g.buffExpPrefix} {(int)ExpMultiplier}{g.buffExpSuffix}";
                // ⭐ 변경: FieldMessageType 사용
                MessageSystem.instance.PostBuffMessage(expMsg, MessageSystem.instance.GetBuffColor(FieldMessageType.DoubleExp));
                Logger.Log($"[FieldBuff] 경험치 배율 → {ExpMultiplier}배, 타이머 {duration}초 리셋");
                break;
            case FieldBuffType.DoubleCoin:
                IsDoubleCoin = true;
                if (coDoubleCoin != null) StopCoroutine(coDoubleCoin);
                coDoubleCoin = StartCoroutine(DoubleCoinCo(duration));
                OnBuffApplied?.Invoke(buffType, duration);
                // ⭐ 변경: FieldMessageType 사용
                MessageSystem.instance.PostBuffMessage(LocalizationManager.Game.coinFrenzy, MessageSystem.instance.GetBuffColor(FieldMessageType.DoubleCoin));
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
            int additionalBoost = (int)(character.DamageBonus * 0.5f);
            currentDamageBoostValue += additionalBoost;
            character.AddDamageBonus(additionalBoost);
            DamageBoostStack++;
            Logger.Log($"[FieldBuff] 데미지 버프 중첩 +{additionalBoost}, 총 버프량: {currentDamageBoostValue}, 타이머 {duration}초 리셋");
        }
        else
        {
            isDamageBoostActive = true;
            DamageBoostStack = 1;
            currentDamageBoostValue = (int)(character.DamageBonus * 0.5f);
            character.AddDamageBonus(currentDamageBoostValue);
            Logger.Log($"[FieldBuff] 데미지 버프 시작 +{currentDamageBoostValue} (현재의 1.5배), {duration}초");
        }
        string dmgMsg = $"{LocalizationManager.Game.buffDamageBoost} Up{DamageBoostStack}";
        // ⭐ 변경: FieldMessageType 사용
        MessageSystem.instance.PostBuffMessage(dmgMsg, MessageSystem.instance.GetBuffColor(FieldMessageType.DamageBoost));
        yield return new WaitForSeconds(duration);
        character.AddDamageBonus(-currentDamageBoostValue);
        isDamageBoostActive = false;
        currentDamageBoostValue = 0;
        DamageBoostStack = 0;
        coDamageBoost = null;
        OnBuffExpired?.Invoke(FieldBuffType.DamageBoost);
        Logger.Log("[FieldBuff] 데미지 버프 종료");
    }

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
        // ⭐ 추가: 시간정지 메시지
        MessageSystem.instance.PostBuffMessage(LocalizationManager.Game.timeStop, MessageSystem.instance.GetBuffColor(FieldMessageType.TimeStop));
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
        // ⭐ 추가: 무적 메시지
        MessageSystem.instance.PostBuffMessage(LocalizationManager.Game.invincible, MessageSystem.instance.GetBuffColor(FieldMessageType.Invincible));
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

    #region 모든 보석/필드 아이템(버프 아이템 포함) 제거
    public void RemoveAllGems()
    {
        Collectable[] collectables = FindObjectsOfType<Collectable>();
        foreach (var collectable in collectables)
        {
            if (collectable == null || !collectable.gameObject.activeSelf) continue;
            GameObject effect = GameManager.instance.poolManager.GetMisc(itemDieEffect);
            if (effect != null) effect.transform.position = collectable.transform.position;
            collectable.gameObject.SetActive(false);
        }
    }
    #endregion

    #region 모든 상자 제거
    public void RemoveAllChests()
    {
        DestructableObject[] destructables = FindObjectsOfType<DestructableObject>();
        foreach (var destructable in destructables)
        {
            if (destructable == null || !destructable.gameObject.activeSelf) continue;
            GameObject effect = GameManager.instance.poolManager.GetMisc(itemDieEffect);
            if (effect != null) effect.transform.position = destructable.transform.position;
            destructable.gameObject.SetActive(false);
        }
    }
    #endregion
}