#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// DEV : apk, TEST : AAB, REAL : AAB
public enum BuildType
{
    DEV,
    TEST,
    REAL
}

public class BuildManager : Editor
{
    public const string DEV_SCRIPTING_DEFINE_SYMBOLS = "UNITY_ANDROID;DOTWEEN;DEV_VER";
    public const string REAL_SCRIPTING_DEFINE_SYMBOLS = "UNITY_ANDROID;DOTWEEN";
    static BuildType m_buildType = BuildType.DEV;

    // 빌드 실패시 버전 코드를 증가시켰다면 다시 되돌리기 위한 변수들
    static int s_prevVersionCode = -1;
    static bool s_versionCodeIncreased = false;

    [MenuItem("Build/1. Set AOS DEV Build Settings")]
    public static void SetAOSDEVBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, DEV_SCRIPTING_DEFINE_SYMBOLS);

        m_buildType = BuildType.DEV;
    }

    [MenuItem("Build/2. Set AOS TEST Build Settings")]
    public static void SetAOSTESTBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, DEV_SCRIPTING_DEFINE_SYMBOLS);

        m_buildType = BuildType.TEST;
    }

    [MenuItem("Build/3. Set AOS REAL Build Settings")]
    public static void SetAOSREALBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, REAL_SCRIPTING_DEFINE_SYMBOLS);

        m_buildType = BuildType.REAL;
    }

    [MenuItem("Build/- Start AOS Build")]
    public static void StartAOSBuild()
    {
        TryIncreaseVersionCode();

        PlayerSettings.Android.keystoreName = "Builds/AOS/user.keystore";
        PlayerSettings.Android.keystorePass = "b7dotori!";
        PlayerSettings.Android.keyaliasName = "io";
        PlayerSettings.Android.keyaliasPass = "b7dotori!";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[]
        {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/ESSential.unity",
        "Assets/Scenes/Stage.unity",
        "Assets/Scenes/InfiniteStage.unity" 
    };
        buildPlayerOptions.target = BuildTarget.Android;
        string fileExtention = string.Empty;
        BuildOptions compressOption = BuildOptions.None;

        switch (m_buildType)
        {
            case BuildType.DEV:
                fileExtention = "apk";
                compressOption = BuildOptions.CompressWithLz4;
                break;
            case BuildType.TEST:
            case BuildType.REAL:
                fileExtention = "aab";
                compressOption = BuildOptions.CompressWithLz4HC;
                break;
            default:
                break;
        }
        buildPlayerOptions.locationPathName = $"Builds/Aos/QS_{Application.version}_{DateTime.Now.ToString("yyMMdd_HHmmss")}.{fileExtention}";
        buildPlayerOptions.options = compressOption;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"빌드 성공. {summary.totalSize} bytes.");
        }
        else if (summary.result == BuildResult.Failed)
        {
            RollbackVersionCodeIfNeeded();
            Debug.LogError($"빌드 실패!");
        }
    }

    static void TryIncreaseVersionCode()
    {
        s_versionCodeIncreased = false;
        s_prevVersionCode = -1;

        // TEST / REAL (AAB)만 증가
        if (m_buildType != BuildType.TEST && m_buildType != BuildType.REAL)
        {
            Debug.Log("[Build] DEV 빌드 → Version Code 증가 안 함");
            return;
        }

        s_prevVersionCode = PlayerSettings.Android.bundleVersionCode;
        int nextCode = s_prevVersionCode + 1;

        PlayerSettings.Android.bundleVersionCode = nextCode;
        s_versionCodeIncreased = true;

        Debug.Log(
            $"[Build] Version Code 증가: {s_prevVersionCode} → {nextCode}");
    }
    static void RollbackVersionCodeIfNeeded()
    {
        if (!s_versionCodeIncreased)
            return;

        PlayerSettings.Android.bundleVersionCode = s_prevVersionCode;

        Debug.LogWarning(
            $"[Build] 빌드 실패 → Version Code 롤백: " +
            $"{s_prevVersionCode}");
    }
}
#endif