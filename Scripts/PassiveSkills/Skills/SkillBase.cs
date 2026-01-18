using UnityEngine;

public abstract class SkillBase : MonoBehaviour, ISkill
{
    public abstract SkillType SkillType { get; }  // 👈 추상 프로퍼티로 변경
    public int Grade { get; set; }
    public int EvoStage { get; set; }
    
    protected SkillData skillData;
    protected float realCoolDownTime;
    protected float skillCounter;
    protected SkillUI skillUi;
    protected bool isActivated;
    
    protected float rate = .3f;

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
}