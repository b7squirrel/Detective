using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDataContainer", menuName = "Game/Skill Data Container")]
public class SkillDataContainer : ScriptableObject
{
    [SerializeField] private List<SkillData> skillDataList = new List<SkillData>();
    
    private Dictionary<SkillType, SkillData> skillDataDict;
    
    public void Initialize()
    {
        if (skillDataDict != null) return;
        
        skillDataDict = new Dictionary<SkillType, SkillData>();
        foreach (var data in skillDataList)
        {
            if (data != null && !skillDataDict.ContainsKey(data.skillType))
            {
                skillDataDict.Add(data.skillType, data);
            }
        }
    }
    
    public SkillData GetSkillData(SkillType type)
    {
        Initialize();
        
        if (skillDataDict.TryGetValue(type, out SkillData data))
        {
            return data;
        }
        
        Debug.LogWarning($"스킬 데이터를 찾을 수 없습니다: {type}");
        return null;
    }
    
    public Sprite GetSkillIcon(SkillType type)
    {
        SkillData data = GetSkillData(type);
        return data != null ? data.skillIcon : null;
    }
}