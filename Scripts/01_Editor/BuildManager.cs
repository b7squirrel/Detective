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
        PlayerSettings.Android.keystoreName = "Builds/AOS/user.keystore";
        PlayerSettings.Android.keystorePass = "b7dotori!";
        PlayerSettings.Android.keyaliasName = "io";
        PlayerSettings.Android.keyaliasPass = "b7dotori!";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[]
        {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/ESSential.unity",
        "Assets/Scenes/Stage.unity"
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
            Debug.LogError($"빌드 실패!");
        }
    }
}
#endif