using UnityEngine;

[CreateAssetMenu]
public class EnemyData : ScriptableObject
{
    public string Name;
    public RuntimeAnimatorController animController;
    public EnemyStats stats;
}
