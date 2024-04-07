public interface ISkill
{
    int Name { get; set; }
    int Grade { get; set; }
    int EvoStage { get; set; }
    float CoolDownTime { get; set; }
    void UseSkill();
}
