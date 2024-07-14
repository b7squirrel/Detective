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

    void Start()
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