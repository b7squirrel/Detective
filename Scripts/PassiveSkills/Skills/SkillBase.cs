using UnityEngine;

public class SkillBase : MonoBehaviour, ISkill
{
    public int Name { get; set; }
    public int Grade { get; set; }
    public int EvoStage { get; set; }
    public float CoolDownTime { get; set; }

    protected float rate = .3f; // ���, ��ų ������ ���� �󸶸�ŭ ��Ÿ�ӿ� ������ ��ġ�� �� �� ���ϴ� ����
    protected float realCoolDownTime;
    protected float skillCounter;
    protected SkillUI skillUi;

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
    /// Skill Counter�� ������Ű��, UI�� �ݿ�
    /// </summary>
    public virtual void UseSkill()
    {
        skillCounter += Time.deltaTime;
        skillUi.SetSlider(skillCounter / realCoolDownTime);
        // �� ��ų���� ����
    }
}