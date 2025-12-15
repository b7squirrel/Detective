using UnityEngine;

public enum EnemyVariant {
  NORMAL,      // 일반
  JUMPER,      // 점프하는
  ATTACKER,    // 공격력이 강한
  DEFENDER     // 방어력이 강한
}

[System.Serializable]
public class EnemyDataContainer
{
    public EnemyData[] enemyDatas;
}

/// <summary>
/// Stage Manager의 Stage Contents 클래스가 참조
/// </summary>
public class EnemyDataManager : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyDataContainer[] enemyDataContainers; // 적의 데이터 입력 받기

    public EnemyDataContainer GetEnemyDataContainer(EnemyVariant enemyVariant)
    {
        if(enemyVariant == EnemyVariant.NORMAL) return enemyDataContainers[0];
        if(enemyVariant == EnemyVariant.JUMPER) return enemyDataContainers[1];
        if(enemyVariant == EnemyVariant.ATTACKER) return enemyDataContainers[2];
        if(enemyVariant == EnemyVariant.DEFENDER) return enemyDataContainers[3];
        return null;
    }
}
