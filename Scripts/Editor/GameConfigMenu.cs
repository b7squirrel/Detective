#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class GameConfigMenu
{
    private const string DebugModePath  = "Tools/QuackSurvivors/Config/Debug Mode";
    private const string IAPTestModePath = "Tools/QuackSurvivors/Config/IAP Test Mode";

    // ── Debug Mode ──────────────────────────────

    [MenuItem(DebugModePath)]
    private static void ToggleDebugMode()
    {
        var config = LoadConfig();
        if (config == null) return;

        config.isDebugMode = !config.isDebugMode;
        Save(config);
        Debug.Log($"[GameConfig] Debug Mode: {config.isDebugMode}");
    }

    [MenuItem(DebugModePath, true)]
    private static bool ValidateDebugMode()
    {
        var config = LoadConfig();
        if (config == null) return false;
        Menu.SetChecked(DebugModePath, config.isDebugMode);
        return true;
    }

    // ── IAP Test Mode ───────────────────────────

    [MenuItem(IAPTestModePath)]
    private static void ToggleIAPTestMode()
    {
        var config = LoadConfig();
        if (config == null) return;

        config.enableIAPTestMode = !config.enableIAPTestMode;
        Save(config);
        Debug.Log($"[GameConfig] IAP Test Mode: {config.enableIAPTestMode}");
    }

    [MenuItem(IAPTestModePath, true)]
    private static bool ValidateIAPTestMode()
    {
        var config = LoadConfig();
        if (config == null) return false;
        Menu.SetChecked(IAPTestModePath, config.enableIAPTestMode);
        return true;
    }

    // ── 공통 유틸 ────────────────────────────────

    private static GameConfig LoadConfig()
    {
        var config = Resources.Load<GameConfig>("GameConfig");
        if (config == null)
            Debug.LogWarning("[GameConfig] Resources/GameConfig.asset 을 찾을 수 없습니다!");
        return config;
    }

    private static void Save(GameConfig config)
    {
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
    }
}
#endif