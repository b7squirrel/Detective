using UnityEngine;

public class StageGroundEffectManager : MonoBehaviour
{
    [SerializeField] float desertSlowFactor = 0.6f;
    [SerializeField] float iceSlideDecay = 0.92f; // 1에 가까울수록 더 많이 미끄러짐
    public static bool IsIceStage { get; private set; }
    DesertWindManager desertWindManager;
    LavaVolcanoSpawner lavaVolcanoSpawner;
    EarthquakeManager earthquakeManager;
    FireballManager fireballManager;
    SnowManager snowManager; 

    Player player;
    Character character;

    public void Init(StageGroundType groundType)
    {
        player = FindObjectOfType<Player>();
        character = FindObjectOfType<Character>();
        lavaVolcanoSpawner = GetComponent<LavaVolcanoSpawner>();
        desertWindManager = GetComponent<DesertWindManager>();
        earthquakeManager = GetComponent<EarthquakeManager>(); 
        fireballManager = GetComponent<FireballManager>();
        snowManager = GetComponent<SnowManager>();

        IsIceStage = (groundType == StageGroundType.BlueIce);

        ShadowController[] shadowControllers = FindObjectsOfType<ShadowController>();
        foreach (var sc in shadowControllers)
            sc.ApplyShadow(IsIceStage);

        switch (groundType)
        {
            case StageGroundType.OrangeDesert:
                desertWindManager.StartWind(); // SetSlowDownFactor 대신 교체
                break;
            case StageGroundType.BlueIce:
                player.EnableIceMode(true, iceSlideDecay);
                snowManager.StartSnow();
                break;
            case StageGroundType.GreyStone: // 추가
                earthquakeManager.StartEarthquake();
                break;
            case StageGroundType.GreyLava:
                fireballManager.StartSpawning();
                break;
            default:
                break;
        }
    }
}