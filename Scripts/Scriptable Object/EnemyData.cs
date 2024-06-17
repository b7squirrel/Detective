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

    [Header("Ranged")]
    public float distanceToPlayer; // �÷��̾�� ������ ������ �Ÿ�
    public float attackInterval; // ���� ���� �ð� ����
}