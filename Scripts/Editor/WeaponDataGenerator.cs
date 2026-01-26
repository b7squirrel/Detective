using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class WeaponDataGenerator : EditorWindow
{
    private string weaponBaseName = "Bomb";
    private string displayName = "폭탄";
    private string synergyDispName = "";
    private string savePath = "Assets/Data/Weapons_Items/01_Weapon";
    
    [MenuItem("Tools/Weapon Data Generator")]
    public static void ShowWindow()
    {
        GetWindow<WeaponDataGenerator>("Weapon Data Generator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Weapon Data Auto Generator", EditorStyles.boldLabel);
        
        weaponBaseName = EditorGUILayout.TextField("Weapon Base Name:", weaponBaseName);
        displayName = EditorGUILayout.TextField("Display Name:", displayName);
        synergyDispName = EditorGUILayout.TextField("Synergy Display Name:", synergyDispName);
        
        EditorGUILayout.Space();
        
        savePath = EditorGUILayout.TextField("Save Path:", savePath);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate All Weapon Data (Grade 0-4)"))
        {
            GenerateAllWeaponData();
        }
    }
    
    void GenerateAllWeaponData()
    {
        // 저장 경로 확인
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            Debug.LogError($"Invalid path: {savePath}");
            return;
        }
        
        // 무기 이름으로 폴더 생성
        string weaponFolderPath = Path.Combine(savePath, weaponBaseName);
        if (!AssetDatabase.IsValidFolder(weaponFolderPath))
        {
            // 폴더 생성
            string guid = AssetDatabase.CreateFolder(savePath, weaponBaseName);
            weaponFolderPath = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log($"Created folder: {weaponFolderPath}");
        }
        
        // Grade 0~4까지 생성
        for (int grade = 0; grade < 5; grade++)
        {
            GenerateWeaponDataSet(grade, weaponFolderPath);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Successfully generated weapon data for {weaponBaseName} (Grade 0-4) in {weaponFolderPath}");
    }
    
    void GenerateWeaponDataSet(int grade, string folderPath)
    {
        string gradeName = $"{weaponBaseName} {grade}";
        
        // 1. WeaponData 생성
        WeaponData weaponData = CreateAsset<WeaponData>($"{gradeName}.asset", folderPath);
        weaponData.grade = grade;
        weaponData.Name = weaponBaseName;
        weaponData.DisplayName = displayName;
        weaponData.SynergyDispName = synergyDispName;
        weaponData.SynergyWeapon = weaponBaseName + " Synergy";
        weaponData.upgrades = new List<UpgradeData>();
        EditorUtility.SetDirty(weaponData);
        
        // 2. Acquire UpgradeData 생성
        UpgradeData acquireData = CreateAsset<UpgradeData>($"{gradeName} Acquire.asset", folderPath);
        acquireData.upgradeType = UpgradeType.WeaponGet;
        acquireData.weaponData = weaponData;
        EditorUtility.SetDirty(acquireData);
        
        // 3. Skill UpgradeData 5개 생성 및 upgrades 리스트에 추가
        for (int i = 1; i <= 5; i++)
        {
            UpgradeData skillData = CreateAsset<UpgradeData>($"{gradeName} Skill_{i:D2}.asset", folderPath);
            skillData.upgradeType = UpgradeType.WeaponUpgrade;
            skillData.weaponData = weaponData;
            EditorUtility.SetDirty(skillData);
            
            // upgrades 리스트에 추가
            weaponData.upgrades.Add(skillData);
        }
        
        // 4. Synergy UpgradeData 생성 및 synergyUpgrade에 연결
        UpgradeData synergyData = CreateAsset<UpgradeData>($"{gradeName} Synergy.asset", folderPath);
        synergyData.upgradeType = UpgradeType.SynergyUpgrade;
        synergyData.weaponData = weaponData;
        EditorUtility.SetDirty(synergyData);
        
        // synergyUpgrade에 자동 연결
        weaponData.synergyUpgrade = synergyData;
        
        // WeaponData 최종 변경사항 저장
        EditorUtility.SetDirty(weaponData);
    }
    
    T CreateAsset<T>(string fileName, string folderPath) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        string assetPath = Path.Combine(folderPath, fileName);
        
        AssetDatabase.CreateAsset(asset, assetPath);
        EditorUtility.SetDirty(asset);
        
        return asset;
    }
}