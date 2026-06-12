#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;

[CustomEditor(typeof(AchievementTexts))]
public class AchievementTextsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AchievementTexts achievementTexts = (AchievementTexts)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "프로젝트의 모든 AchievementSO에서 자동으로 목록을 생성합니다.",
            MessageType.Info
        );

        if (GUILayout.Button("Auto-Populate from AchievementSO Assets", GUILayout.Height(30)))
            AutoPopulate(achievementTexts);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("CSV", EditorStyles.boldLabel);

        if (GUILayout.Button("Export CSV", GUILayout.Height(28)))
            ExportCSV(achievementTexts);

        if (GUILayout.Button("Import CSV", GUILayout.Height(28)))
            ImportCSV(achievementTexts);
    }

    // ─────────────────────────────────────────────────────────
    // Auto-Populate
    // ─────────────────────────────────────────────────────────
    private void AutoPopulate(AchievementTexts achievementTexts)
    {
        string[] guids = AssetDatabase.FindAssets("t:AchievementSO");

        var list = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<AchievementSO>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(a => a != null && !string.IsNullOrEmpty(a.id))
            .OrderBy(a => a.id)
            .ToList();

        if (list.Count == 0)
        {
            Debug.LogWarning("[AchievementTextsEditor] AchievementSO를 찾을 수 없습니다.");
            return;
        }

        // 기존 데이터 유지
        var existing = new System.Collections.Generic.Dictionary<string, AchievementTexts.AchievementLocalizedText>();
        if (achievementTexts.achievements != null)
        {
            foreach (var a in achievementTexts.achievements)
                if (a != null && !string.IsNullOrEmpty(a.achievementId))
                    existing[a.achievementId] = a;
        }

        achievementTexts.achievements = new AchievementTexts.AchievementLocalizedText[list.Count];

        int newCount = 0, keptCount = 0;
        for (int i = 0; i < list.Count; i++)
        {
            string id = list[i].id;
            if (existing.ContainsKey(id))
            {
                achievementTexts.achievements[i] = existing[id];
                keptCount++;
            }
            else
            {
                achievementTexts.achievements[i] = new AchievementTexts.AchievementLocalizedText
                {
                    achievementId = id,
                    title = "",
                    description = ""
                };
                newCount++;
            }
        }

        EditorUtility.SetDirty(achievementTexts);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AchievementTextsEditor] ✓ Auto-Populate 완료: 총 {list.Count}개 (신규: {newCount}, 유지: {keptCount})");
    }

    // ─────────────────────────────────────────────────────────
    // Export CSV
    // ─────────────────────────────────────────────────────────
    private void ExportCSV(AchievementTexts achievementTexts)
    {
        if (achievementTexts.achievements == null || achievementTexts.achievements.Length == 0)
        {
            EditorUtility.DisplayDialog("Export CSV", "데이터가 없습니다. 먼저 Auto-Populate를 실행해 주세요.", "확인");
            return;
        }

        string path = EditorUtility.SaveFilePanel(
            "Export Achievement Texts CSV",
            Application.dataPath,
            "AchievementTexts",
            "csv"
        );
        if (string.IsNullOrEmpty(path)) return;

        var sb = new StringBuilder();
        sb.AppendLine("achievementId,title_Korean,title_English,description_Korean,description_English");

        foreach (var a in achievementTexts.achievements)
        {
            if (a == null) continue;
            string title = (a.title ?? "").Replace("\"", "\"\"");
            string desc  = (a.description ?? "").Replace("\"", "\"\"").Replace("\n", "\\n");
            sb.AppendLine($"{a.achievementId},\"{title}\",,\"{desc}\",");
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[AchievementTextsEditor] ✓ CSV 내보내기 완료: {path}");
        EditorUtility.DisplayDialog("Export CSV", $"완료!\n{path}", "확인");
    }

    // ─────────────────────────────────────────────────────────
    // Import CSV
    // ─────────────────────────────────────────────────────────
    private void ImportCSV(AchievementTexts achievementTexts)
    {
        string path = EditorUtility.OpenFilePanel(
            "Import Achievement Texts CSV",
            Application.dataPath,
            "csv"
        );
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length < 2)
        {
            EditorUtility.DisplayDialog("Import CSV", "데이터가 없습니다.", "확인");
            return;
        }

        // 헤더 파싱
        string[] headers = ParseCSVLine(lines[0]);
        int idCol       = System.Array.IndexOf(headers, "achievementId");
        int titleKorCol = System.Array.IndexOf(headers, "title_Korean");
        int titleEngCol = System.Array.IndexOf(headers, "title_English");
        int descKorCol  = System.Array.IndexOf(headers, "description_Korean");
        int descEngCol  = System.Array.IndexOf(headers, "description_English");

        if (idCol < 0)
        {
            EditorUtility.DisplayDialog("Import CSV", "'achievementId' 열을 찾을 수 없습니다.", "확인");
            return;
        }

        // SO 이름으로 Korean/English 판단
        string soName = achievementTexts.name.ToLower();
        bool isEnglish = soName.Contains("english");

        int targetTitleCol = isEnglish && titleEngCol >= 0 ? titleEngCol :
                             titleKorCol >= 0 ? titleKorCol : titleEngCol;
        int targetDescCol  = isEnglish && descEngCol >= 0 ? descEngCol :
                             descKorCol >= 0 ? descKorCol : descEngCol;

        // 기존 데이터 Dictionary로
        var existing = new System.Collections.Generic.Dictionary<string, AchievementTexts.AchievementLocalizedText>();
        if (achievementTexts.achievements != null)
            foreach (var a in achievementTexts.achievements)
                if (a != null && !string.IsNullOrEmpty(a.achievementId))
                    existing[a.achievementId] = a;

        int updated = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = ParseCSVLine(lines[i]);
            if (cols.Length <= idCol) continue;

            string id    = cols[idCol].Trim();
            string title = targetTitleCol >= 0 && targetTitleCol < cols.Length ? cols[targetTitleCol].Trim() : "";
            string desc  = targetDescCol  >= 0 && targetDescCol  < cols.Length ? cols[targetDescCol].Trim() : "";

            // \n 복원
            desc = desc.Replace("\\n", "\n");

            if (string.IsNullOrEmpty(id)) continue;

            if (existing.ContainsKey(id))
            {
                if (!string.IsNullOrEmpty(title)) existing[id].title = title;
                if (!string.IsNullOrEmpty(desc))  existing[id].description = desc;
                updated++;
            }
            else
            {
                existing[id] = new AchievementTexts.AchievementLocalizedText
                {
                    achievementId = id,
                    title = title,
                    description = desc
                };
                updated++;
            }
        }

        achievementTexts.achievements = existing.Values.OrderBy(a => a.achievementId).ToArray();

        EditorUtility.SetDirty(achievementTexts);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AchievementTextsEditor] ✓ CSV 가져오기 완료: {updated}개 업데이트");
        EditorUtility.DisplayDialog("Import CSV", $"완료! {updated}개 업데이트", "확인");
    }

    // ─────────────────────────────────────────────────────────
    // CSV 한 줄 파싱 (따옴표 처리 포함)
    // ─────────────────────────────────────────────────────────
    private string[] ParseCSVLine(string line)
    {
        var result = new System.Collections.Generic.List<string>();
        bool inQuotes = false;
        var current = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                { current.Append('"'); i++; }
                else inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            { result.Add(current.ToString()); current.Clear(); }
            else current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
#endif