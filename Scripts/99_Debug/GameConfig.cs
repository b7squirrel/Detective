using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "QuackSurvivors/GameConfig")]
public class GameConfig : ScriptableObject
{
    public static GameConfig Instance { get; private set; }

    [Header("Debug")]
    public bool isDebugMode = true;

    [Header("IAP")]
    public bool enableIAPTestMode = true;  // ← 추가

    private void OnEnable()
    {
        Instance = this;
    }
}