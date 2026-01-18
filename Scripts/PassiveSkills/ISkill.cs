public interface ISkill
{
    SkillType SkillType { get; }  // 👈 get만 남김
    int Grade { get; set; }
    int EvoStage { get; set; }
    void UseSkill();
    void Init(SkillManager skillManager, CardData cardData, SkillData data);
    bool IsActivated();
}