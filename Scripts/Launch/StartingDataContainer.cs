using UnityEngine;

public class StartingDataContainer : MonoBehaviour
{
    OriAttribute leadAttr = new OriAttribute(0, 0);
    WeaponData leadWd;

    [Header("µð¹ö±ë")]
    [SerializeField] int hp = 0;
    [SerializeField] int atk = 0;

    void Awake() => DontDestroyOnLoad(this);
    public void SetLead(CardData lead, OriAttribute leadAttr)
    {
        this.leadAttr = leadAttr;
        hp = this.leadAttr.Hp;
        atk = this.leadAttr.Atk;

        leadWd = FindObjectOfType<CardsDictionary>().GetWeaponItemData(lead).weaponData;
    }
    public OriAttribute GetLeadAttr() => this.leadAttr;
    public WeaponData GetLeadWeaponData() => this.leadWd;
}
