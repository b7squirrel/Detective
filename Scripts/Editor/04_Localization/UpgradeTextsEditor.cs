#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;

[CustomEditor(typeof(UpgradeTexts))]
public class UpgradeTextsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UpgradeTexts upgradeTexts = (UpgradeTexts)target;

        EditorGUILayout.Space(10);

        // ─── 오리 업그레이드 ───
        EditorGUILayout.LabelField("오리 업그레이드", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Assets/Resources/Weapons/Friends 폴더의 UpgradeData를 가져옵니다.", MessageType.Info);
        if (GUILayout.Button("Auto-Populate Weapon Upgrades", GUILayout.Height(28)))
            AutoPopulateWeapons(upgradeTexts);

        EditorGUILayout.Space(5);

        // ─── 아이템 업그레이드 ───
        EditorGUILayout.LabelField("아이템 업그레이드", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Assets/Resources/Items 폴더의 UpgradeData를 가져옵니다.", MessageType.Info);
        if (GUILayout.Button("Auto-Populate Item Upgrades", GUILayout.Height(28)))
            AutoPopulateItems(upgradeTexts);

        EditorGUILayout.Space(10);

        // ─── CSV ───
        EditorGUILayout.LabelField("CSV", EditorStyles.boldLabel);
        if (GUILayout.Button("Export CSV", GUILayout.Height(28)))
            ExportCSV(upgradeTexts);
        if (GUILayout.Button("Import CSV", GUILayout.Height(28)))
            ImportCSV(upgradeTexts);
    }

    // ─────────────────────────────────────────────────────────
    // Auto-Populate Weapons
    // ─────────────────────────────────────────────────────────
    private void AutoPopulateWeapons(UpgradeTexts upgradeTexts)
    {
        var list = LoadUpgradeData(
        "Assets/Resources/Weapons/Friends",
        "Assets/Data/Weapons_Items/01_Weapon"
    );
        if (list == null) return;

        var existing = ToDict(upgradeTexts.weaponUpgradeTexts);
        upgradeTexts.weaponUpgradeTexts = BuildArray(list, existing, out int newCount, out int keptCount);

        EditorUtility.SetDirty(upgradeTexts);
        AssetDatabase.SaveAssets();
        Debug.Log($"[UpgradeTextsEditor] ✓ 오리 업그레이드 Auto-Populate 완료: 총 {list.Count}개 (신규: {newCount}, 유지: {keptCount})");
    }

    // ─────────────────────────────────────────────────────────
    // Auto-Populate Items
    // ─────────────────────────────────────────────────────────
    private void AutoPopulateItems(UpgradeTexts upgradeTexts)
    {
        var list = LoadUpgradeData("Assets/Resources/Items");
        if (list == null) return;

        var existing = ToDict(upgradeTexts.itemUpgradeTexts);
        upgradeTexts.itemUpgradeTexts = BuildArray(list, existing, out int newCount, out int keptCount);

        EditorUtility.SetDirty(upgradeTexts);
        AssetDatabase.SaveAssets();
        Debug.Log($"[UpgradeTextsEditor] ✓ 아이템 업그레이드 Auto-Populate 완료: 총 {list.Count}개 (신규: {newCount}, 유지: {keptCount})");
    }

    // ─────────────────────────────────────────────────────────
    // 공통 헬퍼
    // ─────────────────────────────────────────────────────────
    private System.Collections.Generic.List<UpgradeData> LoadUpgradeData(params string[] folders)
    {
        string[] guids = AssetDatabase.FindAssets("t:UpgradeData", folders);
        var list = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<UpgradeData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(ud => ud != null)
            .OrderBy(ud => ud.name)
            .ToList();

        if (list.Count == 0)
        {
            Debug.LogWarning($"[UpgradeTextsEditor] UpgradeData를 찾을 수 없습니다: {string.Join(", ", folders)}");
            return null;
        }
        return list;
    }

    private System.Collections.Generic.Dictionary<string, string> ToDict(UpgradeTexts.UpgradeLocalizedText[] arr)
    {
        var dict = new System.Collections.Generic.Dictionary<string, string>();
        if (arr == null) return dict;
        foreach (var t in arr)
            if (t != null && !string.IsNullOrEmpty(t.upgradeName))
                dict[t.upgradeName] = t.description;
        return dict;
    }

    private UpgradeTexts.UpgradeLocalizedText[] BuildArray(
        System.Collections.Generic.List<UpgradeData> list,
        System.Collections.Generic.Dictionary<string, string> existing,
        out int newCount, out int keptCount)
    {
        newCount = 0; keptCount = 0;
        var result = new UpgradeTexts.UpgradeLocalizedText[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            var ud = list[i];
            var entry = new UpgradeTexts.UpgradeLocalizedText { upgradeName = ud.name };
            if (existing.ContainsKey(ud.name)) { entry.description = existing[ud.name]; keptCount++; }
            else { entry.description = ud.description ?? ""; newCount++; }
            result[i] = entry;
        }
        return result;
    }

    // ─────────────────────────────────────────────────────────
    // Export CSV
    // ─────────────────────────────────────────────────────────
    private void ExportCSV(UpgradeTexts upgradeTexts)
    {
        string path = EditorUtility.SaveFilePanel("Export Upgrade Texts CSV", Application.dataPath, "UpgradeTexts", "csv");
        if (string.IsNullOrEmpty(path)) return;

        var sb = new StringBuilder();
        sb.AppendLine("upgradeName,Korean,English");

        void AppendSection(UpgradeTexts.UpgradeLocalizedText[] arr, string sectionLabel)
        {
            if (arr == null || arr.Length == 0) return;
            sb.AppendLine($"# {sectionLabel}");
            foreach (var t in arr)
            {
                if (t == null) continue;
                string desc = (t.description ?? "").Replace("\"", "\"\"");
                sb.AppendLine($"{t.upgradeName},\"{desc}\",");
            }
        }

        AppendSection(upgradeTexts.weaponUpgradeTexts, "오리 업그레이드");
        AppendSection(upgradeTexts.itemUpgradeTexts, "아이템 업그레이드");

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[UpgradeTextsEditor] ✓ CSV 내보내기 완료: {path}");
        EditorUtility.DisplayDialog("Export CSV", $"완료!\n{path}", "확인");
    }

    // ─────────────────────────────────────────────────────────
    // Import CSV
    // ─────────────────────────────────────────────────────────
    private void ImportCSV(UpgradeTexts upgradeTexts)
    {
        string path = EditorUtility.OpenFilePanel("Import Upgrade Texts CSV", Application.dataPath, "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length < 2) { EditorUtility.DisplayDialog("Import CSV", "데이터가 없습니다.", "확인"); return; }

        string[] headers = ParseCSVLine(lines[0]);
        int nameCol = System.Array.IndexOf(headers, "upgradeName");
        int korCol  = System.Array.IndexOf(headers, "Korean");
        int engCol  = System.Array.IndexOf(headers, "English");

        if (nameCol < 0) { EditorUtility.DisplayDialog("Import CSV", "'upgradeName' 열을 찾을 수 없습니다.", "확인"); return; }

        string soName = upgradeTexts.name.ToLower();
        int targetCol = soName.Contains("english") && engCol >= 0 ? engCol :
                        korCol >= 0 ? korCol : engCol;

        if (targetCol < 0) { EditorUtility.DisplayDialog("Import CSV", "Korean 또는 English 열을 찾을 수 없습니다.", "확인"); return; }

        // 기존 데이터를 Dictionary로
        var weaponDict = ToDict(upgradeTexts.weaponUpgradeTexts);
        var itemDict   = ToDict(upgradeTexts.itemUpgradeTexts);

        int updated = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]) || lines[i].StartsWith("#")) continue;
            string[] cols = ParseCSVLine(lines[i]);
            if (cols.Length <= nameCol) continue;

            string name = cols[nameCol].Trim();
            string desc = targetCol < cols.Length ? cols[targetCol].Trim() : "";
            if (string.IsNullOrEmpty(name)) continue;

            if (weaponDict.ContainsKey(name)) { weaponDict[name] = desc; updated++; }
            else if (itemDict.ContainsKey(name)) { itemDict[name] = desc; updated++; }
        }

        // 다시 배열로 변환
        upgradeTexts.weaponUpgradeTexts = DictToArray(weaponDict, upgradeTexts.weaponUpgradeTexts);
        upgradeTexts.itemUpgradeTexts   = DictToArray(itemDict,   upgradeTexts.itemUpgradeTexts);

        EditorUtility.SetDirty(upgradeTexts);
        AssetDatabase.SaveAssets();
        Debug.Log($"[UpgradeTextsEditor] ✓ CSV 가져오기 완료: {updated}개 업데이트");
        EditorUtility.DisplayDialog("Import CSV", $"완료! {updated}개 업데이트", "확인");
    }

    private UpgradeTexts.UpgradeLocalizedText[] DictToArray(
        System.Collections.Generic.Dictionary<string, string> dict,
        UpgradeTexts.UpgradeLocalizedText[] original)
    {
        if (original == null) return new UpgradeTexts.UpgradeLocalizedText[0];
        var result = new UpgradeTexts.UpgradeLocalizedText[original.Length];
        for (int i = 0; i < original.Length; i++)
        {
            result[i] = new UpgradeTexts.UpgradeLocalizedText
            {
                upgradeName = original[i].upgradeName,
                description = dict.ContainsKey(original[i].upgradeName) ? dict[original[i].upgradeName] : original[i].description
            };
        }
        return result;
    }

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
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                else inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes) { result.Add(current.ToString()); current.Clear(); }
            else current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
#endif