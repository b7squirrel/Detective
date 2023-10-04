using UnityEngine;

public class StartingDataContainer : MonoBehaviour
{
    OriAttribute leadAttr = new OriAttribute(0, 0);
    WeaponData leadWd;
    RuntimeAnimatorController[] equipmentsAnims = new RuntimeAnimatorController[4];

    [Header("debugging")]
    [SerializeField] int hp = 0;
    [SerializeField] int atk = 0;
    [SerializeField] RuntimeAnimatorController[] equipAnims = new RuntimeAnimatorController[4];

    void Awake() => DontDestroyOnLoad(this);
    public void SetLead(CardData lead, OriAttribute leadAttr, Animator[] equipAnims)
    {
        this.leadAttr = leadAttr;
        // debugging
        hp = this.leadAttr.Hp;
        atk = this.leadAttr.Atk;
        //Debug.Log(equipAnims.Length);
        //for (int i = 0; i < 4; i++)
        //{
        //    equipmentsAnims[i] = equipAnims[i].runtimeAnimatorController;
        //}

        //leadWd = FindObjectOfType<CardsDictionary>().GetWeaponItemData(lead).weaponData;
        //for (int i = 0; i < 4; i++)
        //{
        //    if (equipAnims.Length == 0) return;
        //    if (equipAnims[i] == null)
        //    {
        //        equipmentsAnims[i] = null;
        //    }
        //    else
        //    {
        //        equipmentsAnims[i] = equipAnims[i].runtimeAnimatorController;
        //    }
        //}
    }

    // Player loads the following information after starting the game
    public OriAttribute GetLeadAttr() => this.leadAttr;
    public WeaponData GetLeadWeaponData() => this.leadWd;
    public RuntimeAnimatorController[] GetEquipRuntimeAnims() => this.equipmentsAnims;
}
