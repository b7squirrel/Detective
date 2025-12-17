using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Scaling Config", menuName = "Game/Enemy Scaling Config")]
public class EnemyScalingConfig : ScriptableObject
{
    [Header("Base Stats (Stage 1)")]
    public int baseHP = 100;
    public float baseSpeed = 5f;
    public int baseDamage = 1;
    public int baseExperience = 10;
    
    [Header("Scaling Formulas")]
    [Tooltip("HP = baseHP * (1 + hpGrowth * stage)^hpExponent")]
    public float hpGrowth = 0.15f; // 15% per stage
    public float hpExponent = 1.1f; // 지수 성장
    
    public float speedGrowth = 0.05f; // 5% per stage
    public float speedCap = 15f; // 최대 속도 제한
    
    public float damageGrowth = 0.12f;
    public float damageExponent = 1.0f;
    
    public float experienceGrowth = 0.2f;
    
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