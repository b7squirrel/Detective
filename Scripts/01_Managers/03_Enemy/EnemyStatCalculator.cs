using UnityEngine;

public class EnemyStatCalculator : MonoBehaviour
{
    [SerializeField] EnemyScalingConfig scalingConfig;

    // ⭐ 무한 모드 체크를 위해 추가
    static InfiniteStageManager infiniteStageManager;
    static bool isInfiniteMode = false;

    public EnemyStats GetStatsForStage(int stage, EnemyData baseData)
    {
        if (scalingConfig == null)
        {
            Debug.LogError("EnemyScalingConfig가 할당되지 않았습니다!");
            return CreateDefaultStats(); // 기본 스탯 생성
        }

        // 무한 모드 체크
        if (infiniteStageManager == null)
        {
            infiniteStageManager = FindObjectOfType<InfiniteStageManager>();
            isInfiniteMode = (infiniteStageManager != null);
        }


        // ⭐ 새로 생성 (복사 안 함)
        EnemyStats stats = new EnemyStats();

        float roleHPBonus = GetRoleHPBonus(baseData.enemyRole);
        float roleDamageBonus = GetRoleDamageBonus(baseData.enemyRole);
        float bossMultiplier = GetBossMultiplier(stage, baseData);
        float normalCycleMultiplier = GetNormalEnemyCycleMultiplier(stage, baseData); // ⭐ 추가
        float tieredDamageMult = GetTieredDamageMultiplier(stage, baseData); // ⭐ 추가

        stats.hp = CalculateHP(stage, baseData, roleHPBonus, bossMultiplier, normalCycleMultiplier); // 기존 그대로
        stats.speed = CalculateSpeed(stage, baseData);
        stats.damage = CalculateDamage(stage, baseData, roleDamageBonus, true, bossMultiplier, normalCycleMultiplier * tieredDamageMult); // ⭐ 곱해줌
        stats.rangedDamage = CalculateDamage(stage, baseData, roleDamageBonus, false, bossMultiplier, normalCycleMultiplier * tieredDamageMult); // ⭐ 곱해줌
        stats.experience_reward = CalculateExperience(stage, baseData, bossMultiplier);

        // ⭐ 회피 확률 계산 추가
        stats.dodgeChance = CalculateDodgeChance(stage, baseData);

        ApplyManualOverrides(stage, ref stats, baseData);

        return stats;
    }
    EnemyStats CreateDefaultStats()
    {
        return new EnemyStats
        {
            hp = 100,
            speed = 5,
            damage = 10,
            rangedDamage = 10,
            experience_reward = 50,
            dodgeChance = 0.05f

        };
    }

    // ⭐ 회피 확률 계산 함수 추가
    float CalculateDodgeChance(int stage, EnemyData baseData)
    {
        // 기본 회피 확률 + 스테이지당 증가량
        float dodgeChance = scalingConfig.baseDodgeChance +
                           (scalingConfig.dodgeGrowthPerStage * stage);

        // 보스 배율 적용
        if (baseData.bossType != BossType.Normal)
        {
            dodgeChance *= scalingConfig.bossDodgeMultiplier;
        }

        // 모드별 상한선 적용
        float cap = isInfiniteMode ?
            scalingConfig.dodgeCapInfinite :
            scalingConfig.dodgeCapRegular;

        dodgeChance = Mathf.Min(dodgeChance, cap);

        return dodgeChance;
    }

    /// <summary>
    /// 보스 타입과 스테이지에 따른 강화 배율 계산
    /// </summary>
    float GetBossMultiplier(int stage, EnemyData baseData)
    {
        // 기본 적이면 1.0
        if (baseData.bossType == BossType.Normal)
            return 1.0f;

        // 여왕 슬라임은 특별 배율
        if (baseData.bossType == BossType.QueenBoss)
            return scalingConfig.queenBossMultiplier;

        // 중간 보스와 스테이지 보스는 스테이지에 따라 동적으로 계산
        int cycleStage = ((stage - 1) % 6) + 1; // 1-6으로 순환
        int cycleNumber = (stage - 1) / 6; // 몇 번째 사이클인지 (0부터 시작)

        float baseMultiplier = 1.0f;

        if (baseData.bossType == BossType.SubBoss)
        {
            // 중간 보스 배율
            baseMultiplier = scalingConfig.subBossMultiplier;
        }
        else if (baseData.bossType == BossType.StageBoss)
        {
            // 스테이지 보스 배율 = 서브보스 배율 × 추가 배율 (서브보스보다 강해야 함)
            baseMultiplier = scalingConfig.subBossMultiplier * scalingConfig.stageBossMultiplier;
        }

        // 사이클이 반복될수록 더 강해짐
        float cycleBonus = 1.0f + (cycleNumber * scalingConfig.cycleGrowth);

        return baseMultiplier * cycleBonus;
    }

