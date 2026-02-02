using UnityEngine;

/// <summary>
/// 느림보 최면술 - Sluggish Slumber
/// </summary>
public class Skill200 : SkillBase
{
    public override SkillType SkillType => SkillType.SluggishSlumber;
    
    float baseDuration; // ⭐ 기본 지속시간 저장
    float realDuration;
    float durationTimer;
    float slownessFactor;
    
    [Header("Duration Upgrade")]
    [SerializeField] float durationIncreasePerLevel = 2f; // 레벨당 증가 시간 (초)
    
    [Header("Visual Effects")]
    [SerializeField] Color slowColor = new Color(0.5f, 0.5f, 1f, 1f);
    
    [Header("Debug")]
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTimer;
    [SerializeField] float _slownessFactor;
    [SerializeField] int _affectedEnemyCount;
    [SerializeField] int _durationUpgradeLevel;

    public override void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        base.Init(skillManager, cardData, data);
        
        // ⭐ 기본 지속시간 저장
        baseDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, data.baseDuration);
        
        // 업그레이드 적용된 지속시간 계산
        CalculateRealDuration();
        
        slownessFactor = new Equation().GetSlowSpeedFactor(Grade, EvoStage);
        
        Debug.Log($"[Skill200] 초기화 완료 - Cooldown: {realCoolDownTime}초, Duration: {realDuration}초, Slow: {slownessFactor * 100}%");
    }

    // ⭐ 지속시간 업그레이드 오버라이드
    public override void ApplyDurationUpgrade(int level)
    {
        base.ApplyDurationUpgrade(level);
        CalculateRealDuration();
        
        Debug.Log($"[Skill200] 💫 지속시간 업그레이드 LV{level} - {baseDuration}초 → {realDuration}초");
    }

    // ⭐ 실제 지속시간 계산
    void CalculateRealDuration()
    {
        realDuration = baseDuration + (durationUpgradeLevel * durationIncreasePerLevel);
    }

    public override void UseSkill()
    {
        base.UseSkill();
        DebugValues();
        
        if (skillCounter > realCoolDownTime)
        {
            if (durationTimer > realDuration)
            {
                // 스킬 종료
                skillCounter = 0;
                durationTimer = 0;
                isActivated = false;
                ReleaseSlowEffect();
                skillUi.PlayBadgeAnim("Done");
            }
            else
            {
                // 스킬 지속
                if (!isActivated)
                {
                    isActivated = true;
                    skillUi.BadgeUpAnim();
                    skillUi.PlayBadgeAnim("Duration");
                }
                
                ApplySlowEffect();
                durationTimer += Time.deltaTime;
                return;
            }
        }
    }

    void ApplySlowEffect()
    {
        Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
        if (allEnemies == null || allEnemies.Length == 0) return;
        
        int slowedCount = 0;
        for (int i = 0; i < allEnemies.Length; i++)
        {
            EnemyBase enemy = allEnemies[i].GetComponent<EnemyBase>();
            if (enemy == null || enemy.IsSlowed) continue;
            
            enemy.IsSlowed = true;
            enemy.CastSlownessToEnemy(slownessFactor);
            enemy.SetTintColor(slowColor);
            slowedCount++;
        }
        
        _affectedEnemyCount = slowedCount;
    }

    void ReleaseSlowEffect()
    {
        Collider2D[] allSlowEnemies = EnemyFinder.instance.GetAllEnemies();
        if (allSlowEnemies == null || allSlowEnemies.Length == 0) return;
        
        int releasedCount = 0;
        for (int i = 0; i < allSlowEnemies.Length; i++)
        {
            EnemyBase enemy = allSlowEnemies[i].GetComponent<EnemyBase>();
            if (enemy == null || !enemy.IsSlowed) continue;
            
            enemy.IsSlowed = false;
            enemy.ResetCurrentSpeedToDefault();
            enemy.ResetTintColor();
            releasedCount++;
        }
    }

    void DebugValues()
    {
        _cooldownCounter = skillCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTimer = durationTimer;
        _slownessFactor = slownessFactor;
        _durationUpgradeLevel = durationUpgradeLevel;
    }
}