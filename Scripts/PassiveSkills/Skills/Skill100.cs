using UnityEngine;

public class Skill100 : MonoBehaviour, ISkill
{
    public int Name { get; set; } = 100;
    public float CoolDownTime { get; set; } = 2f;
    public void UseSkill()
    {
        Debug.Log("Skill 100");
    }
}
