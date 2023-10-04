using System.Collections.Generic;
using UnityEngine;

public class StartingDataContainer : MonoBehaviour
{
    OriAttribute leadAttr = new OriAttribute(0, 0);
    WeaponData leadWd;
    [SerializeField] Animator[] equipmentsAnims;

    [Header("Debugging")]
    [SerializeField] int hp = 0;
    [SerializeField] int atk = 0;
    [SerializeField] List<RuntimeAnimatorController> equipAnimators = new();

    void Awake() => DontDestroyOnLoad(this);
    public void SetLead(CardData lead, OriAttribute leadAttr, List<RuntimeAnimatorController> equipAnims)
    {
        this.leadAttr = leadAttr;
        // debugging
        hp = this.leadAttr.Hp;
        atk = this.leadAttr.Atk;
        for (int i = 0; i < 4; i++)
        {
            equipmentsAnims[i].runtimeAnimatorController = equipAnims[i];
            equipAnimators.Add(equipAnims[i]);
        }

        leadWd = FindObjectOfType<CardsDictionary>().GetWeaponItemData(lead).weaponData;
        
    }

    // Player loads the following information after starting the game
    public OriAttribute GetLeadAttr() => this.leadAttr;
    public WeaponData GetLeadWeaponData() => this.leadWd;
    //public RuntimeAnimatorController[] GetEquipRuntimeAnims() => this.equipmentsAnims;
}
