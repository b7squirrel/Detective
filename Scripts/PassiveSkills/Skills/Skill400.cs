using UnityEngine;

/// <summary>
/// Invincible Body, 일정 시간동안 무적
/// </summary>
public class Skill400 : MonoBehaviour, ISkill
{
    public int Name { get; set; } = 400;
    public float CoolDownTime { get; set; } = 5f;
    public int Grade { get; set; }
    public int EvoStage { get; set; }
    float rate = .3f; // 등급, 스킬 레벨에 따라 얼마만큼 쿨타임에 영향을 미치게 할 지 정하는 비율

    float cooldownCounter;
    float realCoolDownTime;
    [SerializeField] float defaultInvincibleDuration;
    float realDuration;
    float durationTImer;

    public void Init(SkillManager _skillManager, CardData _cardData)
    {
        Grade = _cardData.Grade;
        EvoStage = _cardData.EvoStage;

        _skillManager.onSkill += UseSkill;
        realCoolDownTime = new Equation().GetCoolDownTime(rate, Grade, EvoStage, CoolDownTime);
        Debug.Log($" 디폴트 Duration = {defaultInvincibleDuration}");
        realDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, defaultInvincibleDuration);
        Debug.Log($" Real Duration = {realDuration}");
    }
    public void UseSkill()
    {
        if (cooldownCounter > realCoolDownTime)
        {
            // 스킬 발동 시간 끝나면 초기화
            if (durationTImer > realDuration)
            {
                cooldownCounter = 0;
                durationTImer = 0;
                GameManager.instance.IsPlayerInvincible = false;
                return;
            }
            // 스킬 계속 유지
            durationTImer += Time.deltaTime;
            Debug.Log($"skill Duartion {realDuration}");
            GameManager.instance.IsPlayerInvincible = true;
            return;
        }
        cooldownCounter += Time.deltaTime;
    }
}
