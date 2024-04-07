using System;
using UnityEngine;

/// <summary>
/// 플레이어에 붙여서 사용.
/// 스킬을 검색해서 쿨다운을 설정하고 UseSkill을 이벤트에 등록
/// </summary>
public class SkillManager : MonoBehaviour
{
    [SerializeField] GameObject[] skillObject;
    ISkill[] skills = new ISkill[5];
    event Action onSkill;
    float cooldownTime;
    float skillTimeCounter;

    void Start()
    {
        for (int i = 0; i < skillObject.Length; i++)
        {
            skills[i] = skillObject[i].GetComponent<ISkill>();
        }
        Debug.Log("Number of skills = " + skills.Length);
        int skillName = GameManager.instance.startingDataContainer.GetSkillName();
        CardData playerCardData = GameManager.instance.startingDataContainer.GetPlayerCardData();
        Init(skillName, playerCardData);
        Debug.Log("Skill Name = " + skillName);
    }
    void Update()
    {
        if (onSkill == null) return;

        onSkill?.Invoke();
    }
    /// <summary>
    /// 이름으로 스킬을 찾아서 등록시킴
    /// </summary>
    public void Init(int _Name, CardData _playerCardData)
    {
        int skill = (_Name / 100) % 10; // 100의 자리수를 얻어 낸다.
        Debug.Log($"받은 스킬 이름 {skill}");

        for (int i = 0; i < skills.Length; i++)
        {
            Debug.Log($"가지고 있는 스킬 {(skills[i].Name / 100) % 10 }");

            if ((skills[i].Name / 100) % 10 == skill)
            {
                onSkill += skills[i].UseSkill;
                Debug.Log($"skill Init {skills[i].Name}");
                return;
            }
        }
        Debug.Log("스킬이 없습니다.");
    }
}

