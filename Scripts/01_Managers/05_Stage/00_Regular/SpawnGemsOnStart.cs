using UnityEngine;
using System.Collections;


public class SpawnGemsOnStart : MonoBehaviour
{
    [SerializeField] int numbersOfGemToSpawn;
    [SerializeField] GameObject gemToSpawn;
    [SerializeField] float innerRadius = 11.9f;
    [SerializeField] float outerRadius = 12f;
    GameManager manager;

    [Header("Chest")]
    [SerializeField] GameObject chestPrefab;
    [SerializeField] float innerRadiusForChest = 11f;
    [SerializeField] float outerRadiusForChest = 18f;

    public void InitGemData(GameObject _gemToSpawn, int _gemNums, float _innerR, float _outerR)
    {
        gemToSpawn = _gemToSpawn;
        numbersOfGemToSpawn = _gemNums;
        // innerRadius = _innerR;
        // outerRadius = _outerR;
        innerRadius = 11.9f;
        outerRadius = 12f;
    }
    public void InitChestData(GameObject _chestPrefab, float _innerR, float _outerR)
    {
        chestPrefab = _chestPrefab;
        // innerRadiusForChest = _innerR;
        // outerRadiusForChest = _outerR;
        innerRadiusForChest = 11f;
        outerRadiusForChest = 13f;
    }

    public void GenGemsAndChest()
    {
        StartCoroutine(SpawnGemsAndChestCo());
    }

    IEnumerator SpawnGemsAndChestCo()
    {
        yield return new WaitForSeconds(.8f);
        
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

        GameManager.instance.fieldItemSpawner.SpawnEggBox(posChest);

        CameraShake.instance.Shake();
    }
}