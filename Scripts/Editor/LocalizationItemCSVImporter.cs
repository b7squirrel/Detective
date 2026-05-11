#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ItemCSVImporter : EditorWindow
{
    [MenuItem("Tools/Localization/Import Items CSV")]
    public static void ShowWindow()
    {
        GetWindow<ItemCSVImporter>("Items CSV Importer");
    }

    private string itemsCSVPath = "Assets/Localization/CSV/items.csv";
    private ItemTexts koreanItemTexts;
    private ItemTexts englishItemTexts;

    void OnGUI()
    {
        GUILayout.Label("Items CSV Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        GUILayout.Label("CSV File", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        itemsCSVPath = EditorGUILayout.TextField("Items CSV Path", itemsCSVPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Select Items CSV",
                "Assets/Localization/CSV", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                    itemsCSVPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        GUILayout.Label("Target ScriptableObjects", EditorStyles.boldLabel);
        koreanItemTexts = (ItemTexts)EditorGUILayout.ObjectField(
            "Korean ItemTexts", koreanItemTexts, typeof(ItemTexts), false);
        englishItemTexts = (ItemTexts)EditorGUILayout.ObjectField(
            "English ItemTexts", englishItemTexts, typeof(ItemTexts), false);

        EditorGUILayout.Space(10);
        if (File.Exists(itemsCSVPath))
            EditorGUILayout.HelpBox($"✓ CSV 파일 찾음: {itemsCSVPath}", MessageType.Info);
        else
            EditorGUILayout.HelpBox($"✗ CSV 파일 없음: {itemsCSVPath}", MessageType.Warning);

        EditorGUILayout.Space(10);

        // ── Import ────────────────────────────────────────
        GUI.enabled = File.Exists(itemsCSVPath)
                      && koreanItemTexts != null
                      && englishItemTexts != null;
        if (GUILayout.Button("Import from CSV", GUILayout.Height(40)))
            ImportCSV();
        GUI.enabled = true;

        EditorGUILayout.Space(10);

        // ── Export ────────────────────────────────────────
        GUI.enabled = koreanItemTexts != null && englishItemTexts != null;
        if (GUILayout.Button("Export to CSV", GUILayout.Height(30)))
            ExportToCSV();
        GUI.enabled = true;

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "CSV 컬럼 순서:\n" +
            "itemInternalName | korean_name | english_name |\n" +
            "korean_set_name | english_set_name |\n" +
            "korean_part_name | english_part_name\n\n" +
            "사용 방법:\n" +
            "1. Export로 현재 데이터를 CSV로 내보내기\n" +
            "2. 구글 시트에서 편집\n" +
            "3. CSV 다운로드 후 Import",
            MessageType.Info);
    }

    void ImportCSV()
    {
        if (!File.Exists(itemsCSVPath))
        {
            EditorUtility.DisplayDialog("Error",
                $"CSV 파일을 찾을 수 없습니다:\n{itemsCSVPath}", "OK");
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(itemsCSVPath);
            if (lines.Length < 2)
            {
                EditorUtility.DisplayDialog("Error",
                    "CSV 파일이 비어있거나 헤더만 있습니다.", "OK");
                return;
            }

            var dataLines = lines.Skip(1)
                .Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

            var koreanItems   = new List<ItemTexts.ItemLocalizedName>();
            var englishItems  = new List<ItemTexts.ItemLocalizedName>();
            var koreanSets    = new List<ItemTexts.ItemLocalizedName>(); // ★
            var englishSets   = new List<ItemTexts.ItemLocalizedName>(); // ★
            var koreanParts   = new List<ItemTexts.ItemLocalizedName>(); // ★
            var englishParts  = new List<ItemTexts.ItemLocalizedName>(); // ★

            int lineNumber = 2;
            foreach (string line in dataLines)
            {
                string[] v = ParseCSVLine(line);
                if (v.Length < 3)
                {
                    Debug.LogWarning($"Line {lineNumber}: 열 부족. 건너뜁니다.");
                    lineNumber++;
                    continue;
                }

                string internalName = v[0].Trim();
                if (string.IsNullOrEmpty(internalName))
                {
                    lineNumber++;
                    continue;
                }

                // itemNames (기존)
                koreanItems.Add(new ItemTexts.ItemLocalizedName
                    { itemInternalName = internalName, displayName = v[1].Trim() });
                englishItems.Add(new ItemTexts.ItemLocalizedName
                    { itemInternalName = internalName, displayName = v[2].Trim() });

                // setDisplayNames (★ 컬럼 3, 4)
                koreanSets.Add(new ItemTexts.ItemLocalizedName
                {
                    itemInternalName = internalName,
                    displayName      = v.Length > 3 ? v[3].Trim() : ""
                });
                englishSets.Add(new ItemTexts.ItemLocalizedName
                {
                    itemInternalName = internalName,
                    displayName      = v.Length > 4 ? v[4].Trim() : ""
                });

                // itemPartNames (★ 컬럼 5, 6)
                koreanParts.Add(new ItemTexts.ItemLocalizedName
                {
                    itemInternalName = internalName,
                    displayName      = v.Length > 5 ? v[5].Trim() : ""
                });
                englishParts.Add(new ItemTexts.ItemLocalizedName
                {
                    itemInternalName = internalName,
                    displayName      = v.Length > 6 ? v[6].Trim() : ""
                });

                lineNumber++;
            }

            // 저장
            koreanItemTexts.itemNames      = koreanItems.ToArray();
            englishItemTexts.itemNames     = englishItems.ToArray();
            koreanItemTexts.setDisplayNames  = koreanSets.ToArray();   // ★
            englishItemTexts.setDisplayNames = englishSets.ToArray();  // ★
            koreanItemTexts.itemPartNames    = koreanParts.ToArray();  // ★
            englishItemTexts.itemPartNames   = englishParts.ToArray(); // ★

            EditorUtility.SetDirty(koreanItemTexts);
            EditorUtility.SetDirty(englishItemTexts);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Import 완료!",
                $"{koreanItems.Count}개 아이템 임포트 완료", "OK");
            Debug.Log($"✓ CSV Import 완료: {koreanItems.Count}개");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Import 오류:\n{e.Message}", "OK");
            Debug.LogError($"CSV Import 오류: {e}");
        }
    }

    void ExportToCSV()
    {
        try
        {
            var lines = new List<string>();

            // ★ 헤더에 컬럼 추가
            lines.Add("itemInternalName,korean_name,english_name," +
                      "korean_set_name,english_set_name," +
                      "korean_part_name,english_part_name");

            // Korean 기준으로 인덱스 순회
            int count = koreanItemTexts.itemNames?.Length ?? 0;
            var engDict     = ToDict(englishItemTexts.itemNames);
            var korSetDict  = ToDict(koreanItemTexts.setDisplayNames);
            var engSetDict  = ToDict(englishItemTexts.setDisplayNames);
            var korPartDict = ToDict(koreanItemTexts.itemPartNames);
            var engPartDict = ToDict(englishItemTexts.itemPartNames);

            for (int i = 0; i < count; i++)
            {
                var kor = koreanItemTexts.itemNames[i];
                if (kor == null) continue;

                string key = kor.itemInternalName;
                string line =
                    $"{EscapeCSV(key)}," +
                    $"{EscapeCSV(kor.displayName)}," +
                    $"{EscapeCSV(engDict.TryGetValue(key, out var e) ? e.displayName : "")}," +
                    // ★ set / part 컬럼
                    $"{EscapeCSV(korSetDict.TryGetValue(key,  out var ks) ? ks.displayName : "")}," +
                    $"{EscapeCSV(engSetDict.TryGetValue(key,  out var es) ? es.displayName : "")}," +
                    $"{EscapeCSV(korPartDict.TryGetValue(key, out var kp) ? kp.displayName : "")}," +
                    $"{EscapeCSV(engPartDict.TryGetValue(key, out var ep) ? ep.displayName : "")}";

                lines.Add(line);
            }

            string directory = Path.GetDirectoryName(itemsCSVPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllLines(itemsCSVPath, lines);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Export 완료!",
                $"{count}개 아이템 Export 완료\n저장 위치: {itemsCSVPath}", "OK");
            Debug.Log($"✓ CSV Export 완료: {itemsCSVPath}");
            EditorUtility.RevealInFinder(itemsCSVPath);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Export 오류:\n{e.Message}", "OK");
            Debug.LogError($"CSV Export 오류: {e}");
        }
    }

    // ── 유틸 ────────────────────────────────────────────
    static Dictionary<string, ItemTexts.ItemLocalizedName> ToDict(
        ItemTexts.ItemLocalizedName[] arr)
    {
        var dict = new Dictionary<string, ItemTexts.ItemLocalizedName>();
        if (arr == null) return dict;
        foreach (var e in arr)
            if (e != null && !string.IsNullOrEmpty(e.itemInternalName))
                dict[e.itemInternalName] = e;
        return dict;
    }

    string[] ParseCSVLine(string line)
    {
        var values = new List<string>();
        bool inQuotes = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == '"')          inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes) { values.Add(current); current = ""; }
            else                   current += c;
        }
        values.Add(current);
        return values.ToArray();
    }

    string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }
        return value;
    }
}
#endif