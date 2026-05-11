#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ItemSOValidator : EditorWindow
{
    [MenuItem("Tools/Localization/Validate & Sync Item SOs")]
    public static void ShowWindow()
    {
        GetWindow<ItemSOValidator>("Item SO Validator");
    }

    private string csvPath       = "Assets/Localization/CSV/items.csv";
    private string itemSOFolder  = "Assets/Resources/03_Equipment";
    private Vector2 scrollPos;
    private string reportText    = "";
    private bool   hasRun        = false;

    void OnGUI()
    {
        GUILayout.Label("Item SO Validator & Syncer", EditorStyles.boldLabel);
        EditorGUILayout.Space(8);

        csvPath      = EditorGUILayout.TextField("CSV 경로",       csvPath);
        itemSOFolder = EditorGUILayout.TextField("Item SO 폴더",   itemSOFolder);

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "CSV → Item SO 대응 관계:\n" +
            "itemInternalName  →  Item.Name       (매칭 키)\n" +
            "korean_name       →  Item.DisplayName\n" +
            "korean_set_name   →  Item.setDisplayName\n" +
            "korean_part_name  →  Item.itemDisplayName",
            MessageType.Info);

        EditorGUILayout.Space(8);

        // ── 검사만 (수정 없음) ────────────────────────────
        if (GUILayout.Button("① 검사만 (수정 없음)", GUILayout.Height(32)))
        {
            Run(applyFix: false);
        }

        EditorGUILayout.Space(4);

        // ── 검사 + 수정 ──────────────────────────────────
        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
        if (GUILayout.Button("② 검사 + CSV 기준으로 수정", GUILayout.Height(32)))
        {
            if (EditorUtility.DisplayDialog("확인",
                "불일치 항목을 CSV 기준으로 모두 덮어씁니다.\n계속하시겠습니까?",
                "수정", "취소"))
            {
                Run(applyFix: true);
            }
        }
        GUI.backgroundColor = Color.white;

        // ── 결과 로그 ────────────────────────────────────
        if (hasRun)
        {
            EditorGUILayout.Space(8);
            GUILayout.Label("결과", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos,
                GUILayout.Height(position.height - 260));
            EditorGUILayout.TextArea(reportText,
                GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    // ── 핵심 로직 ─────────────────────────────────────────
    void Run(bool applyFix)
    {
        hasRun = true;
        var sb = new StringBuilder();

        // 1. CSV 로드
        if (!File.Exists(csvPath))
        {
            reportText = $"✗ CSV 파일 없음: {csvPath}";
            return;
        }

        var csvRows = LoadCSV(csvPath, sb);
        if (csvRows == null) { reportText = sb.ToString(); return; }
        sb.AppendLine($"CSV 로드 완료: {csvRows.Count}개 항목\n");

        // 2. Item SO 로드
        string[] guids = AssetDatabase.FindAssets("t:Item",
            new[] { itemSOFolder });

        if (guids.Length == 0)
        {
            sb.AppendLine($"✗ '{itemSOFolder}'에서 Item SO를 찾을 수 없습니다.");
            reportText = sb.ToString();
            return;
        }
        sb.AppendLine($"Item SO 발견: {guids.Length}개\n");

        int matchCount    = 0;
        int mismatchCount = 0;
        int fixedCount    = 0;
        int noCSVRow      = 0;

        foreach (string guid in guids.OrderBy(g =>
            AssetDatabase.GUIDToAssetPath(g)))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Item   item = AssetDatabase.LoadAssetAtPath<Item>(path);
            if (item == null) continue;

            // CSV에서 매칭 행 탐색
            if (!csvRows.TryGetValue(item.Name, out CSVRow row))
            {
                sb.AppendLine($"[미매칭] {item.Name}  ← CSV에 없음");
                noCSVRow++;
                continue;
            }

            // 각 필드 비교
            bool dirty = false;
            var  diff  = new StringBuilder();

            dirty |= CheckAndFix(item, "DisplayName",
                item.DisplayName,     row.koreanName,
                v => item.DisplayName = v,
                applyFix, diff);

            dirty |= CheckAndFix(item, "setDisplayName",
                item.setDisplayName,  row.koreanSetName,
                v => item.setDisplayName = v,
                applyFix, diff);

            dirty |= CheckAndFix(item, "itemDisplayName",
                item.itemDisplayName, row.koreanPartName,
                v => item.itemDisplayName = v,
                applyFix, diff);

            if (dirty)
            {
                mismatchCount++;
                string tag = applyFix ? "[수정됨]" : "[불일치]";
                sb.AppendLine($"{tag} {item.Name}");
                sb.Append(diff);

                if (applyFix)
                {
                    EditorUtility.SetDirty(item);
                    fixedCount++;
                }
            }
            else
            {
                matchCount++;
            }
        }

        if (applyFix && fixedCount > 0)
            AssetDatabase.SaveAssets();

        // 요약
        sb.AppendLine();
        sb.AppendLine("══ 요약 ══════════════════════════");
        sb.AppendLine($"일치:    {matchCount}개");
        sb.AppendLine($"불일치:  {mismatchCount}개");
        sb.AppendLine($"CSV 없음: {noCSVRow}개");
        if (applyFix)
            sb.AppendLine($"수정 완료: {fixedCount}개");

        reportText = sb.ToString();
        Repaint();
    }

    // ── 필드 비교 및 수정 ─────────────────────────────────
    bool CheckAndFix(Item item, string fieldName,
                     string current, string csvValue,
                     System.Action<string> setter,
                     bool applyFix, StringBuilder diff)
    {
        // 빈 CSV 값은 무시 (덮어쓰지 않음)
        if (string.IsNullOrEmpty(csvValue)) return false;
        if (current == csvValue)            return false;

        diff.AppendLine($"  {fieldName}:");
        diff.AppendLine($"    SO  : \"{current}\"");
        diff.AppendLine($"    CSV : \"{csvValue}\"");

        if (applyFix) setter(csvValue);
        return true;
    }

    // ── CSV 파싱 ──────────────────────────────────────────
    class CSVRow
    {
        public string koreanName;
        public string koreanSetName;
        public string koreanPartName;
    }

    Dictionary<string, CSVRow> LoadCSV(string path, StringBuilder log)
    {
        var dict  = new Dictionary<string, CSVRow>();
        var lines = File.ReadAllLines(path);

        if (lines.Length < 2)
        {
            log.AppendLine("✗ CSV 헤더만 있거나 비어 있습니다.");
            return null;
        }

        // 헤더에서 컬럼 인덱스 탐색
        string[] header = ParseCSVLine(lines[0]);
        int idxInternal  = FindCol(header, "itemInternalName");
        int idxKorName   = FindCol(header, "korean_name");
        int idxKorSet    = FindCol(header, "korean_set_name");
        int idxKorPart   = FindCol(header, "korean_part_name");

        if (idxInternal < 0)
        {
            log.AppendLine("✗ CSV에 'itemInternalName' 컬럼이 없습니다.");
            return null;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] v = ParseCSVLine(lines[i]);

            string key = Get(v, idxInternal).Trim();
            if (string.IsNullOrEmpty(key)) continue;

            dict[key] = new CSVRow
            {
                koreanName     = Get(v, idxKorName).Trim(),
                koreanSetName  = Get(v, idxKorSet).Trim(),
                koreanPartName = Get(v, idxKorPart).Trim(),
            };
        }

        return dict;
    }

    static int FindCol(string[] header, string name)
    {
        for (int i = 0; i < header.Length; i++)
            if (header[i].Trim().Equals(name,
                System.StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    static string Get(string[] arr, int idx) =>
        idx >= 0 && idx < arr.Length ? arr[idx] : "";

    static string[] ParseCSVLine(string line)
    {
        var    values   = new List<string>();
        bool   inQuotes = false;
        string current  = "";

        foreach (char c in line)
        {
            if      (c == '"')             inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes) { values.Add(current); current = ""; }
            else                            current += c;
        }
        values.Add(current);
        return values.ToArray();
    }
}
#endif