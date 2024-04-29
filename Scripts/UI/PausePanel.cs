using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// weapon manager에서 무기를 추가할 떄 호출해서 초기화.
/// 무기를 업그레이드 할 때도 호출해서 업데이트.
/// </summary>
public class PausePanel : MonoBehaviour
{
    [SerializeField] GameObject cardSlot; // 오리 카드 슬롯 프리펩
    [SerializeField] GameObject itemSlot; // 아이템 카드 슬롯 프리펩
    [SerializeField] Transform weaponContents; // 무기 슬롯들을 집어넣을 레이아웃
    [SerializeField] Transform itemContents; // 아이템 슬롯들을 집어넣을 레이아웃
    List<PauseCardDisp> weaponCards;
    List<PauseCardDisp> itemCards;

    public void InitWeaponSlot(WeaponData wd, bool isLead)
    {
        if (weaponCards == null) weaponCards = new();

        GameObject wSlot = Instantiate(cardSlot, weaponContents.transform);
        weaponCards.Add(wSlot.GetComponent<PauseCardDisp>());

        if(isLead)
        {
            wSlot.GetComponent<PauseCardDisp>().InitLeadWeaponCardDisplay(wd);
        }
        else
        {
            wSlot.GetComponent<PauseCardDisp>().InitWeaponCardDisplay(wd);
        }
    }
    public void InitItemSlot(Item _item)
    {
        if (itemCards == null) itemCards = new();

        GameObject iSlot = Instantiate(itemSlot, itemContents.transform);
        itemCards.Add(iSlot.GetComponent<PauseCardDisp>());

        iSlot.GetComponent<PauseCardDisp>().InitItemCardDisplay(_item);
    }
    public void UpdateWeaponLevel(string _weaponName, int _levelToUpdate, bool _isSynergy)
    {
        for (int i = 0; i < weaponCards.Count; i++)
        {
            if (weaponCards[i].Name == _weaponName)
            {
                weaponCards[i].UpdatePauseCardLevel(_levelToUpdate, true, _isSynergy);
                return;
            }
        }

    }
    public void UpdateItemLevel(Item _item)
    {
        for (int i = 0; i < itemCards.Count; i++)
        {
            if (itemCards[i].Name == _item.Name)
            {
                itemCards[i].UpdatePauseCardLevel(_item.stats.currentLevel,false, false);
                return;
            }
        }
    }
}