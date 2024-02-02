using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Object = UnityEngine.Object;

public class UTF8Changer : Editor
{
    private const string SYMBOL = "AUTOUTF8Convert";

    private static string Key =>
        Application.companyName + "." + Application.productName + "." + nameof(UTF8Changer);

    [MenuItem("Assets/UFT8 변환하기")]
    public static void ConvertUTF8MenuItem()
    {
        Object[] targets = Selection.objects;

        foreach (Object target in targets)
            ConvertTask(target);

        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/UTF8 Auto Convert/Enable", true)]
    private static bool ToggleUTF8MenuItemValidate()
    {
        Menu.SetChecked("Tools/UTF8 Auto Convert/Enable", EditorPrefs.GetBool(Key, false));
        return true;
    }

    [MenuItem("Tools/UTF8 Auto Convert/Enable")]
    public static void ToggleUTF8MenuItem()
    {
        bool isAutoConvert = EditorPrefs.GetBool(Key, false);
        isAutoConvert = !isAutoConvert;
        EditorPrefs.SetBool(Key, isAutoConvert);
        Menu.SetChecked("Tools/UTF8 Auto Convert/Enable", isAutoConvert);
        SetupDefine(isAutoConvert);
    }

    /// <summary>
    /// Moves the selected object to a new normalized path.
    /// </summary>
    /// <param name="target">The selected object to be moved.</param>
    private static void ConvertTask(Object target)
    {
        // 상대 경로를 가져옵니다.
        string path = AssetDatabase.GetAssetPath(target);

        // 선택한 파일이 폴더가 아니면
        if (!Directory.Exists(path))
        {
            // CS 파일이라면,
            if (path.EndsWith(".cs"))
            {
                // Window-949로 인코딩된 파일을 UTF-8로 읽기
                string text = File.ReadAllText(path, Encoding.GetEncoding("euc-kr"));

                // UTF-8로 인코딩된 파일로 저장
                File.WriteAllText(path, text, Encoding.UTF8);
            }
        }

        IEnumerable<Object> getChildAssets = GetChildObjects(target);

        if (getChildAssets == null) return;

        // 자식 녀석들까지 전부 처리
        foreach (Object childAssets in getChildAssets)
            ConvertTask(childAssets);
    }

    /// <summary>
    /// Returns a collection of selected objects and their child objects.
    /// </summary>
    /// <param name="target">The selected object.</param>
    /// <returns>A collection of selected objects and their child objects.</returns>
    private static IEnumerable<Object> GetChildObjects(Object target)
    {
        string path = AssetDatabase.GetAssetPath(target);

        // 해당 경로가 폴더가 아니면 null을 반환합니다.
        if (!Directory.Exists(path)) return null;

        string[] assetPaths = Directory.GetFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly);

        // 필터링된 에셋 경로를 담을 리스트, .meta 파일 필터링하여 나머지 파일들의 경로를 리스트에 추가
        List<string> filteredAssetPaths = assetPaths.Where(filePath => !filePath.EndsWith(".meta")).ToList();

        Object[] result = new Object[filteredAssetPaths.Count];

        for (int i = 0; i < filteredAssetPaths.Count; i++)
            result[i] = AssetDatabase.LoadAssetAtPath(filteredAssetPaths[i], typeof(Object));

        return result;
    }

    /// <summary>
    /// Sets up the define symbols for the selected build target group based on the provided activation status.
    /// </summary>
    /// <param name="active">The activation status of the define symbols. True to activate the define symbol, false to deactivate it.</param>
    private static void SetupDefine(bool active)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        
#if UNITY_2023
        List<string> defines =
            PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup)).Split(';')
                .ToList();
#else
        List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';').ToList();
#endif

        //Scriptable 값에 따른 Define값 적용 
        if (active)
            defines.Add(SYMBOL);
        else
            defines.Remove(SYMBOL);

        defines = defines.Distinct().ToList();

#if UNITY_2023
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), string.Join(";", defines.ToArray()));
#else
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", defines.ToArray()));
#endif
    }
}

#if AUTOUTF8Convert
public class UTF8ConvertService : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            // CS 파일이라면,
            if (importedAsset.EndsWith(".cs"))
            {
                // 이미 UTF-8이라면 무시
                if (IsUTF8Encoded(importedAsset))
                    return;

                // Window-949로 인코딩된 파일을 UTF-8로 읽기
                string text = File.ReadAllText(importedAsset, Encoding.GetEncoding("euc-kr"));

                // UTF-8로 인코딩된 파일로 저장
                File.WriteAllText(importedAsset, text, Encoding.UTF8);
            }
        }
    }

    /// <summary>
    /// Checks if a file is UTF-8 encoded.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>True if the file is UTF-8 encoded, false otherwise.</returns>
    static bool IsUTF8Encoded(string filePath)
    {
        try
        {
            using (StreamReader sr = new StreamReader(filePath, Encoding.Default, true))
            {
                // Read the first few bytes to check for UTF-8 BOM
                char[] bom = new char[4];
                sr.Read(bom, 0, 4);

                // Check for UTF-8 BOM (EF BB BF)
                return (bom[0] == '\uFEFF');
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error checking UTF-8 encoding: " + e.Message);
            return false;
        }
    }
}
#endif