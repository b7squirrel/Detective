using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// weapon manager에서 무기를 추가할 떄 호출해서 초기화.
/// 무기를 업그레이드 할 때도 호출해서 업데이트.
/// </summary>
public class PausePanel : MonoBehaviour
{
    [SerializeField] GameObject cardSlot; // 오리 카드 슬롯 프리펩
    [SerializeField] Transform weaponContents; // 무기 슬롯들을 집어넣을 레이아웃
    [SerializeField] Transform itemContents; // 아이템 슬롯들을 집어넣을 레이아웃
    List<WeaponData> weaponDatas;

    public void InitWeaponSlot(WeaponData wd, bool isLead)
    {
        if (weaponDatas == null) weaponDatas = new();
        weaponDatas.Add(wd);
        GameObject wSlot = Instantiate(cardSlot, weaponContents.transform);
        if(isLead)
        {
            wSlot.GetComponent<PauseCardDisp>().InitLeadWeaponCardDisplay(wd);
        }
        else
        {
            wSlot.GetComponent<PauseCardDisp>().InitWeaponCardDisplay(wd);
        }
    }
    public void InitLeadWeaponSlot(WeaponData wd)
    {
        
    }
    public void UpdateWeaponLevel(WeaponData wd)
    {
        // wd.weaponStats.currentLevel을 받아서 Pause Card Disp에 넘겨주기
        Debug.Log($"{wd.Name} Level = {wd.stats.currentLevel}");
    }
}