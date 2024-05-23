using UnityEngine;

/// <summary>
/// 모든 오리들의 방어 콜라이더 100% (아직 구현 안했음)
/// 지금은 skill500을 복제해 놓았음
/// Steel Body
/// </summary>
public class Skill100 : SkillBase
{
    [SerializeField] float defaultDuration; // 인스펙터에서 입력
    float realDuration;
    float durationTImer;

    float slownessFactor; // 적의 속도 앞에 곱해줘서 속도를 낮추는 역할을 하는 팩터

    [Header("Debug")]
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTImer;
    private void Awake()
    {
        Name = 100;
        CoolDownTime = 5f;
    }
    public override void Init(SkillManager _skillManager, CardData _cardData)
    {
        base.Init(_skillManager, _cardData);

        _skillManager.onSkill += UseSkill;
        realCoolDownTime = new Equation().GetCoolDownTime(rate, Grade, EvoStage, CoolDownTime);
        realDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, defaultDuration);
        slownessFactor = new Equation().GetSlowSpeedFactor(Grade, EvoStage);
    }

    public void UseSkill()
    {
        //DebugValues();
        base.UseSkill();
        //DebugValues();
        if (skillCounter > realCoolDownTime)
        {
            if (durationTImer > realDuration)
            {
                skillCounter = 0;
                durationTImer = 0;
                
                // 초기화 시키기
            }
            else
            {
                // duration 쿨타임이 끝나지 않았다면 콜라이더 유지
                
                durationTImer += Time.deltaTime;
                return;
            }
        }
    }
    void DebugValues()
    {
        _cooldownCounter = skillCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTImer = durationTImer;
    }
}
