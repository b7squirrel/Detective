using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

[CreateAssetMenu(fileName = "AchievementTexts", menuName = "Localization/Achievement Texts")]
public class AchievementTexts : ScriptableObject
{
    [Header("Achievement Display Names")]
    [Tooltip("AchievementSO의 id와 매칭됩니다")]
    public AchievementLocalizedText[] achievements = new AchievementLocalizedText[0];
    
    [System.Serializable]
    public class AchievementLocalizedText
    {
        [Tooltip("AchievementSO.id (내부 식별용)")]
        public string achievementId;
        
        [Tooltip("업적 이름")]
        public string title;
        
        [Tooltip("업적 설명")]
        [TextArea(2, 4)]
        public string description;
    }
    
    // ID로 업적 텍스트 찾기
    public AchievementLocalizedText GetAchievementText(string achievementId)
    {
        foreach (var ach in achievements)
        {
            if (ach != null && ach.achievementId == achievementId)
                return ach;
        }
        Debug.LogWarning($"Achievement text not found: {achievementId}");
        return null;
    }
    
    public string GetTitle(string achievementId)
    {
        var text = GetAchievementText(achievementId);
        return text?.title ?? achievementId;
    }
    
    public string GetDescription(string achievementId)
    {
        var text = GetAchievementText(achievementId);
        return text?.description ?? "";
    }
    
#if UNITY_EDITOR
    private void OnEnable()
    {
        // 이미 초기화되어 있으면 스킵
        if (achievements != null && achievements.Length > 0 && 
            !string.IsNullOrEmpty(achievements[0]?.achievementId))
            return;
        
        // 에디터에서만 자동 초기화
        if (!Application.isPlaying)
        {
            InitializeAchievements();
        }
    }
    
    private void InitializeAchievements()
    {
        // 프로젝트에서 모든 AchievementSO 찾기
        string[] guids = AssetDatabase.FindAssets("t:AchievementSO");
        
        var achievementList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<AchievementSO>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(ach => ach != null && !string.IsNullOrEmpty(ach.id))
            .OrderBy(ach => ach.id)
            .ToList();
        
        if (achievementList.Count == 0)
        {
            Debug.LogWarning("No AchievementSO found in project. Achievements not initialized.");
            return;
        }
        
        achievements = new AchievementLocalizedText[achievementList.Count];
        
        for (int i = 0; i < achievementList.Count; i++)
        {
            achievements[i] = new AchievementLocalizedText
            {
                achievementId = achievementList[i].id,
                title = "", // 비워둠 - 수동 입력 필요
                description = "" // 비워둠 - 수동 입력 필요
            };
        }
        
        EditorUtility.SetDirty(this);
        Debug.Log($"AchievementTexts initialized with {achievementList.Count} achievements");
    }
#endif
}