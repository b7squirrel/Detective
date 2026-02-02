using UnityEngine;

/// <summary>
/// 천하 무적 - Invincible Body
/// </summary>
public class Skill400 : SkillBase
{
    public override SkillType SkillType => SkillType.InvincibleBody;
    
    float baseDuration; // ⭐ 기본 지속시간 저장
    float realDuration;
    float durationTimer;
    
    [Header("Duration Upgrade")]
    [SerializeField] float durationIncreasePerLevel = 1f; // 레벨당 증가 시간 (초)
    
    [Header("Debug")]
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTimer;
    [SerializeField] int _durationUpgradeLevel;

    public override void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        base.Init(skillManager, cardData, data);
        
        // ⭐ 기본 지속시간 저장
        baseDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, data.baseDuration);
        
        // 업그레이드 적용된 지속시간 계산
        CalculateRealDuration();
        
        Debug.Log($"[Skill400] 초기화 완료 - Cooldown: {realCoolDownTime}초, Duration: {realDuration}초");
    }

    // ⭐ 지속시간 업그레이드 오버라이드
    public override void ApplyDurationUpgrade(int level)
    {
        base.ApplyDurationUpgrade(level);
        CalculateRealDuration();
        
        Debug.Log($"[Skill400] 🛡️ 무적 지속시간 업그레이드 LV{level} - {baseDuration}초 → {realDuration}초");
    }

    // ⭐ 실제 지속시간 계산
    void CalculateRealDuration()
    {
        realDuration = baseDuration + (durationUpgradeLevel * durationIncreasePerLevel);
    }

    public override void UseSkill()
    {
        // 월계수로 무적 상태라면 모든 타이머를 정지시킴
        if (GameManager.instance.IsPlayerItemInvincible) return;
        
        base.UseSkill();
        
        // ⭐ 디버그 값 업데이트
        UpdateDebugValues();
        
        if (skillCounter > realCoolDownTime)
        {
            // 스킬 발동 시간 끝나면 초기화
            if (durationTimer > realDuration)
            {
                skillCounter = 0;
                durationTimer = 0;
                GameManager.instance.IsPlayerInvincible = false;
                skillUi.PlayBadgeAnim("Done"); // ⭐ UI 애니메이션 추가
                return;
            }
            
            // ⭐ 스킬 활성화 시작
            if (!isActivated)
            {
                isActivated = true;
                skillUi.BadgeUpAnim();
                skillUi.PlayBadgeAnim("Duration"); // ⭐ UI 애니메이션 추가
            }
            
            // 스킬 계속 유지
            durationTimer += Time.deltaTime;
            GameManager.instance.IsPlayerInvincible = true;
            return;
        }
    }

    // ⭐ 디버그 값 업데이트
    void UpdateDebugValues()
    {
        _cooldownCounter = skillCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTimer = durationTimer;
        _durationUpgradeLevel = durationUpgradeLevel;
    }
}