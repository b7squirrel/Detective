using UnityEngine;

/// <summary>
/// 모든 오리들의 방어 콜라이더 100% (아직 구현 안했음)
/// 지금은 skill500을 복제해 놓았음
/// Steel Body
/// </summary>
public class Skill100 : SkillBase
{
    public override SkillType SkillType => SkillType.SteelBody;
    float realDuration;
    float durationTimer;
    float slownessFactor;
    
    [Header("Debug")]
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTimer;

    public override void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        base.Init(skillManager, cardData, data);

        realDuration = new Equation().GetSkillDuration(
            rate, Grade, EvoStage, data.baseDuration);

        slownessFactor = new Equation().GetSlowSpeedFactor(Grade, EvoStage);
    }

    public override void UseSkill()
    {
        base.UseSkill();

        if (skillCounter > realCoolDownTime)
        {
            if (durationTimer > realDuration)
            {
                skillCounter = 0;
                durationTimer = 0;
                // 초기화
            }
            else
            {
                // duration 쿨타임이 끝나지 않았다면 콜라이더 유지
                durationTimer += Time.deltaTime;
                return;
            }
        }
    }

    void DebugValues()
    {
        _cooldownCounter = skillCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTimer = durationTimer;
    }
}
