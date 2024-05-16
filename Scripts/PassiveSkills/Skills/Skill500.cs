using UnityEngine;

/// <summary>
/// Spicy Booster, 모든 오리들의 공격력 증가.
/// </summary>
public class Skill500 : SkillBase
{
    int defaultDamageBonus;
    int realDamageBonus; // 디폴트 데미지에서 계산이 적용된 후의 데미지, 실제로 적에게 들어가는 데미지

    [SerializeField] float defaultDuration; // 인스펙터에서 입력
    float realDuration;
    float durationTImer;

    [Header("Debug")]
    [SerializeField] int _defaultDamageBonus;
    [SerializeField] int _realDamageBonus; 
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTImer;
    [SerializeField] float _characterDamageBonus;

    private void Awake()
    {
        Name = 500;
        CoolDownTime = 5f;
    }
    public override void Init(SkillManager _skillManager, CardData _cardData)
    {
        base.Init(_skillManager, _cardData);
        defaultDamageBonus = GameManager.instance.character.DamageBonus;

        realDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, defaultDuration);
        realDamageBonus = new Equation().GetSkillDamageBonus(rate, Grade, EvoStage, defaultDamageBonus);
    }

    public override void UseSkill()
    {
        base.UseSkill();

        //DebugValues();
        if (skillCounter > realCoolDownTime)
        {
            // 스킬 발동 시간 끝나면 초기화
            if (durationTImer > realDuration)
            {
                skillCounter = 0;
                durationTImer = 0;
                GameManager.instance.character.DamageBonus = defaultDamageBonus;
                return;
            }
            else
            {
                // 스킬 계속 유지
                durationTImer += Time.deltaTime;
                GameManager.instance.character.DamageBonus = realDamageBonus;
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
        _durationTImer = durationTImer;
        _characterDamageBonus = GameManager.instance.character.DamageBonus;
    }
}
