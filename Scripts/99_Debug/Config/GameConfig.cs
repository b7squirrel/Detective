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

    private void OnEnable()
    {
        _instance = this;
    }
}