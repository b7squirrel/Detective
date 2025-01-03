using UnityEngine;

public class SkillBase : MonoBehaviour, ISkill
{
    public int Name { get; set; }
    public int Grade { get; set; }
    public int EvoStage { get; set; }
    public float CoolDownTime { get; set; }

    protected float rate = .3f; // 등급, 스킬 레벨에 따라 얼마만큼 쿨타임에 영향을 미치게 할 지 정하는 비율
    protected float realCoolDownTime;
    protected float skillCounter;
    protected SkillUI skillUi;

    protected bool isActivated; // 외부에서 어떤 스킬이 발동되었는지 알기 위해

    protected bool isHitAnimPlayed;
    protected bool isDurationAnimPlaying;

    public virtual void Init(SkillManager _skillManager, CardData _cardData)
    {
        _skillManager.onSkill += UseSkill;

        Grade = _cardData.Grade;
        EvoStage = _cardData.EvoStage;
        realCoolDownTime = new Equation().GetCoolDownTime(rate, Grade, EvoStage, CoolDownTime);

        skillUi = FindObjectOfType<SkillUI>();
        int skillIndex = ((Name / 100) % 10) - 1;
        string skillName = Skills.SkillNames[skillIndex];
        skillUi.Init(skillName, EvoStage, realCoolDownTime);
    }
    /// <summary>
    /// Skill Counter를 증가시키고, UI에 반영
    /// </summary>
    public virtual void UseSkill()
    {
        skillCounter += Time.deltaTime;
        skillUi.SetSlider(skillCounter / realCoolDownTime);
        // 각 스킬에서 구현
    }

    public bool IsActivated()
    {
        return isActivated;
    }

    //protected void UpdateUI(float _durationTimer, float _realDuration)
    //{
    //    if (skillCounter > realCoolDownTime)
    //    {
    //        if (isHitAnimPlayed == false)
    //        {
    //            skillUi.BadgeUpAnim();
    //            isHitAnimPlayed = true;
    //        }

    //        // 스킬 발동 시간 모두 종료
    //        if (_durationTimer > _realDuration)
    //        {
    //            skillUi.PlayBadgeAnim("Done");
    //            isHitAnimPlayed = false;

    //            return;
    //        }
    //        else
    //        {
    //            // 스킬 계속 유지
    //            if (isDurationAnimPlaying == false)
    //            {
    //                skillUi.PlayBadgeAnim("Duration");
    //                isDurationAnimPlaying = true;
    //            }

    //            return;
    //        }

    //    }
    //}
}