using UnityEngine;

/// <summary>
/// 파티 타임 - Party Time (Spicy Booster) 오리들의 공격력 증가
/// </summary>
public class Skill500 : SkillBase
{
    public override SkillType SkillType => SkillType.PartyTime;
    int defaultDamageBonus;
    int realDamageBonus;
    float realDuration;
    float durationTimer;
    bool isHitAnimPlaying;
    bool isDurationAnimPlaying;
    
    [Header("Debug")]
    [SerializeField] int _defaultDamageBonus;
    [SerializeField] int _realDamageBonus;
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTimer;
    [SerializeField] float _characterDamageBonus;

    public override void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        base.Init(skillManager, cardData, data);
        
        defaultDamageBonus = GameManager.instance.character.DamageBonus;
        
        realDuration = new Equation().GetSkillDuration(
            rate, Grade, EvoStage, data.baseDuration);
        
        realDamageBonus = new Equation().GetSkillDamageBonus(
            rate, Grade, EvoStage, defaultDamageBonus);
    }

    public override void UseSkill()
    {
        base.UseSkill();

        if (skillCounter > realCoolDownTime)
        {
            if (isHitAnimPlaying == false)
            {
                skillUi.BadgeUpAnim();
                isHitAnimPlaying = true;
            }
            
            // 스킬 발동 시간 끝나면 초기화
            if (durationTimer > realDuration)
            {
                skillCounter = 0;
                durationTimer = 0;
                GameManager.instance.character.DamageBonus = defaultDamageBonus;
                skillUi.PlayBadgeAnim("Done");
                isDurationAnimPlaying = false;
                isHitAnimPlaying = false;
                return;
            }
            else
            {
                // 스킬 계속 유지
                durationTimer += Time.deltaTime;
                GameManager.instance.character.DamageBonus = realDamageBonus;
                
                if (isDurationAnimPlaying == false)
                {
                    skillUi.PlayBadgeAnim("Duration");
                    isDurationAnimPlaying = true;
                }
                return;
            }
        }
    }

    void DebugValues()
    {
        _defaultDamageBonus = defaultDamageBonus;
        _realDamageBonus = realDamageBonus;
        _cooldownCounter = skillCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTimer = durationTimer;
        _characterDamageBonus = GameManager.instance.character.DamageBonus;
    }
}