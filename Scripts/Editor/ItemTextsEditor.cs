#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(ItemTexts))]
public class ItemTextsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ItemTexts itemTexts = (ItemTexts)target;

        // ── 기존 버튼 (완전히 원본 유지) ─────────────────
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "프로젝트의 모든 Item에서 자동으로 Internal Name을 가져옵니다.",
            MessageType.Info);

        if (GUILayout.Button("Auto-Populate from Item Assets", GUILayout.Height(30)))
            AutoPopulateFromItems(itemTexts);

        // ── 새 버튼 ───────────────────────────────────────
        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox(
            "Item SO의 setDisplayName / itemDisplayName을\n" +
            "SetDisplayNames와 ItemPartNames에 채웁니다.\n" +
            "이미 입력된 항목은 유지됩니다.",
            MessageType.Info);

        if (GUILayout.Button("Auto-Populate Set & Part Names from Item SO",
            GUILayout.Height(30)))
            AutoPopulateSetAndPartNames(itemTexts);
    }

    // ── 원본 그대로 ──────────────────────────────────────
    void AutoPopulateFromItems(ItemTexts itemTexts)
    {
        string[] guids = AssetDatabase.FindAssets("t:Item",
    new[] { "Assets/Resources/03_Equipment" });
        var itemList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<Item>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(item => item != null && !string.IsNullOrEmpty(item.Name))
            .OrderBy(item => item.Name)
            .ToList();

        if (itemList.Count == 0)
        {
            Debug.LogWarning("No Item assets found in project!");
            return;
        }

        var existingNames = new Dictionary<string, ItemTexts.ItemLocalizedName>();
        if (itemTexts.itemNames != null)
        {
            foreach (var item in itemTexts.itemNames)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemInternalName))
                    existingNames[item.itemInternalName] = item;
            }
        }

        itemTexts.itemNames = new ItemTexts.ItemLocalizedName[itemList.Count];
        int newCount = 0, keptCount = 0;

        for (int i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];
            if (existingNames.ContainsKey(item.Name))
            {
                itemTexts.itemNames[i] = existingNames[item.Name];
                keptCount++;
            }
            else
            {
                itemTexts.itemNames[i] = new ItemTexts.ItemLocalizedName
                {
                    itemInternalName = item.Name,
                    displayName = item.Name
                };
                newCount++;
            }
        }

        EditorUtility.SetDirty(itemTexts);
        Debug.Log($"✓ Auto-populated {itemList.Count} items " +
                  $"(New: {newCount}, Kept: {keptCount})");
    }

    // ── 신규: 원본과 동일한 로딩 + HashSet으로 중복 제거 ─
    void AutoPopulateSetAndPartNames(ItemTexts itemTexts)
    {
        string[] guids = AssetDatabase.FindAssets("t:Item",
    new[] { "Assets/Resources/03_Equipment" });
        var itemList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<Item>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(item => item != null && !string.IsNullOrEmpty(item.Name))
            .OrderBy(item => item.Name)
            .ToList();

        if (itemList.Count == 0)
        {
            Debug.LogWarning("No Item assets found in project!");
            return;
        }

        var existingSet = ToDict(itemTexts.setDisplayNames);
        var existingPart = ToDict(itemTexts.itemPartNames);

        var setList = new List<ItemTexts.ItemLocalizedName>();
        var partList = new List<ItemTexts.ItemLocalizedName>();
        // ★ 이미 처리한 Name은 건너뜀 → 원본과 동일한 개수 보장
        var seen = new HashSet<string>();

        foreach (var item in itemList)
        {
            if (seen.Contains(item.Name)) continue;
            seen.Add(item.Name);

            // setDisplayNames
            bool hasSetValue = existingSet.ContainsKey(name)
                   && !string.IsNullOrEmpty(existingSet[name].displayName); // ★ 비어있으면 false

            setList.Add(existingSet.ContainsKey(item.Name)
                ? existingSet[item.Name]
                : new ItemTexts.ItemLocalizedName
                {
                    itemInternalName = item.Name,
                    displayName = item.setDisplayName ?? ""
                });

            // itemPartNames
            bool hasPartValue = existingPart.ContainsKey(name)
                    && !string.IsNullOrEmpty(existingPart[name].displayName); // ★ 비어있으면 false
                    
            partList.Add(existingPart.ContainsKey(item.Name)
                ? existingPart[item.Name]
                : new ItemTexts.ItemLocalizedName
                {
                    itemInternalName = item.Name,
                    displayName = item.itemDisplayName ?? ""
                });
        }

        itemTexts.setDisplayNames = setList.ToArray();
        itemTexts.itemPartNames = partList.ToArray();

        EditorUtility.SetDirty(itemTexts);
        Debug.Log($"✓ Set & Part Names 완료: {setList.Count}개");
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
}
#endif