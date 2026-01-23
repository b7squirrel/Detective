using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyData : ScriptableObject
{
    [Header("Common")]
    public string Name;
    public RuntimeAnimatorController animController;
    public EnemyType enemyType;
    
    [Header("Enemy Role")]
    public EnemyRole enemyRole = EnemyRole.Balanced;
    
    [Header("Boss Type")]
    public BossType bossType = BossType.Normal;

    [Header("Special Ability")]
    [Tooltip("이 적의 특수 능력")]
    public SpecialAbility specialAbility = SpecialAbility.None;

    [Header("Scaling Multipliers")]
    [Tooltip("이 적 타입의 HP 스케일링 배율 (1.0 = 기본)")]
    [Range(0.5f, 15f)]
    public float hpScalingMultiplier = 1.0f;
    
    [Tooltip("이 적 타입의 속도 스케일링 배율 (1.0 = 기본)")]
    [Range(0.1f, 2.0f)]
    public float speedScalingMultiplier = 1.0f;
    
    [Tooltip("이 적 타입의 공격력 스케일링 배율 (1.0 = 기본)")]
    [Range(0.5f, 30f)]
    public float damageScalingMultiplier = 1.0f;

    [Header("Special Abilities - Dash")]
    [Tooltip("대시 쿨다운 (초)")]
    public float dashCooldown = 3f;

    [Tooltip("대시 속도")]
    public float dashSpeed = 15f;

    [Tooltip("대시 지속 시간 (초)")]
    public float dashDuration = 0.5f;

    //레이저 설정 추가
    [Header("Special Abilities - Laser")]
    [Tooltip("레이저 쿨다운 (초)")]
    public float laserCooldown = 5f;
    
    [Tooltip("레이저 예고 시간 (초) - 빨간 선 표시")]
    public float laserAnticipationTime = 1f;
    
    [Tooltip("레이저 발사 지속 시간 (초)")]
    public float laserFireDuration = 0.3f;
    
    [Tooltip("레이저 데미지")]
    public int laserDamage = 20;
    
    [Tooltip("레이저 사거리")]
    public float laserRange = 100f;
    
    [Tooltip("레이저 두께")]
    public float laserWidth = 0.2f;

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

// 적을 스폰할 때 어떤 타입의 적을 스폰할지 정하기 위해
public enum EnemyType
{
    Melee,
    Ranged,
    Explode,
    Projectile
}

public enum SpecialAbility
{
    None,      // 특수 능력 없음
    Dash,      // 대시 (빨강 - 어택커)
    Laser,     // 레이저 (보라 - 유리대포)
    Jump       // 점프 (파랑 - 탱커) - 기존 isJumper를 이것으로 대체 예정
}

public enum EnemyRole
{
    Balanced,    // 노랑 - 균형형
    Tank,        // 파랑 - 탱커
    Attacker,    // 빨강 - 어택커
    Ranged,      // 초록 - 원거리
    GlassCannon  // 보라 - 유리대포
}

public enum BossType
{
    Normal,      // 일반 적
    SubBoss,     // 중간 보스 (수염 달린 보스)
    StageBoss,   // 스테이지 보스 (해당 스테이지 최종 보스)
    QueenBoss    // 여왕 슬라임
}