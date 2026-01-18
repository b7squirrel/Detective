using UnityEngine;

/// <summary>
/// 천하 무적 - Invincible Body
/// </summary>
public class Skill400 : SkillBase
{
    public override SkillType SkillType => SkillType.InvincibleBody;
    float realDuration;
    float durationTimer;

    public override void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        base.Init(skillManager, cardData, data);

        realDuration = new Equation().GetSkillDuration(
            rate, Grade, EvoStage, data.baseDuration);
    }

    public override void UseSkill()
    {
        // 월계수로 무적 상태라면 모든 타이머를 정지시킴
        if (GameManager.instance.IsPlayerItemInvincible) return;
        
        base.UseSkill();

        if (skillCounter > realCoolDownTime)
        {
            // 스킬 발동 시간 끝나면 초기화
            if (durationTimer > realDuration)
            {
                skillCounter = 0;
                durationTimer = 0;
                GameManager.instance.IsPlayerInvincible = false;
                return;
            }
            
            // 스킬 계속 유지
            durationTimer += Time.deltaTime;
            GameManager.instance.IsPlayerInvincible = true;
            return;
        }
        
        skillCounter += Time.deltaTime;
    }
}