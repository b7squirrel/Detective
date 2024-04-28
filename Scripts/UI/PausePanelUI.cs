using System.Collections.Generic;
using UnityEngine;

public class PausePanelUI : MonoBehaviour
{
    [SerializeField] GameObject cardSlot; // 오리 카드 슬롯 프리펩
    [SerializeField] Transform contents; // 슬롯들을 집어 넣을 레이아웃
    List<WeaponData> weaponDatas;

    public void InitWeaponSlot(WeaponData wd)
    {
        weaponDatas.Add(wd);
        GameObject wSlot = Instantiate(cardSlot, contents.transform);
        wSlot.GetComponent <PauseCardDisp>().InitWeaponCardDisplay(wd);
    }
    public void UpdateWeaponLevel(WeaponData wd)
    {

    }

}