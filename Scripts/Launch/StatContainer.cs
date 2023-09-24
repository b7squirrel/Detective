using UnityEngine;

public class StatContainer : MonoBehaviour
{
    OriAttribute leadAttr = new OriAttribute(0, 0);

    [Header("µð¹ö±ë")]
    [SerializeField] int hp = 0;
    [SerializeField] int atk = 0;

    void Awake() => DontDestroyOnLoad(this);
    public void SetLeadAttr(OriAttribute leadAttr)
    {
        this.leadAttr = leadAttr;
        hp = this.leadAttr.Hp;
        atk = this.leadAttr.Atk;
    }
    public OriAttribute GetLeadAttr() => this.leadAttr;
}
