using UnityEngine;

public class StageAssetManager : MonoBehaviour
{
    public GameObject[] enemies;
    public GameObject bossPrefab;
    public GameObject[] effects;
    public GameObject[] bossEffects;

    public GameObject GetBoss()
    {
        return bossPrefab;
    }
}
