public interface ISkill
{
    int Name { get; set; }
    int Grade { get; set; }
    int EvoStage { get; set; }
    float CoolDownTime { get; set; }
    void UseSkill();
    void Init(SkillManager _skillManager, CardData _cardData);
    bool IsActivated();
}
