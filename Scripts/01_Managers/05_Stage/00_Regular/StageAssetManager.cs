using UnityEngine;

public class StageAssetManager : MonoBehaviour
{
    [Header("공통 적 프리팹 (모든 스테이지 공유)")]
    public GameObject[] enemies;        // ✅ Inspector에서 한 번만 할당
    public GameObject[] subBossEnemies; // ✅ Inspector에서 한 번만 할당

     // 스테이지별로 바뀌는 것들
    public GameObject bossPrefab;
    public GameObject[] effects;
    public GameObject[] bossEffects;

    // ✅ enemies, subBossEnemies는 파라미터에서 제거
    public void Init(GameObject _bossPrefab, GameObject[] _effects, GameObject[] _bossEffects)
    {
        bossPrefab = _bossPrefab;
        effects = _effects;
        bossEffects = _bossEffects;
        // enemies, subBossEnemies는 Inspector 할당값 그대로 유지
    }

    public GameObject GetBoss()
    {
        return bossPrefab;
    }

    public int GetSubBossIndex(string enemyName)
    {
        for (int i = 0; i < subBossEnemies.Length; i++)
        {
            if (subBossEnemies[i].name == enemyName) return i;
        }
        Logger.LogError($"[StageAssetManager] SubBoss not found: {enemyName}");
        return 0;
    }
}
