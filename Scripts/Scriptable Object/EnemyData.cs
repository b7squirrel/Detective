using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyData : ScriptableObject
{
    [Header("Common")]
    public string Name;
    public RuntimeAnimatorController animController;
    public EnemyStats stats;
    public EnemyType enemyType;
    
    [Header("Enemy Role")]
    public EnemyRole enemyRole = EnemyRole.Balanced;

    [Header("Scaling Multipliers")]
    [Tooltip("이 적 타입의 HP 스케일링 배율 (1.0 = 기본)")]
    [Range(0.5f, 15f)]
    public float hpScalingMultiplier = 1.0f;
    
    [Tooltip("이 적 타입의 속도 스케일링 배율 (1.0 = 기본)")]
    [Range(0.1f, 5f)]
    public float speedScalingMultiplier = 1.0f;
    
    [Tooltip("이 적 타입의 공격력 스케일링 배율 (1.0 = 기본)")]
    [Range(0.5f, 10f)]
    public float damageScalingMultiplier = 1.0f;
    
    [Header("Special Abilities")]
    [Tooltip("대시 능력 (빨강)")]
    public bool canDash = false;
    public float dashCooldown = 3f;
    public float dashSpeed = 15f;
    
    [Tooltip("점프 공격 (파랑)")]
    public bool hasJumpAttack = false;

    [Header("Prefabs")]
    public GameObject projectilePrefab;
    public GameObject splitDieEffectPrefab;
    public Color enemyColor;
    public Color enemyColorHighlight;

    [Header("Ranged")]
    public float distanceToPlayer;
    public float attackInterval;

    [Header("점프")]
    public bool isJumper;
    public float jumpInterval;

    [Header("보스 대사")]
    public List<string> dialogs;

    [Header("쪼개짐")]
    public EnemyData split;
    public int splitNum;

    [Header("사운드")]
    public AudioClip hitSound;
    public AudioClip dieSound;
}

public enum EnemyRole
{
    Balanced,    // 노랑 - 균형형
    Tank,        // 파랑 - 탱커
    Attacker,    // 빨강 - 어택커
    Ranged,      // 초록 - 원거리
    GlassCannon  // 보라 - 유리대포
}