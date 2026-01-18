using UnityEngine;

/// <summary>
/// 느림보 최면술 - Sluggish Slumber
/// </summary>
public class Skill200 : SkillBase
{
    public override SkillType SkillType => SkillType.SluggishSlumber;
    
    float realDuration;
    float durationTimer;
    float slownessFactor;
    
    [Header("Visual Effects")]
    [SerializeField] Color slowColor = new Color(0.5f, 0.5f, 1f, 1f); // 파란색 틴트
    
    [Header("Debug")]
    [SerializeField] float _cooldownCounter;
    [SerializeField] float _realCoolDownTime;
    [SerializeField] float _realDuration;
    [SerializeField] float _durationTimer;
    [SerializeField] float _slownessFactor;
    [SerializeField] int _affectedEnemyCount;

    public override void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        base.Init(skillManager, cardData, data);
        
        realDuration = new Equation().GetSkillDuration(
            rate, Grade, EvoStage, data.baseDuration);
        
        slownessFactor = new Equation().GetSlowSpeedFactor(Grade, EvoStage);
        
        Debug.Log($"[Skill200] 초기화 완료 - Cooldown: {realCoolDownTime}초, Duration: {realDuration}초, Slow: {slownessFactor * 100}%");
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
                    Debug.Log($"[Skill200] ⚡ 스킬 발동! (Duration: {realDuration}초)");
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
            
            // 느림 효과 적용
            enemy.IsSlowed = true;
            enemy.CastSlownessToEnemy(slownessFactor);
            enemy.SetTintColor(slowColor); // 👈 간단하게 호출!
            
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
            
            // 느림 해제
            enemy.IsSlowed = false;
            enemy.ResetCurrentSpeedToDefault();
            enemy.ResetTintColor(); // 👈 간단하게 호출!
            
            releasedCount++;
        }
        
        Debug.Log($"[Skill200] 💨 {releasedCount}명 느림 해제");
    }

    void DebugValues()
    {
        _cooldownCounter = skillCounter;
        _realCoolDownTime = realCoolDownTime;
        _realDuration = realDuration;
        _durationTimer = durationTimer;
        _slownessFactor = slownessFactor;
    }
}