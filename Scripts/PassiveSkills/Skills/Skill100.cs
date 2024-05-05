using UnityEngine;

/// <summary>
/// 모든 오리들의 방어 콜라이더 100% (아직 구현 안했음)
/// 지금은 skill500을 복제해 놓았음
/// Steel Body
/// </summary>
public class Skill100 : MonoBehaviour, ISkill
{
    public int Name { get; set; } = 100;
    public float CoolDownTime { get; set; } = 5f;
    public int Grade { get; set; }
    public int EvoStage { get; set; }
    float rate = .3f; // 등급, 스킬 레벨에 따라 얼마만큼 쿨타임에 영향을 미치게 할 지 정하는 비율

    int defaultDamageBonus;
    int realDamageBonus; // 디폴트 데미지에서 계산이 적용된 후의 데미지, 실제로 적에게 들어가는 데미지

    float cooldownCounter;
    float realCoolDownTime;
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
    public void Init(SkillManager _skillManager, CardData _cardData)
    {
        Grade = _cardData.Grade;
        EvoStage = _cardData.EvoStage;

        defaultDamageBonus = GameManager.instance.character.DamageBonus;

        _skillManager.onSkill += UseSkill;
        realCoolDownTime = new Equation().GetCoolDownTime(rate, Grade, EvoStage, CoolDownTime);
        realDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, defaultDuration);
        realDamageBonus = new Equation().GetSkillDamageBonus(rate, Grade, EvoStage, defaultDamageBonus);
    }

    public void UseSkill()
    {
        //DebugValues();
        if (cooldownCounter > realCoolDownTime)
        {
            // 스킬 발동 시간 끝나면 초기화
            if (durationTImer > realDuration)
            {
                cooldownCounter = 0;
                durationTImer = 0;
                GameManager.instance.character.DamageBonus = defaultDamageBonus;
                return;
            }
            // 스킬 계속 유지
            durationTImer += Time.deltaTime;
            GameManager.instance.character.DamageBonus = realDamageBonus;
            return;
        }
        cooldownCounter += Time.deltaTime;
    }
    void DebugValues()
    {
        _defaultDamageBonus = defaultDamageBonus;
        _realDamageBonus = realDamageBonus;
        _cooldownCounter = cooldownCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTImer = durationTImer;
        _characterDamageBonus = GameManager.instance.character.DamageBonus;
    }
}
