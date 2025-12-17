using UnityEngine;

public class EnemyStatCalculator : MonoBehaviour
{
    [SerializeField] EnemyScalingConfig scalingConfig;
    
    public EnemyStats GetStatsForStage(int stage, EnemyData baseData)
    {
        if (scalingConfig == null)
        {
            Debug.LogError("EnemyScalingConfig가 할당되지 않았습니다!");
            return new EnemyStats(baseData.stats);
        }
        
        EnemyStats stats = new EnemyStats(baseData.stats);
        
        // 역할에 따른 추가 보너스 적용
        float roleHPBonus = GetRoleHPBonus(baseData.enemyRole);
        float roleDamageBonus = GetRoleDamageBonus(baseData.enemyRole);
        
        stats.hp = CalculateHP(stage, baseData, roleHPBonus);
        stats.speed = CalculateSpeed(stage, baseData);
        stats.damage = CalculateDamage(stage, baseData, roleDamageBonus, true); // melee
        stats.rangedDamage = CalculateDamage(stage, baseData, roleDamageBonus, false); // ranged
        stats.experience_reward = CalculateExperience(stage, baseData);
        
        // 수동 오버라이드 적용
        ApplyManualOverrides(stage, ref stats);
        
        return stats;
    }
    
    float GetRoleHPBonus(EnemyRole role)
    {
        switch (role)
        {
            case EnemyRole.Tank:
                return scalingConfig.tankHPBonus; // 20% 추가
            case EnemyRole.GlassCannon:
                return -0.2f; // 20% 감소
            case EnemyRole.Attacker:
                return -0.1f; // 10% 감소
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
                return scalingConfig.attackerDamageBonus; // 15% 추가
            case EnemyRole.Tank:
                return -0.15f; // 15% 감소
            default:
                return 0f;
        }
    }
    
    int CalculateHP(int stage, EnemyData baseData, float roleBonus)
    {
        // 기본 공식: HP = baseHP * (1 + growth * stage)^exponent
        float stageMultiplier = Mathf.Pow(
            1 + scalingConfig.hpGrowth * stage, 
            scalingConfig.hpExponent
        );
        
        // 타입별 배율
        float typeMultiplier = baseData.hpScalingMultiplier;
        
        // 역할별 보너스
        float roleFactor = 1f + roleBonus;
        
        int finalHP = Mathf.RoundToInt(
            scalingConfig.baseHP * stageMultiplier * typeMultiplier * roleFactor
        );
        
        return finalHP;
    }
    
    float CalculateSpeed(int stage, EnemyData baseData)
    {
        float speed = scalingConfig.baseSpeed * (1 + scalingConfig.speedGrowth * stage);
        speed *= baseData.speedScalingMultiplier;
        
        // 최대 속도 제한
        return Mathf.Min(speed, scalingConfig.speedCap);
    }
    
    int CalculateDamage(int stage, EnemyData baseData, float roleBonus, bool isMelee)
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
        
        // 원거리 공격은 추가 보너스
        float rangedBonus = isMelee ? 1f : 1.2f;
        
        int finalDamage = Mathf.RoundToInt(
            baseDamage * stageMultiplier * typeMultiplier * roleFactor * rangedBonus
        );
        
        return finalDamage;
    }
    
    int CalculateExperience(int stage, EnemyData baseData)
    {
        // 더 강한 적일수록 경험치 많이 줌
        float difficultyMultiplier = 
            (baseData.hpScalingMultiplier + baseData.damageScalingMultiplier) / 2f;
        
        int exp = Mathf.RoundToInt(
            scalingConfig.baseExperience * 
            (1 + scalingConfig.experienceGrowth * stage) * 
            difficultyMultiplier
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