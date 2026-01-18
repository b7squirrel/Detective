using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어에 붙여서 사용.
/// 스킬을 검색해서 쿨다운을 설정하고 UseSkill을 이벤트에 등록
/// </summary>
public class SkillManager : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] SkillDataContainer skillDataContainer;
    [SerializeField] GameObject[] skillObjects;
    
    private Dictionary<SkillType, ISkill> skillDictionary = new Dictionary<SkillType, ISkill>();
    private ISkill currentSkill;
    
    public event Action onSkill;

    void Start()
    {
        InitializeSkillDictionary();
        
        SkillType skillType = GetSkillTypeFromStartingData();
        CardData playerCardData = GameManager.instance.startingDataContainer.GetPlayerCardData();
        
        InitSkill(skillType, playerCardData);
    }

    void Update()
    {
        if (onSkill == null) return;
        onSkill?.Invoke();
    }

    /// <summary>
    /// 모든 스킬을 Dictionary에 등록
    /// </summary>
    void InitializeSkillDictionary()
    {
        skillDictionary.Clear();
        
        foreach (GameObject skillObj in skillObjects)
        {
            ISkill skill = skillObj.GetComponent<ISkill>();
            if (skill != null)
            {
                // Awake에서 설정된 SkillType을 가져오기 위해 임시로 활성화
                skillObj.SetActive(true);
                
                if (!skillDictionary.ContainsKey(skill.SkillType))
                {
                    skillDictionary.Add(skill.SkillType, skill);
                }
                
                // 다시 비활성화 (Init에서 활성화됨)
                skillObj.SetActive(false);
            }
        }
        
        Debug.Log($"스킬 Dictionary 초기화 완료: {skillDictionary.Count}개 스킬 등록됨");
    }

    /// <summary>
    /// StartingDataContainer에서 스킬 타입 가져오기
    /// </summary>
    SkillType GetSkillTypeFromStartingData()
    {
        int skillName = GameManager.instance.startingDataContainer.GetSkillName();
        
        // 기존 숫자 시스템을 SkillType으로 변환
        // 100 -> SteelBody, 200 -> SluggishSlumber, etc.
        int skillNumber = (skillName / 100) % 10;
        
        SkillType skillType = (SkillType)(skillNumber * 100);
        
        Debug.Log($"받은 스킬 번호: {skillNumber}, 변환된 SkillType: {skillType}");
        
        return skillType;
    }

    /// <summary>
    /// 특정 스킬을 초기화하고 활성화
    /// </summary>
    void InitSkill(SkillType skillType, CardData playerCardData)
    {
        if (skillDataContainer == null)
        {
            Debug.LogError("SkillDataContainer가 할당되지 않았습니다!");
            return;
        }
        
        if (skillDictionary.TryGetValue(skillType, out ISkill skill))
        {
            SkillData skillData = skillDataContainer.GetSkillData(skillType);
            
            if (skillData == null)
            {
                Debug.LogError($"스킬 데이터를 찾을 수 없습니다: {skillType}");
                return;
            }
            
            // 스킬 오브젝트 활성화
            ((MonoBehaviour)skill).gameObject.SetActive(true);
            
            // 스킬 초기화
            skill.Init(this, playerCardData, skillData);
            currentSkill = skill;
            
            Debug.Log($"스킬 초기화 완료: {skillType} ({skillData.skillName})");
        }
        else
        {
            Debug.LogError($"스킬을 찾을 수 없습니다: {skillType}");
        }
    }

    /// <summary>
    /// 현재 활성화된 스킬 반환
    /// </summary>
    public ISkill GetCurrentSkill()
    {
        return currentSkill;
    }
}