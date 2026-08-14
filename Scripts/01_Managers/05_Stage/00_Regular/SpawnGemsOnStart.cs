using UnityEngine;
using UnityEngine.Profiling;
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
        innerRadius = 11.9f;
        outerRadius = 12f;
    }

    public void InitChestData(GameObject _chestPrefab, float _innerR, float _outerR)
    {
        chestPrefab = _chestPrefab;
        innerRadiusForChest = 11f;
        outerRadiusForChest = 13f;
    }

    // ⭐ 코루틴 제거 - 애니메이션 이벤트가 정확한 프레임에 직접 호출하므로
    // 별도의 딜레이 없이 즉시 실행
    public void GenGemsAndChest()
{
    var sw = System.Diagnostics.Stopwatch.StartNew();

    if (manager == null)
        manager = FindObjectOfType<GameManager>();

    bool hideItems = GameConfig.Instance != null && GameConfig.Instance.hideFieldItems;

    if (!hideItems)
    {
        for (int i = 0; i < numbersOfGemToSpawn; i++)
        {
            Vector2 posGem = new GeneralFuctions().GetRandomPointInRing(Vector2.zero, outerRadius, innerRadius);
            GameObject gem = manager.poolManager.GetMisc(gemToSpawn);
            gem.transform.position = posGem;
        }

        Vector2 posChest = new GeneralFuctions().GetRandomPointInRing(Vector2.zero, outerRadiusForChest, innerRadiusForChest);
        GameManager.instance.fieldItemSpawner.SpawnEggBox(posChest);
        CameraShake.instance.Shake();
    }

    sw.Stop();
    Logger.Log($"[SpawnGemsOnStart] GenGemsAndChest 소요 시간: {sw.Elapsed.TotalMilliseconds:F2}ms");
}
}