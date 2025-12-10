#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(CharTexts))]
public class CharTextsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        CharTexts charTexts = (CharTexts)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "프로젝트의 모든 WeaponData에서 자동으로 Internal Name을 가져옵니다.", 
            MessageType.Info
        );
        
        if (GUILayout.Button("Auto-Populate from WeaponData Assets", GUILayout.Height(30)))
        {
            AutoPopulateFromWeaponData(charTexts);
        }
    }
    
    private void AutoPopulateFromWeaponData(CharTexts charTexts)
    {
        // 프로젝트에서 모든 WeaponData 찾기
        string[] guids = AssetDatabase.FindAssets("t:WeaponData");
        
        var weaponDataList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<WeaponData>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(wd => wd != null && !string.IsNullOrEmpty(wd.Name))
            .OrderBy(wd => wd.Name) // 이름 순으로 정렬
            .ToList();
        
        if (weaponDataList.Count == 0)
        {
            Debug.LogWarning("No WeaponData assets found in project!");
            return;
        }
        
        // 기존 weaponNames를 Dictionary로 변환 (displayName 유지용)
        var existingNames = new System.Collections.Generic.Dictionary<string, CharTexts.WeaponLocalizedName>();
        if (charTexts.weaponNames != null)
        {
            foreach (var weapon in charTexts.weaponNames)
            {
                if (weapon != null && !string.IsNullOrEmpty(weapon.weaponInternalName))
                {
                    existingNames[weapon.weaponInternalName] = weapon;
                }
            }
        }
        
        charTexts.weaponNames = new CharTexts.WeaponLocalizedName[weaponDataList.Count];
        
        int newCount = 0;
        int keptCount = 0;
        
        for (int i = 0; i < weaponDataList.Count; i++)
        {
            var weaponData = weaponDataList[i];
            
            // 기존에 같은 internal name이 있으면 displayName 유지
            if (existingNames.ContainsKey(weaponData.Name))
            {
                charTexts.weaponNames[i] = existingNames[weaponData.Name];
                keptCount++;
            }
            else
            {
                charTexts.weaponNames[i] = new CharTexts.WeaponLocalizedName
                {
                    weaponInternalName = weaponData.Name,
                    displayName = weaponData.DisplayName ?? "",
                    synergyDisplayName = weaponData.SynergyDispName ?? ""
                };
                newCount++;
            }
        }
        
        EditorUtility.SetDirty(charTexts);
        Debug.Log($"✓ Auto-populated {weaponDataList.Count} weapons (New: {newCount}, Kept: {keptCount})");
    }
}
#endif