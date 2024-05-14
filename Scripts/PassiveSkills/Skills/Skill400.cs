using UnityEngine;

/// <summary>
/// Invincible Body, 일정 시간동안 무적
/// </summary>
public class Skill400 : SkillBase
{
    [SerializeField] float defaultInvincibleDuration;
    float realDuration;
    float durationTImer;

    private void Awake()
    {
        Name = 400;
        CoolDownTime = 5f;
    }
    public override void Init(SkillManager _skillManager, CardData _cardData)
    {
        base.Init(_skillManager, _cardData);
        Debug.Log($" 디폴트 Duration = {defaultInvincibleDuration}");
        realDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, defaultInvincibleDuration);
        Debug.Log($" Real Duration = {realDuration}");
    }
    public override void UseSkill()
    {
        base.UseSkill();

        if (skillCounter > realCoolDownTime)
        {
            // 스킬 발동 시간 끝나면 초기화
            if (durationTImer > realDuration)
            {
                skillCounter = 0;
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
        skillCounter += Time.deltaTime;
    }
}
