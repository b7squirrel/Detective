using UnityEngine;

/// <summary>
/// 무한 모드에서 사용할 적 정보
/// 프리팹, 데이터, 가중치를 하나의 구조로 관리
/// </summary>
[System.Serializable]
public class InfiniteEnemyConfig
{
    [Header("Enemy Setup")]
    public GameObject prefab;
    public EnemyData data;
    
    [Header("Spawn Settings")]
    [Tooltip("스폰 가중치 - 높을수록 자주 등장")]
    [Range(1, 10)]
    public int weight = 1;
    
    [Header("Wave Unlock")]
    [Tooltip("이 적이 등장하기 시작하는 웨이브 (0 = 처음부터)")]
    public int unlockWave = 0;
    
    /// <summary>
    /// 현재 웨이브에서 이 적이 스폰 가능한지 확인
    /// </summary>
    public bool IsAvailableAtWave(int currentWave)
    {
        return currentWave >= unlockWave;
    }
}