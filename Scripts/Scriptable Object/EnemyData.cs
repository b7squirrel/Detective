using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyData : ScriptableObject
{
    [Header("Common")]
    public string Name;
    public RuntimeAnimatorController animController;
    public EnemyStats stats; // 이제 기본값으로만 사용
    public EnemyType enemyType;

    [Header("Scaling Multipliers")]
    [Tooltip("이 적 타입의 HP 스케일링 배율 (1.0 = 기본)")]
    public float hpScalingMultiplier = 1.0f;
    [Tooltip("이 적 타입의 속도 스케일링 배율 (1.0 = 기본)")]
    public float speedScalingMultiplier = 1.0f;
    [Tooltip("이 적 타입의 공격력 스케일링 배율 (1.0 = 기본)")]
    public float damageScalingMultiplier = 1.0f;

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