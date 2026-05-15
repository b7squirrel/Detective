using UnityEngine;

public class StageGroundEffectManager : MonoBehaviour
{
    [SerializeField] float desertSlowFactor = 0.6f;
    [SerializeField] float iceSlideDecay = 0.92f; // 1에 가까울수록 더 많이 미끄러짐
    public static bool IsIceStage { get; private set; }

    Player player;
    Character character;
    LavaVolcanoSpawner lavaVolcanoSpawner;

    public void Init(StageGroundType groundType)
    {
        player = FindObjectOfType<Player>();
        character = FindObjectOfType<Character>();
        lavaVolcanoSpawner = GetComponent<LavaVolcanoSpawner>();

        IsIceStage = (groundType == StageGroundType.BlueIce);

        // WeaponContainerPrefab처럼 이미 씬에 있는 오브젝트에도 즉시 적용
        ShadowController[] shadowControllers = FindObjectsOfType<ShadowController>();
        foreach (var sc in shadowControllers)
            sc.ApplyShadow(IsIceStage);

        switch (groundType)
        {
            case StageGroundType.OrangeDesert:
                player.SetSlowDownFactor(desertSlowFactor);
                break;
            case StageGroundType.BlueIce:
                player.EnableIceMode(true, iceSlideDecay);
                break;
            case StageGroundType.GreyLava:
                lavaVolcanoSpawner.StartSpawning();
                break;
            default:
                break;
        }
    }
}