using UnityEditor;
using UnityEngine;
using System.IO;

public class AchievementCreator : EditorWindow
{
    private string description = "업적 설명을 입력하세요";
    private int rewardGem = 10;
    private string fileName = "new_achievement";

    [MenuItem("Tools/Achievement Creator")]
    public static void Open()
    {
        GetWindow<AchievementCreator>("Achievement Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create New Achievement", EditorStyles.boldLabel);

        description = EditorGUILayout.TextField("Description", description);
        rewardGem = EditorGUILayout.IntField("Reward Gem", rewardGem);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        GUILayout.Space(10);

        if (GUILayout.Button("Create Achievement", GUILayout.Height(30)))
        {
            CreateAchievementSO();
        }
    }

    private void CreateAchievementSO()
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Debug.LogError("업적 설명(Description)을 입력해주세요.");
            return;
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            Debug.LogError("파일 이름(File Name)을 입력해주세요.");
            return;
        }

        // 폴더 자동 생성
        if (!Directory.Exists("Assets/Resources/Achievements"))
        {
            Directory.CreateDirectory("Assets/Resources/Achievements");
        }

        // ScriptableObject 생성
        AchievementSO so = ScriptableObject.CreateInstance<AchievementSO>();

        // Description 저장
        so.description = description;

        // 자동 ID 생성 (description 기반 + GUID 일부)
        string safeDesc = description.Replace(" ", "_").Replace("/", "_").ToLower();
        string guidPart = System.Guid.NewGuid().ToString().Substring(0, 8);
        so.id = safeDesc + "_" + guidPart;

        // 보상
        so.rewardGem = rewardGem;

        // 파일 저장 경로 (입력받은 파일 이름 기반)
        string safeFileName = fileName.Replace(" ", "_").Replace("/", "_");
        string path = $"Assets/Resources/Achievements/{safeFileName}.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"업적 생성 완료: {path}\nID: {so.id}");
    }
}