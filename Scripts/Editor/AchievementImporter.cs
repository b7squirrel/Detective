using System.IO;
using UnityEditor;
using UnityEngine;

public class AchievementImporter : EditorWindow
{
    private const string CSV_PATH = "Assets/Data/Achievements/achievement.csv";
    private const string SAVE_PATH = "Assets/Data/Achievements/SO/";

    [MenuItem("Tools/Import/Achievement CSV → SO")]
    public static void ImportAchievementCSV()
    {
        if (!File.Exists(CSV_PATH))
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + CSV_PATH);
            return;
        }

        if (!Directory.Exists(SAVE_PATH))
            Directory.CreateDirectory(SAVE_PATH);

        string[] lines = File.ReadAllLines(CSV_PATH);

        for (int i = 1; i < lines.Length; i++) // 헤더 스킵
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue; // 빈 줄 스킵

            string[] row = lines[i].Split(',');

            if (row.Length < 6)
            {
                Debug.LogWarning($"필드 부족: line {i+1}");
                continue;
            }

            string id = row[0].Trim();
            string title = row[1].Trim();
            string desc = row[2].Trim();
            string typeString = row[4].Trim();
            string icon = row.Length > 6 ? row[6].Trim() : "";

            // 숫자 파싱
            if (!int.TryParse(row[3].Trim(), out int target))
            {
                Debug.LogWarning($"Target parse failed: '{row[3]}' at line {i+1}");
                continue;
            }

            if (!int.TryParse(row[5].Trim(), out int reward))
            {
                Debug.LogWarning($"Reward parse failed: '{row[5]}' at line {i+1}");
                continue;
            }

            var so = ScriptableObject.CreateInstance<AchievementSO>();
            so.id = id;
            so.title = title;
            so.description = desc;
            so.targetValue = target;
            so.rewardGem = reward;
            so.icon = icon;

            if (System.Enum.TryParse(typeString, out AchievementType result))
                so.type = result;

            string assetPath = $"{SAVE_PATH}{id}.asset";
            AssetDatabase.CreateAsset(so, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ Achievement ScriptableObject 생성 완료!");
    }
}
