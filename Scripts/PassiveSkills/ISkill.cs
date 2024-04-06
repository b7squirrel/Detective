public interface ISkill
{
    int Name { get; set; }
    float CoolDownTime { get; set; }
    void UseSkill();
}
