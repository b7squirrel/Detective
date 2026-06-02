using UnityEditor;
using System.IO;
using UnityEngine;

public class BatchRenameFriends
{
    [MenuItem("Tools/Rename 99_Friends Assets (Add F)")]
    static void Rename()
    {
        string folderPath = "Assets/Data/Weapons_Items/01_Weapon/99_Friends";
        string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });

        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // 파일만 처리 (폴더 제외)
            if (AssetDatabase.IsValidFolder(path)) continue;

            string oldName = Path.GetFileNameWithoutExtension(path);

            // 첫 번째 '_' 위치 찾기
            int underscoreIndex = oldName.IndexOf('_');
            if (underscoreIndex < 0) continue; // '_' 없으면 스킵

            // 이미 F가 붙어있으면 스킵
            if (underscoreIndex > 0 && oldName[underscoreIndex - 1] == 'F') continue;

            // 무기이름 + F + 나머지
            string newName = oldName.Substring(0, underscoreIndex) + "F" + oldName.Substring(underscoreIndex);

            string error = AssetDatabase.RenameAsset(path, newName);
            if (string.IsNullOrEmpty(error))
            {
                count++;
            }
            else
            {
                Debug.LogWarning($"Rename failed: {oldName} → {newName} | {error}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ 완료! {count}개 파일 이름 변경됨.");
    }
}