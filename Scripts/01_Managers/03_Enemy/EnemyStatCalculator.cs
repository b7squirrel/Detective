using UnityEngine;

/// <summary>
/// 스테이지에 따라 적의 스탯을 계산하는 클래스
/// GameManager에 컴포넌트로 부착하거나 싱글톤으로 관리
/// </summary>
public class EnemyStatCalculator : MonoBehaviour
{
    [SerializeField] EnemyScalingConfig scalingConfig;

    /// <summary>
    /// 특정 스테이지에 맞는 적 스탯을 계산
    /// </summary>
    public EnemyStats GetStatsForStage(int stage, EnemyData baseData)
    {
        if (scalingConfig == null)
        {
            Debug.LogError("EnemyScalingConfig가 할당되지 않았습니다!");
            return new EnemyStats(baseData.stats);
        }

        EnemyStats stats = new EnemyStats(baseData.stats);

        stats.hp = CalculateHP(stage, baseData);
        stats.speed = CalculateSpeed(stage, baseData);
        stats.damage = CalculateDamage(stage, baseData);
        stats.experience_reward = CalculateExperience(stage, baseData);

        StageStatModifier modifier = GetModifierForStage(stage);
        if (modifier != null)
        {
            if (modifier.hpOverride > 0)
                stats.hp = modifier.hpOverride;
            if (modifier.speedOverride > 0)
                stats.speed = modifier.speedOverride;
            if (modifier.damageOverride > 0)
                stats.damage = modifier.damageOverride;
            if (modifier.experienceOverride > 0)
                stats.experience_reward = modifier.experienceOverride;
        }

        return stats;
    }

    int CalculateHP(int stage, EnemyData baseData)
    {
        // HP = baseHP * (1 + growth * stage)^exponent
        float multiplier = Mathf.Pow(1 + scalingConfig.hpGrowth * stage, scalingConfig.hpExponent);

        // 적 타입별 배율 적용
        float typeMultiplier = baseData.hpScalingMultiplier;

        return Mathf.RoundToInt(scalingConfig.baseHP * multiplier * typeMultiplier);
    }

    float CalculateSpeed(int stage, EnemyData baseData)
    {
        float speed = scalingConfig.baseSpeed * (1 + scalingConfig.speedGrowth * stage);
        speed *= baseData.speedScalingMultiplier;
        return Mathf.Min(speed, scalingConfig.speedCap);
    }

    int CalculateDamage(int stage, EnemyData baseData)
    {
        float multiplier = Mathf.Pow(1 + scalingConfig.damageGrowth * stage, scalingConfig.damageExponent);
        float typeMultiplier = baseData.damageScalingMultiplier;

        return Mathf.RoundToInt(scalingConfig.baseDamage * multiplier * typeMultiplier);
    }

    int CalculateExperience(int stage, EnemyData baseData)
    {
        return Mathf.RoundToInt(scalingConfig.baseExperience * (1 + scalingConfig.experienceGrowth * stage));
    }

    StageStatModifier GetModifierForStage(int stage)
    {
        if (scalingConfig.stageModifiers == null)
            return null;

        foreach (var modifier in scalingConfig.stageModifiers)
        {
            if (modifier.stageNumber == stage)
                return modifier;
        }
        return null;
    }
}