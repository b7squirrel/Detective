using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyData : ScriptableObject
{
    [Header("Common")]
    public string Name; // 보스를 위한 이름
    public RuntimeAnimatorController animController;
    public EnemyStats stats;
    public EnemyType enemyType;

    [Header("Prefabs")]
    public GameObject projectilePrefab;
    public GameObject splitDieEffectPrefab; // 분리되며 사라지는 적의 이펙트
    //public EnemyColor enemyColor;
    public Color enemyColor;
    public Color enemyColorHighlight;

    [Header("Ranged")]
    public float distanceToPlayer; // 플레이어에게 공격을 시작할 거리
    public float attackInterval; // 범위 공격 시간 간격

    [Header("점프")]
    public bool isJumper; // 점프를 하는 적인지
    public float jumpInterval; // 점프 주기

    [Header("보스 대사")]
    public List<string> dialogs;

    [Header("쪼개짐")] // LV4처럼 쪼개지는 적
    public EnemyData split;
    public int splitNum; // 몇 개로 쪼개질지

    [Header("사운드")]
    public AudioClip hitSound;
    public AudioClip dieSound;
}