    /// <summary>
    /// 일반 몹(보스 아님) 전용 - 사이클마다 누적되는 강화 배율
    /// 사이클1(스테이지 1~6)은 보너스 없음, 사이클2(7~12)부터 누적 증가
    /// </summary>
    float GetNormalEnemyCycleMultiplier(int stage, EnemyData baseData)
    {
        // 보스류는 이미 GetBossMultiplier에서 별도로 강화되므로 여기선 1.0
        if (baseData.bossType != BossType.Normal)
            return 1f;

        int cycleNumber = (stage - 1) / 6; // 0부터 시작 (사이클1 = 0)
        return 1f + (cycleNumber * scalingConfig.normalEnemyCycleGrowth);
    }

    float GetRoleHPBonus(EnemyRole role)
    {
        switch (role)
        {
            case EnemyRole.Tank:
                return scalingConfig.tankHPBonus;
            case EnemyRole.GlassCannon:
                return -0.2f;
            case EnemyRole.Attacker:
                return -0.1f;
            default:
                return 0f;
        }
    }

    float GetRoleDamageBonus(EnemyRole role)
    {
        switch (role)
        {
            case EnemyRole.Attacker:
            case EnemyRole.GlassCannon:
                return scalingConfig.attackerDamageBonus;
            case EnemyRole.Tank:
                return -0.15f;
            default:
                return 0f;
        }
    }

    int CalculateHP(int stage, EnemyData baseData, float roleBonus, float bossMultiplier, float normalCycleMultiplier)
    {
        float stageMultiplier = Mathf.Pow(
            1 + scalingConfig.hpGrowth * stage,
            scalingConfig.hpExponent
        );

        float typeMultiplier = baseData.hpScalingMultiplier;
        float roleFactor = 1f + roleBonus;

        int finalHP = Mathf.RoundToInt(
            scalingConfig.baseHP * stageMultiplier * typeMultiplier * roleFactor * bossMultiplier * normalCycleMultiplier
        );

        return finalHP;
    }

    float CalculateSpeed(int stage, EnemyData baseData)
    {
        float speed = scalingConfig.baseSpeed * (1 + scalingConfig.speedGrowth * stage);
        speed *= baseData.speedScalingMultiplier;

        return Mathf.Min(speed, scalingConfig.speedCap);
    }

    int CalculateDamage(int stage, EnemyData baseData, float roleBonus, bool isMelee, float bossMultiplier, float normalCycleMultiplier)
    {
        float baseDamage = isMelee ?
            scalingConfig.baseMeleeDamage :
            scalingConfig.baseRangedDamage;

        float stageMultiplier = Mathf.Pow(
            1 + scalingConfig.damageGrowth * stage,
            scalingConfig.damageExponent
        );

        float typeMultiplier = baseData.damageScalingMultiplier;
        float roleFactor = 1f + roleBonus;
        float rangedBonus = isMelee ? 1f : 1.2f;

        int finalDamage = Mathf.RoundToInt(
            baseDamage * stageMultiplier * typeMultiplier * roleFactor * rangedBonus * bossMultiplier * normalCycleMultiplier
        );

        return finalDamage;
    }

    int CalculateExperience(int stage, EnemyData baseData, float bossMultiplier)
    {
        float difficultyMultiplier =
            (baseData.hpScalingMultiplier + baseData.damageScalingMultiplier) / 2f;

        int exp = Mathf.RoundToInt(
            scalingConfig.baseExperience *
            (1 + scalingConfig.experienceGrowth * stage) *
            difficultyMultiplier *
            bossMultiplier
        );

        return exp;
    }
    public EnemyScalingConfig GetScalingConfig()
    {
        return scalingConfig;
    }

    void ApplyManualOverrides(int stage, ref EnemyStats stats, EnemyData baseData)
    {
        if (scalingConfig.stageModifiers == null) return;

        foreach (var modifier in scalingConfig.stageModifiers)
        {
            if (modifier.stageNumber == stage)
            {
                // 보스 전용 오버라이드인데 일반 몹이면 건너뜀
                if (modifier.onlyAffectsBoss && baseData.bossType == BossType.Normal)
                    continue;

                if (modifier.hpOverride > 0)
                    stats.hp = modifier.hpOverride;
                if (modifier.speedOverride > 0)
                    stats.speed = modifier.speedOverride;
                if (modifier.damageOverride > 0)
                    stats.damage = modifier.damageOverride;
                if (modifier.experienceOverride > 0)
                    stats.experience_reward = modifier.experienceOverride;
                break;
            }
        }
    }

    /// <summary>
    /// 일반 몹(보스 제외) 전용 - 스테이지 구간별 데미지 전용 배율
    /// 13스테이지 이상이면 lateGame 배율, 7~12스테이지면 midGame 배율, 그 전엔 1.0
    /// </summary>
    float GetTieredDamageMultiplier(int stage, EnemyData baseData)
    {
        if (baseData.bossType != BossType.Normal) return 1f; // 보스는 asset별로 개별 관리 중이므로 제외

        if (stage >= scalingConfig.lateGameDamageStartStage)
            return scalingConfig.lateGameDamageMultiplier;
        if (stage >= scalingConfig.midGameDamageStartStage)
            return scalingConfig.midGameDamageMultiplier;

        return 1f;
    }
}