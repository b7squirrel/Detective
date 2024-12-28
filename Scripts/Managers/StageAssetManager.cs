using UnityEngine;

public class StageAssetManager : MonoBehaviour
{
    public GameObject[] enemies;
    public GameObject bossPrefab;
    public GameObject[] effects;
    public GameObject[] bossEffects;

    public void Init(GameObject[] _enemies, GameObject _bossPrefab, GameObject[] _effects, GameObject[] _bossEffects)
    {
        enemies = _enemies;
        bossPrefab = _bossPrefab;
        effects = _effects;
        bossEffects = _bossEffects;
    }

    public GameObject GetBoss()
    {
        return bossPrefab;
    }
}
