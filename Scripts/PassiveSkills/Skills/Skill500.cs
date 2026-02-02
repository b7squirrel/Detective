using UnityEngine;

/// <summary>
/// 동료 강화 - Ally Power Boost
/// </summary>
public class Skill500 : SkillBase
{
    public override SkillType SkillType => SkillType.PartyTime;
    
    float baseDuration; // 기본 지속시간
    float realDuration; // 업그레이드 적용된 지속시간
    float durationTimer;
    
    [Header("Boost Settings")]
    float damageMultiplier; 
    
    [Header("Duration Upgrade")]
    [SerializeField] float durationIncreasePerLevel = 2f; // 레벨당 증가 시간 (초)
    
    [Header("Visual Effects")]
    [SerializeField] Color boostColor = new Color(1f, 0.5f, 0f, 0.5f); // 주황색 (선택사항)
    
    [Header("Debug")]
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTimer;
    [SerializeField] float _damageMultiplier;
    [SerializeField] int _boostedAllyCount;
    [SerializeField] int _durationUpgradeLevel;

    public override void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        base.Init(skillManager, cardData, data);

        baseDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, data.baseDuration);
        CalculateRealDuration();

        // ⭐ 이 부분 추가
        damageMultiplier = 1.5f + (Grade * 0.1f) + (EvoStage * 0.15f);

        Logger.LogError($"[Skill500-파티 타임] 초기화 완료\n" +
                        $"  EvoStage: {EvoStage}\n" +
                        $"  Grade: {Grade}\n" +
                        $"  쿨다운: {realCoolDownTime}초\n" +
                        $"  지속시간: {realDuration}초\n" +
                        $"  데미지 배수: {damageMultiplier:F2}x (동료 공격력 {((damageMultiplier - 1) * 100):F0}% 증가)");
    }

    // 지속시간 업그레이드 오버라이드
    public override void ApplyDurationUpgrade(int level)
    {
        base.ApplyDurationUpgrade(level);
        CalculateRealDuration();
        
        Logger.LogError($"[Skill500] 🔥 동료 강화 지속시간 업그레이드 LV{level} - {baseDuration}초 → {realDuration}초");
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
                isActivated = false;
                
                ReleaseBoost();
                skillUi.PlayBadgeAnim("Done");
                
                Logger.LogError($"[Skill500] ✨ 동료 강화 종료");
                return;
            }
            else
            {
                // 스킬 지속
                if (!isActivated)
                {
                    isActivated = true;
                    
                    skillUi.BadgeUpAnim();
                    skillUi.PlayBadgeAnim("Duration");
                    
                    ApplyBoost();
                    
                    Logger.LogError($"[Skill500] 🔥 동료 강화 시작! (지속시간: {realDuration}초, 배수: {damageMultiplier}x)");
                }
                
                durationTimer += Time.deltaTime;
                return;
            }
        }
    }

    void ApplyBoost()
    {
        WeaponBase[] allWeapons = FindObjectsOfType<WeaponBase>();
        if (allWeapons == null || allWeapons.Length == 0) return;
        
        int boostedCount = 0;
        
        for (int i = 0; i < allWeapons.Length; i++)
        {
            // 동료들만 강화 (InitialWeapon == false)
            if (!allWeapons[i].InitialWeapon)
            {
                allWeapons[i].SetAllyDamageMultiplier(damageMultiplier);
                boostedCount++;
                
                // ⭐ 선택사항: 시각적 효과 추가
                // WeaponContainerAnim containerAnim = allWeapons[i].GetComponentInParent<WeaponContainerAnim>();
                // if (containerAnim != null)
                // {
                //     containerAnim.Scale(1.2f); // 크기 증가 효과
                // }
            }
        }
        
        _boostedAllyCount = boostedCount;
        Logger.LogError($"[Skill500] 💪 {boostedCount}명의 동료 강화 적용!");
    }

    void ReleaseBoost()
    {
        WeaponBase[] allWeapons = FindObjectsOfType<WeaponBase>();
        if (allWeapons == null || allWeapons.Length == 0) return;
        
        int releasedCount = 0;
        
        for (int i = 0; i < allWeapons.Length; i++)
        {
            if (!allWeapons[i].InitialWeapon)
            {
                allWeapons[i].ResetAllyDamageMultiplier();
                releasedCount++;
                
                // ⭐ 선택사항: 크기 원래대로
                // WeaponContainerAnim containerAnim = allWeapons[i].GetComponentInParent<WeaponContainerAnim>();
                // if (containerAnim != null)
                // {
                //     containerAnim.Scale(0.8f); // 원래 크기
                // }
            }
        }
        
        Logger.LogError($"[Skill500] 💨 {releasedCount}명의 동료 강화 해제");
    }

    void UpdateDebugValues()
    {
        _cooldownCounter = skillCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTimer = durationTimer;
        _damageMultiplier = damageMultiplier;
        _durationUpgradeLevel = durationUpgradeLevel;
    }
}