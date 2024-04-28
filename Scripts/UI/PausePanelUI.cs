using System.Collections.Generic;
using UnityEngine;

public class PausePanelUI : MonoBehaviour
{
    [SerializeField] GameObject cardSlot; // ���� ī�� ���� ������
    [SerializeField] Transform contents; // ���Ե��� ���� ���� ���̾ƿ�
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