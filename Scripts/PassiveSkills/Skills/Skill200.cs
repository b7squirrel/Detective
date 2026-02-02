using System.Collections.Generic;
using UnityEngine;

public class Skill200 : SkillBase
{
    public override SkillType SkillType => SkillType.SluggishSlumber;
    
    float baseDuration;
    float realDuration;
    float durationTimer;
    float slownessFactor;
    
    [Header("Duration Upgrade")]
    [SerializeField] float durationIncreasePerLevel = 2f;
    
    [Header("Visual Effects")]
    [SerializeField] Color slowColor = new Color(0.5f, 0.5f, 1f, 1f);
    
    // ⭐ 느려진 적들을 관리하는 리스트
    private List<EnemyBase> slowedEnemies = new List<EnemyBase>();
    
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
        
        baseDuration = new Equation().GetSkillDuration(rate, Grade, EvoStage, data.baseDuration);
        CalculateRealDuration();
        slownessFactor = new Equation().GetSlowSpeedFactor(Grade, EvoStage);
        
        Logger.LogError($"[Skill200-느림보 최면술] 초기화 완료\n" +
                        $"  EvoStage: {EvoStage}\n" +
                        $"  Grade: {Grade}\n" +
                        $"  쿨다운: {realCoolDownTime}초\n" +
                        $"  지속시간: {realDuration}초\n" +
                        $"  느림 효과: {slownessFactor * 100}% 속도 감소");
    }

    public override void ApplyDurationUpgrade(int level)
    {
        base.ApplyDurationUpgrade(level);
        CalculateRealDuration();
        
        Debug.Log($"[Skill200] 💫 지속시간 업그레이드 LV{level} - {baseDuration}초 → {realDuration}초");
    }

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
                    
                    // ⭐ 스킬 시작 시 한 번만 적용
                    ApplySlowEffect();
                }
                
                // ⭐ 스킬 지속 중에는 새로 생성된 적만 체크
                CheckAndApplyToNewEnemies();
                
                durationTimer += Time.deltaTime;
                return;
            }
        }
    }

    // ⭐ 스킬 시작 시 한 번만 실행
    void ApplySlowEffect()
    {
        Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
        if (allEnemies == null || allEnemies.Length == 0) return;
        
        slowedEnemies.Clear(); // 리스트 초기화
        
        for (int i = 0; i < allEnemies.Length; i++)
        {
            EnemyBase enemy = allEnemies[i].GetComponent<EnemyBase>();
            if (enemy == null) continue;
            
            ApplySlowToEnemy(enemy);
        }
        
        _affectedEnemyCount = slowedEnemies.Count;
    }

    // ⭐ 스킬 지속 중 새로 생성된 적만 체크 (가벼운 체크)
    void CheckAndApplyToNewEnemies()
    {
        // 프레임당 최대 5마리만 체크 (성능 최적화)
        Collider2D[] allEnemies = EnemyFinder.instance.GetAllEnemies();
        if (allEnemies == null || allEnemies.Length == 0) return;
        
        int checkCount = 0;
        int maxChecksPerFrame = 5; // 프레임당 최대 5마리만
        
        for (int i = 0; i < allEnemies.Length && checkCount < maxChecksPerFrame; i++)
        {
            EnemyBase enemy = allEnemies[i].GetComponent<EnemyBase>();
            if (enemy == null || enemy.IsSlowed) continue;
            
            // 새로운 적 발견
            ApplySlowToEnemy(enemy);
            checkCount++;
        }
    }

    // ⭐ 개별 적에게 느림 적용 + 리스트에 추가
    void ApplySlowToEnemy(EnemyBase enemy)
    {
        if (enemy.IsSlowed) return; // 이미 느린 적은 스킵
        
        enemy.IsSlowed = true;
        enemy.CastSlownessToEnemy(slownessFactor);
        enemy.SetTintColor(slowColor);
        
        slowedEnemies.Add(enemy);
        
        // ⭐ 적이 착지할 때마다 재적용되도록 콜백 등록
        RegisterEnemyCallback(enemy);
    }

    // ⭐ 적의 착지 이벤트에 콜백 등록
    void RegisterEnemyCallback(EnemyBase enemy)
    {
        ShadowHeightEnemy shadowHeight = enemy.GetComponent<ShadowHeightEnemy>();
        if (shadowHeight != null)
        {
            // Unity Event에 등록 (착지할 때마다 실행)
            shadowHeight.onGroundHitEvent.RemoveListener(() => ReapplySlowOnLanding(enemy));
            shadowHeight.onGroundHitEvent.AddListener(() => ReapplySlowOnLanding(enemy));
        }
    }

    // ⭐ 착지 시 재적용 (이벤트 콜백)
    void ReapplySlowOnLanding(EnemyBase enemy)
    {
        if (!isActivated) return; // 스킬이 꺼져있으면 무시
        if (enemy == null || !enemy.gameObject.activeInHierarchy) return;
        
        // 착지 직후 느림 재적용
        enemy.CastSlownessToEnemy(slownessFactor);
    }

    void ReleaseSlowEffect()
    {
        // ⭐ 리스트에 있는 적들만 해제 (훨씬 빠름)
        for (int i = slowedEnemies.Count - 1; i >= 0; i--)
        {
            EnemyBase enemy = slowedEnemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                slowedEnemies.RemoveAt(i);
                continue;
            }
            
            enemy.IsSlowed = false;
            enemy.ResetCurrentSpeedToDefault();
            enemy.ResetTintColor();
            
            // ⭐ 콜백 해제
            ShadowHeightEnemy shadowHeight = enemy.GetComponent<ShadowHeightEnemy>();
            if (shadowHeight != null)
            {
                shadowHeight.onGroundHitEvent.RemoveListener(() => ReapplySlowOnLanding(enemy));
            }
        }
        
        slowedEnemies.Clear();
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