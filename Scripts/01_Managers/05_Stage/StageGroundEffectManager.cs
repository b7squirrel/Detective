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

    // ⭐ 추가: 일반 스테이지(Stage씬)가 언로드되면서 이 컴포넌트가 파괴될 때
    //          IsIceStage를 항상 false로 리셋한다.
    //          static 필드라 씬이 바뀌어도 값이 저절로 초기화되지 않기 때문에,
    //          이 리셋이 없으면 얼음 스테이지를 플레이한 뒤 무한 모드로 진입했을 때
    //          (무한 모드 Stage씬에는 StageGroundEffectManager가 없어서 Init()이
    //          호출되지 않음) 이전 값 true가 그대로 남아 얼음 그림자가 잘못 적용됨.
    void OnDestroy()
    {
        IsIceStage = false;
    }
}