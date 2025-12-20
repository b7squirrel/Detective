using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Scaling Config", menuName = "Game/Enemy Scaling Config")]
public class EnemyScalingConfig : ScriptableObject
{
    [Header("Base Stats (Stage 1 - Yellow Slime 기준)")]
    public int baseHP = 60;
    public float baseSpeed = 4f;
    public int baseMeleeDamage = 9;
    public int baseRangedDamage = 100;
    public int baseExperience = 130;
    
    [Header("Scaling Formulas")]
    public float hpGrowth = 0.2f;
    public float hpExponent = 1.15f;
    
    [Range(0f, 0.2f)]
    public float speedGrowth = 0.08f;
    public float speedCap = 15f;
    
    public float damageGrowth = 0.22f;
    public float damageExponent = 1.1f;
    
    public float experienceGrowth = 0.3f;
    
    [Header("Enemy Type Specific")]
    public float tankHPBonus = 0.3f;
    public float attackerDamageBonus = 0.2f;
    
    [Header("Boss Multipliers")]
    [Tooltip("중간 보스 배율 (기존 데이터 기준: 15~25배)")]
    public float subBossMultiplier = 20.0f; // 평균 20배
    
    [Tooltip("스테이지 보스 배율 (서브보스의 추가 배율)")]
    public float stageBossMultiplier = 2.5f; // 서브보스의 2.5배 (총 50배)
    
    [Tooltip("여왕 슬라임 배율")]
    public float queenBossMultiplier = 80.0f;
    
    [Tooltip("보스의 속도 증가 배율")]
    public float bossSpeedBonus = 1.5f; // 보스는 1.5배 빠름
    
    [Tooltip("6스테이지마다 반복될 때 증가하는 배율")]
    [Range(0.1f, 1.0f)]
    public float cycleGrowth = 0.3f;
    
    [Header("Manual Overrides Per Stage")]
    public StageStatModifier[] stageModifiers;
}

/// <summary>
/// 특정 스테이지에서 공식 대신 수동으로 스탯을 설정
/// 일반적으로는 사용하지 않고, 예외적인 밸런싱이 필요할 때만 사용
/// </summary>
[System.Serializable]
public class StageStatModifier
{
    [Tooltip("조정할 스테이지 번호")]
    public int stageNumber;
    
    [Tooltip("HP 수동 설정 (0이면 공식 사용)")]
    public int hpOverride;
    
    [Tooltip("속도 수동 설정 (0이면 공식 사용)")]
    public float speedOverride;
    
    [Tooltip("데미지 수동 설정 (0이면 공식 사용)")]
    public int damageOverride;
    
    [Tooltip("경험치 수동 설정 (0이면 공식 사용)")]
    public int experienceOverride;
}