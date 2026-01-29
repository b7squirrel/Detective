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
    
    // 5개 스킬 정보
    [System.Serializable]
    public class SkillInfo
    {
        public string skillName = "";
        public string skillDescription = "";
    }
    
    private SkillInfo[] skills = new SkillInfo[5]
    {
        new SkillInfo(),
        new SkillInfo(),
        new SkillInfo(),
        new SkillInfo(),
        new SkillInfo()
    };
    
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Weapon Data Generator")]
    public static void ShowWindow()
    {
        GetWindow<WeaponDataGenerator>("Weapon Data Generator");
    }
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Weapon Data Auto Generator", EditorStyles.boldLabel);
        
        weaponBaseName = EditorGUILayout.TextField("Weapon Base Name:", weaponBaseName);
        displayName = EditorGUILayout.TextField("Display Name:", displayName);
        synergyDispName = EditorGUILayout.TextField("Synergy Display Name:", synergyDispName);
        
        EditorGUILayout.Space();
        
        // 5개 스킬 입력
        GUILayout.Label("Skills (5개)", EditorStyles.boldLabel);
        for (int i = 0; i < 5; i++)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label($"Skill {i + 1}", EditorStyles.boldLabel);
            skills[i].skillName = EditorGUILayout.TextField("Skill Name:", skills[i].skillName);
            skills[i].skillDescription = EditorGUILayout.TextField("Description:", skills[i].skillDescription);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        EditorGUILayout.Space();
        
        savePath = EditorGUILayout.TextField("Save Path:", savePath);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate All Weapon Data (Grade 0-4)", GUILayout.Height(40)))
        {
            GenerateAllWeaponData();
        }
        
        EditorGUILayout.EndScrollView();
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
        for (int i = 0; i < 5; i++)
        {
            // 스킬 이름이 비어있으면 기본 형식 사용
            string skillFileName = string.IsNullOrEmpty(skills[i].skillName) 
                ? $"Skill_{(i + 1):D2}" 
                : skills[i].skillName;
            
            UpgradeData skillData = CreateAsset<UpgradeData>($"{gradeName} {skillFileName}.asset", folderPath);
            skillData.upgradeType = UpgradeType.WeaponUpgrade;
            skillData.weaponData = weaponData;
            skillData.description = skills[i].skillDescription;
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