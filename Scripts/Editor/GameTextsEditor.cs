// Editor/GameTextsEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(GameTexts))]
public class GameTextsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GameTexts gameTexts = (GameTexts)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "프로젝트의 모든 WeaponData에서 자동으로 Internal Name을 가져옵니다.", 
            MessageType.Info
        );
        
        if (GUILayout.Button("Auto-Populate from WeaponData Assets", GUILayout.Height(30)))
        {
            AutoPopulateFromWeaponData(gameTexts);
        }
    }
    
    private void AutoPopulateFromWeaponData(GameTexts gameTexts)
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
        
        gameTexts.weaponNames = new GameTexts.WeaponLocalizedName[weaponDataList.Count];
        
        for (int i = 0; i < weaponDataList.Count; i++)
        {
            var weaponData = weaponDataList[i];
            
            // 기존에 같은 internal name이 있으면 displayName 유지
            var existing = gameTexts.weaponNames?.FirstOrDefault(
                w => w != null && w.weaponInternalName == weaponData.Name);
            
            gameTexts.weaponNames[i] = new GameTexts.WeaponLocalizedName
            {
                weaponInternalName = weaponData.Name,
                displayName = existing?.displayName ?? weaponData.DisplayName ?? "",
                synergyDisplayName = existing?.synergyDisplayName ?? weaponData.SynergyDispName ?? ""
            };
        }
        
        EditorUtility.SetDirty(gameTexts);
        Debug.Log($"Auto-populated {weaponDataList.Count} weapons from WeaponData assets");
    }
}
#endif