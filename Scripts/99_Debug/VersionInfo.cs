using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VersionInfo : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI versionText;

    void Start()
    {
        string buildType =
#if DEV_VER
    "DEV";
#else
    "RELEASE";
#endif

        string versionName = Application.version;
        string buildCode = "";

#if UNITY_EDITOR
        buildCode = PlayerSettings.Android.bundleVersionCode.ToString();
#elif UNITY_ANDROID
        buildCode = Application.version; // 대체용
#endif

        versionText.text =
    $"v{Application.version} ({buildCode}) [{buildType}]";
    }
}