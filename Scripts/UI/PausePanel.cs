using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// weapon manager���� ���⸦ �߰��� �� ȣ���ؼ� �ʱ�ȭ 
/// ���׷��̵� �� ���� ȣ���ؼ� ������Ʈ
/// </summary>
public class PausePanel : MonoBehaviour
{
    [SerializeField] GameObject cardSlot; // ���� ī�� ���� ������
    [SerializeField] Transform weaponContents; // ���Ե��� ���� ���� ���̾ƿ�
    [SerializeField] Transform itemContents; // ���Ե��� ���� ���� ���̾ƿ�
    List<WeaponData> weaponDatas;

    public void InitWeaponSlot(WeaponData wd)
    {
        if (weaponDatas == null) weaponDatas = new();
        weaponDatas.Add(wd);
        GameObject wSlot = Instantiate(cardSlot, weaponContents.transform);
        wSlot.GetComponent<PauseCardDisp>().InitWeaponCardDisplay(wd);
        Debug.Log($"{wd.Name} is added.");
    }
    public void InitLeadWeaponSlot(WeaponData wd)
    {

    }
    public void UpdateWeaponLevel(WeaponData wd)
    {
        // wd.weaponStats.currentLevel�� �޾Ƽ� Pause Card Disp�� �Ѱ��ֱ�
        Debug.Log($"{wd.Name} Level = {wd.stats.currentLevel}");
    }
}