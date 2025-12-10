#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(ItemTexts))]
public class ItemTextsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ItemTexts itemTexts = (ItemTexts)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "프로젝트의 모든 Item에서 자동으로 Internal Name을 가져옵니다.", 
            MessageType.Info
        );
        
        if (GUILayout.Button("Auto-Populate from Item Assets", GUILayout.Height(30)))
        {
            AutoPopulateFromItems(itemTexts);
        }
    }
    
    private void AutoPopulateFromItems(ItemTexts itemTexts)
    {
        // 프로젝트에서 모든 Item 찾기
        string[] guids = AssetDatabase.FindAssets("t:Item");
        
        var itemList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<Item>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(item => item != null && !string.IsNullOrEmpty(item.Name))
            .OrderBy(item => item.Name) // 이름 순으로 정렬
            .ToList();
        
        if (itemList.Count == 0)
        {
            Debug.LogWarning("No Item assets found in project!");
            return;
        }
        
        // 기존 itemNames를 Dictionary로 변환 (displayName 유지용)
        var existingNames = new System.Collections.Generic.Dictionary<string, ItemTexts.ItemLocalizedName>();
        if (itemTexts.itemNames != null)
        {
            foreach (var item in itemTexts.itemNames)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemInternalName))
                {
                    existingNames[item.itemInternalName] = item;
                }
            }
        }
        
        itemTexts.itemNames = new ItemTexts.ItemLocalizedName[itemList.Count];
        
        int newCount = 0;
        int keptCount = 0;
        
        for (int i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];
            
            // 기존에 같은 internal name이 있으면 displayName 유지
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
                    displayName = item.Name // Item에 DisplayName이 없다면 Name 사용
                };
                newCount++;
            }
        }
        
        EditorUtility.SetDirty(itemTexts);
        Debug.Log($"✓ Auto-populated {itemList.Count} items (New: {newCount}, Kept: {keptCount})");
    }
}
#endif