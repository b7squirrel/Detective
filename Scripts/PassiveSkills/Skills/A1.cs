using UnityEngine;

public class A1 : MonoBehaviour, ISkill
{
    public string Name { get; set; } = "A1";
    public float CoolDownTime { get; set; } = 2f;
    public void UseSkill()
    {
        Debug.Log("Skill A1");
    }
}
