using UnityEngine;

/// <summary>
/// 생명의 기운 - Life Recovery
/// </summary>
public class Skill100 : SkillBase
{
    public override SkillType SkillType => SkillType.SteelBody;
    
    float baseDuration; // 기본 지속시간
    float realDuration; // 업그레이드 적용된 지속시간
    float durationTimer;
    
    [Header("Healing Settings")]
    [SerializeField] float healInterval = 0.2f; // 0.2초마다 회복
    [SerializeField] int healAmountPerTick = 2; // 틱당 회복량 (초당 10)
    float healTimer;
    
    [Header("Duration Upgrade")]
    [SerializeField] float durationIncreasePerLevel = 2f; // 레벨당 증가 시간 (초)
    
    [Header("Visual Effects")]
    [SerializeField] Color healColor = new Color(0.2f, 1f, 0.2f, 0.5f); // 초록색 틴트 (선택사항)
    
    [Header("Debug")]
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTimer;
    [SerializeField] float _healTimer;
    [SerializeField] int _totalHealedAmount;
    [SerializeField] int _durationUpgradeLevel;

    Character character;

    public override void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        base.Init(skillManager, cardData, data);

        // 기본 지속시간 저장
        baseDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, data.baseDuration);

        // 업그레이드 적용된 지속시간 계산
        CalculateRealDuration();

        // Character 참조
        character = GameManager.instance.character;

        // ⭐ 디버그 로그
        Logger.LogError($"[Skill100-회복] 초기화 완료\n" +
                        $"  EvoStage: {EvoStage}\n" +
                        $"  Grade: {Grade}\n" +
                        $"  쿨다운: {realCoolDownTime}초\n" +
                        $"  지속시간: {realDuration}초\n" +
                        $"  회복량: 초당 {healAmountPerTick / healInterval}HP (틱당 {healAmountPerTick}HP)");
    }

    // 지속시간 업그레이드 오버라이드
    public override void ApplyDurationUpgrade(int level)
    {
        base.ApplyDurationUpgrade(level);
        CalculateRealDuration();
        
        Logger.LogError($"[Skill100] 💚 회복 지속시간 업그레이드 LV{level} - {baseDuration}초 → {realDuration}초");
    }

    // 실제 지속시간 계산
    void CalculateRealDuration()
    {
        realDuration = baseDuration + (durationUpgradeLevel * durationIncreasePerLevel);
    }

    public override void UseSkill()
    {
        base.UseSkill();
        UpdateDebugValues();
        
        if (skillCounter > realCoolDownTime)
        {
            if (durationTimer > realDuration)
            {
                // 스킬 종료
                skillCounter = 0;
                durationTimer = 0;
                healTimer = 0;
                isActivated = false;
                
                skillUi.PlayBadgeAnim("Done");
                
                Logger.LogError($"[Skill100] ✨ 회복 종료 - 총 회복량: {_totalHealedAmount}");
                _totalHealedAmount = 0;
                return;
            }
            else
            {
                // 스킬 지속
                if (!isActivated)
                {
                    isActivated = true;
                    healTimer = 0;
                    _totalHealedAmount = 0;
                    
                    skillUi.BadgeUpAnim();
                    skillUi.PlayBadgeAnim("Duration");
                    
                    Logger.LogError($"[Skill100] 💚 회복 시작! (지속시간: {realDuration}초)");
                }
                
                // 회복 처리
                ApplyHealing();
                
                durationTimer += Time.deltaTime;
                return;
            }
        }
    }

    void ApplyHealing()
    {
        if (character == null) return;
        if (character.GetCurrentHP() >= character.MaxHealth) return; // 이미 최대 체력이면 회복 안 함
        
        healTimer += Time.deltaTime;
        
        if (healTimer >= healInterval)
        {
            // 회복 실행 (아이템 회복이 아니므로 false)
            character.Heal(healAmountPerTick, false);
            
            _totalHealedAmount += healAmountPerTick;
            
            // 타이머 리셋
            healTimer -= healInterval;
        }
    }

    void UpdateDebugValues()
    {
        _cooldownCounter = skillCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTimer = durationTimer;
        _healTimer = healTimer;
        _durationUpgradeLevel = durationUpgradeLevel;
    }
}