using UnityEngine;

public class EnemyStatCalculator : MonoBehaviour
{
    [SerializeField] EnemyScalingConfig scalingConfig;

    public EnemyStats GetStatsForStage(int stage, EnemyData baseData)
    {
        if (scalingConfig == null)
        {
            Debug.LogError("EnemyScalingConfig가 할당되지 않았습니다!");
            return CreateDefaultStats(); // 기본 스탯 생성
        }

        // ⭐ 새로 생성 (복사 안 함)
        EnemyStats stats = new EnemyStats();

        float roleHPBonus = GetRoleHPBonus(baseData.enemyRole);
        float roleDamageBonus = GetRoleDamageBonus(baseData.enemyRole);
        float bossMultiplier = GetBossMultiplier(stage, baseData);

        stats.hp = CalculateHP(stage, baseData, roleHPBonus, bossMultiplier);
        stats.speed = CalculateSpeed(stage, baseData);
        stats.damage = CalculateDamage(stage, baseData, roleDamageBonus, true, bossMultiplier);
        stats.rangedDamage = CalculateDamage(stage, baseData, roleDamageBonus, false, bossMultiplier);
        stats.experience_reward = CalculateExperience(stage, baseData, bossMultiplier);

        ApplyManualOverrides(stage, ref stats);

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
            experience_reward = 50
        };
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
            // 스테이지 보스 배율 (중간 보스보다 강함)
            baseMultiplier = scalingConfig.stageBossMultiplier;
        }
        
        // 사이클이 반복될수록 더 강해짐
        float cycleBonus = 1.0f + (cycleNumber * scalingConfig.cycleGrowth);
        
        return baseMultiplier * cycleBonus;
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
    
    int CalculateHP(int stage, EnemyData baseData, float roleBonus, float bossMultiplier)
    {
        float stageMultiplier = Mathf.Pow(
            1 + scalingConfig.hpGrowth * stage, 
            scalingConfig.hpExponent
        );
        
        float typeMultiplier = baseData.hpScalingMultiplier;
        float roleFactor = 1f + roleBonus;
        
        int finalHP = Mathf.RoundToInt(
            scalingConfig.baseHP * stageMultiplier * typeMultiplier * roleFactor * bossMultiplier
        );
        
        return finalHP;
    }
    
    float CalculateSpeed(int stage, EnemyData baseData)
    {
        float speed = scalingConfig.baseSpeed * (1 + scalingConfig.speedGrowth * stage);
        speed *= baseData.speedScalingMultiplier;
        
        return Mathf.Min(speed, scalingConfig.speedCap);
    }
    
    int CalculateDamage(int stage, EnemyData baseData, float roleBonus, bool isMelee, float bossMultiplier)
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
            baseDamage * stageMultiplier * typeMultiplier * roleFactor * rangedBonus * bossMultiplier
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
    
    void ApplyManualOverrides(int stage, ref EnemyStats stats)
    {
        if (scalingConfig.stageModifiers == null) return;
        
        foreach (var modifier in scalingConfig.stageModifiers)
        {
            if (modifier.stageNumber == stage)
            {
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
}