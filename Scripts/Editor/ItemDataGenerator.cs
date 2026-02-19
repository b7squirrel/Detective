using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ItemDataGenerator : EditorWindow
{
    private string itemBaseName = "00cowboy_rifle";
    private string itemName = "CowboyRifle";
    private string displayName = "카우보이 라이플";
    private string savePath = "Assets/Data/Weapons_Items/04_Equipment";

    private Vector2 scrollPosition;

    [MenuItem("Tools/Item Data Generator")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataGenerator>("Item Data Generator");
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("Item Data Auto Generator", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        itemBaseName = EditorGUILayout.TextField("File Base Name:", itemBaseName);

        EditorGUILayout.HelpBox(
            "파일명 예시: cowboy_rifle 0 ~ cowboy_rifle 4",
            MessageType.Info
        );

        EditorGUILayout.Space();

        itemName     = EditorGUILayout.TextField("Name:", itemName);
        displayName  = EditorGUILayout.TextField("Display Name:", displayName);

        EditorGUILayout.Space();

        savePath = EditorGUILayout.TextField("Save Path:", savePath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate All Item Data (Grade 0-4)", GUILayout.Height(40)))
        {
            GenerateAllItemData();
        }

        EditorGUILayout.EndScrollView();
    }

    void GenerateAllItemData()
    {
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            Debug.LogError($"Invalid save path: {savePath}");
            return;
        }

        for (int grade = 0; grade < 5; grade++)
        {
            GenerateItemData(grade, savePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Successfully generated Item data for '{itemBaseName}' (Grade 0-4) in {savePath}");
        EditorUtility.DisplayDialog(
            "완료!",
            $"'{itemBaseName}' 아이템 데이터 5개 (Grade 0~4) 생성 완료!\n경로: {savePath}",
            "OK"
        );
    }

    void GenerateItemData(int grade, string folderPath)
    {
        string fileName = $"{itemBaseName} {grade}.asset";
        string assetPath = Path.Combine(folderPath, fileName);

        // 이미 존재하면 덮어쓰지 않고 경고
        if (File.Exists(assetPath))
        {
            Debug.LogWarning($"Already exists, skipping: {assetPath}");
            return;
        }

        Item item = ScriptableObject.CreateInstance<Item>();
        AssetDatabase.CreateAsset(item, assetPath);

        // 필드 설정
        item.Name        = itemName;
        item.DisplayName = displayName;
        item.grade       = grade;
        item.upgrades    = new List<UpgradeData>();
        item.SynergyWeapons = new List<string>();

        EditorUtility.SetDirty(item);
        Debug.Log($"Created: {assetPath}");
    }
}