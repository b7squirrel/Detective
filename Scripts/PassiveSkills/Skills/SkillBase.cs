using UnityEngine;

public abstract class SkillBase : MonoBehaviour, ISkill
{
    public abstract SkillType SkillType { get; }
    public int Grade { get; set; }
    public int EvoStage { get; set; }
    
    protected SkillData skillData;
    protected float realCoolDownTime;
    protected float skillCounter;
    protected SkillUI skillUi;
    protected bool isActivated;
    protected float rate = .3f;

    // ⭐ 지속시간 업그레이드 레벨
    protected int durationUpgradeLevel = 0;

    public virtual void Init(SkillManager skillManager, CardData cardData, SkillData data)
    {
        skillManager.onSkill += UseSkill;
        skillData = data;
        Grade = cardData.Grade;
        EvoStage = cardData.EvoStage;
        
        realCoolDownTime = new Equation().GetCoolDownTime(
            rate, Grade, EvoStage, data.baseCooldown);
        
        skillUi = FindObjectOfType<SkillUI>();
        skillUi.Init(data, EvoStage, realCoolDownTime);
    }

    public virtual void UseSkill()
    {
        skillCounter += Time.deltaTime;
        skillUi.SetSlider(skillCounter / realCoolDownTime);
    }

    public bool IsActivated()
    {
        return isActivated;
    }

    // ⭐ 지속시간 업그레이드 메서드 (각 스킬에서 오버라이드)
    public virtual void ApplyDurationUpgrade(int level)
    {
        durationUpgradeLevel = level;
        Debug.Log($"[{SkillType}] 지속시간 업그레이드 LV{level} 적용");
    }

    // ⭐ 현재 업그레이드 레벨 반환
    public int GetDurationUpgradeLevel()
    {
        return durationUpgradeLevel;
    }
}