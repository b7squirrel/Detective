using System;
using UnityEngine;

/// <summary>
/// 일정 시간동안 무적
/// </summary>
public class Skill400 : MonoBehaviour, ISkill
{
    public int Name { get; set; } = 400;
    public float CoolDownTime { get; set; } = 5f;
    public int Grade { get; set; }
    public int EvoStage { get; set; }
    float skillCounter;
    float rate = .3f; // 등급, 스킬 레벨에 따라 얼마만큼 쿨타임에 영향을 미치게 할 지 정하는 비율

    float realCoolDown;
    [SerializeField] float defaultInvincibleDuration;
    float realDuration;
    float durationTImer;

    public void Init(SkillManager _skillManager, CardData _cardData)
    {
        Grade = _cardData.Grade;
        EvoStage = _cardData.EvoStage;

        _skillManager.onSkill += UseSkill;
        realCoolDown = new Equation().GetCoolDownTime(rate, Grade, EvoStage, CoolDownTime);
        Debug.Log($" 디폴트 Duration = {defaultInvincibleDuration}");
        realDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, defaultInvincibleDuration);
    }
    public void UseSkill()
    {
        if (skillCounter > 2)
        {
            if (durationTImer > realDuration)
            {
                skillCounter = 0;
                durationTImer = 0;
                GameManager.instance.IsPlayerInvincible = false;
                return;
            }
            durationTImer += Time.deltaTime;
            Debug.Log($"skill Duartion {realDuration}");
            GameManager.instance.IsPlayerInvincible = true;
            return;
        }
        skillCounter += Time.deltaTime;
    }
}
