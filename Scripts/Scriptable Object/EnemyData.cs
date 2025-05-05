using UnityEngine;

[CreateAssetMenu]
public class EnemyData : ScriptableObject
{
    [Header("Common")]
    public string Name;
    public RuntimeAnimatorController animController;
    public EnemyStats stats;
    public EnemyType enemyType;

    [Header("Prefabs")]
    public GameObject projectilePrefab;
    public GameObject dieEffectPrefab;
    //public EnemyColor enemyColor;
    public Color enemyColor;
    public Color enemyColorHighlight;

    [Header("Ranged")]
    public float distanceToPlayer; // 플레이어에게 공격을 시작할 거리
    public float attackInterval; // 범위 공격 시간 간격

    [Header("점프")]
    public bool canJump; // 점프하는 캐릭터인지
    public float jumpInterval; // 점프 주기
}