using UnityEngine;
using UnityEditor;
using System.IO;

public class RenameFriendsAssets : EditorWindow
{
    [MenuItem("Tools/Rename Friends Assets")]
    public static void RenameAssets()
    {
        var rules = new (string folder, string oldPrefix, string newPrefix)[]
        {
            ("08_Yoyo",    "HoopF",   "YoyoF"),
            ("10_Nano",    "PartyF",  "NanoF"),
            ("11_Origami", "PlaneF",  "OrigamiF"),
            ("12_Whistle", "PunchF",  "WhistleF"),
        };

        string basePath = "Assets/Resources/Weapons/Friends";
        int totalRenamed = 0;

        foreach (var rule in rules)
        {
            string folderPath = $"{basePath}/{rule.folder}";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"[RenameFriendsAssets] 폴더 없음: {folderPath}");
                continue;
            }

            // 해당 폴더의 모든 에셋 GUID 가져오기
            string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string filename = Path.GetFileNameWithoutExtension(assetPath);

                if (!filename.StartsWith(rule.oldPrefix))
                    continue;

                string newFilename = rule.newPrefix + filename.Substring(rule.oldPrefix.Length);
                string error = AssetDatabase.RenameAsset(assetPath, newFilename);

                if (string.IsNullOrEmpty(error))
                {
                    Debug.Log($"[{rule.folder}] {filename} → {newFilename}");
                    totalRenamed++;
                }
                else
                {
                    Debug.LogError($"[RenameFriendsAssets] 실패: {filename} → {error}");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[RenameFriendsAssets] 완료! 총 {totalRenamed}개 파일 변경됨.");
        EditorUtility.DisplayDialog("완료", $"총 {totalRenamed}개 파일 이름 변경 완료!", "확인");
    }
}