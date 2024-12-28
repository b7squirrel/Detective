using UnityEngine;

public class SpawnGemsOnStart : MonoBehaviour
{
    [SerializeField] int numbersOfGemToSpawn;
    [SerializeField] GameObject gemToSpawn;
    [SerializeField] float innerRadius;
    [SerializeField] float outerRadius;
    GameManager manager;

    [Header("Chest")]
    [SerializeField] GameObject chestPrefab;
    [SerializeField] float innerRadiusForChest;
    [SerializeField] float outerRadiusForChest;

    public void InitGemData(GameObject _gemToSpawn, int _gemNums, float _innerR, float _outerR)
    {
        gemToSpawn = _gemToSpawn;
        numbersOfGemToSpawn = _gemNums;
        innerRadius = _innerR;
        outerRadius = _outerR;
    }
    public void InitChestData(GameObject _chestPrefab, float _innerR, float _outerR)
    {
        chestPrefab = _chestPrefab;
        innerRadiusForChest = _innerR;
        outerRadiusForChest = _outerR;
    }

    public void GenGemsAndChest()
    {
        manager = FindObjectOfType<GameManager>();
        for (int i = 0; i < numbersOfGemToSpawn; i++)
        {
            Vector2 posGem =
                new GeneralFuctions().GetRandomPointInRing(Vector2.zero, outerRadius, innerRadius);
            GameObject gem = manager.poolManager.GetMisc(gemToSpawn);
            gem.transform.position = posGem;
        }

        Vector2 posChest =
            new GeneralFuctions().GetRandomPointInRing(Vector2.zero, outerRadiusForChest, innerRadiusForChest);
        GameObject chest = manager.poolManager.GetMisc(chestPrefab);
        chest.transform.position = posChest;
    }
}