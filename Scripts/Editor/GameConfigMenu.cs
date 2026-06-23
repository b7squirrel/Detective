#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class GameConfigMenu
{
    private const string DebugModePath = "Tools/QuackSurvivors/Config/Debug Mode";
    private const string IAPTestModePath = "Tools/QuackSurvivors/Config/IAP Test Mode";
    private const string HideFieldUIPath = "Tools/QuackSurvivors/Config/Hide Field UI";

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

    // ── Hide Field UI ───────────────────────────

    [MenuItem(HideFieldUIPath)]
    private static void ToggleHideFieldUI()
    {
        var config = LoadConfig();
        if (config == null) return;

        config.hideFieldUI = !config.hideFieldUI;
        Save(config);
        
        Debug.Log($"[GameConfig] Hide Field UI: {config.hideFieldUI}");
    }

    [MenuItem(HideFieldUIPath, true)]
    private static bool ValidateHideFieldUI()
    {
        var config = LoadConfig();
        if (config == null) return false;
        Menu.SetChecked(HideFieldUIPath, config.hideFieldUI);
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