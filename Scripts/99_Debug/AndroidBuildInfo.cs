using UnityEngine;

public static class AndroidBuildInfo
{
    public static string GetVersionCode()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var pm = activity.Call<AndroidJavaObject>("getPackageManager"))
        using (var packageInfo = pm.Call<AndroidJavaObject>(
            "getPackageInfo",
            activity.Call<string>("getPackageName"), 0))
        {
            return packageInfo.Get<int>("versionCode").ToString();
        }
#else
        return "Editor";
#endif
    }
}
