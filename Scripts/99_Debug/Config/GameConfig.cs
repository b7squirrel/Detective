using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "QuackSurvivors/GameConfig")]
public class GameConfig : ScriptableObject
{
    private static GameConfig _instance;

    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<GameConfig>("GameConfig");
            return _instance;
        }
    }

    [Header("Debug")]
    public bool isDebugMode = true;

    [Header("IAP")]
    public bool enableIAPTestMode = true;

    [Header("Field UI")]
    public bool hideFieldUI = false;
    public bool hideCursor = false;

    [Header("필드에서 상자와 보석 제어")]
    public bool hideFieldItems; // gem, chest 시작 배치 숨기기
    public bool hidePeriodicChest; // 주기적 EggBox 스폰 숨기기

    private void OnEnable()
    {
        _instance = this;
    }
}