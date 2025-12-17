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
    [Tooltip("HP = baseHP * (1 + hpGrowth * stage)^hpExponent")]
    public float hpGrowth = 0.15f; // 스테이지당 15% 증가
    public float hpExponent = 1.1f; // 지수 성장
    
    [Range(0f, 0.2f)]
    public float speedGrowth = 0.05f; // 스테이지당 5% 증가
    public float speedCap = 15f; // 최대 속도 제한
    
    public float damageGrowth = 0.18f; // 스테이지당 18% 증가
    public float damageExponent = 1.05f;
    
    public float experienceGrowth = 0.25f; // 스테이지당 25% 증가
    
    [Header("Enemy Type Specific")]
    [Tooltip("탱커형 적의 추가 HP 보너스")]
    public float tankHPBonus = .9f; // 90% 추가
    
    [Tooltip("어택커형 적의 추가 데미지 보너스")]
    public float attackerDamageBonus = 0.15f; // 15% 추가
    
    [Header("Manual Overrides Per Stage")]
    public StageStatModifier[] stageModifiers;
}

[System.Serializable]
public class StageStatModifier
{
    public int stageNumber;
    [Tooltip("Leave 0 to use formula")]
    public int hpOverride;
    public float speedOverride;
    public int damageOverride;
    public int experienceOverride;
}