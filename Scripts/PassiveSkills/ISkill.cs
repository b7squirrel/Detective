public interface ISkill
{
    string Name { get; set; }
    float CoolDownTime { get; set; }
    void UseSkill();
}
