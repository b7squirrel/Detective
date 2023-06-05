using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//시너지 매니져
//시너지 업그레이드 리스트
//AddSynergyList 넘겨받은 업그레이드를 업그레이드 리스트에 추가하는 함수
//GetSynergyUpgrade 업그레이드 리스트 중 하나를 반환하는 함수
//반환하면 그 목록은 리스트에서 제거
public class SynergyManager : MonoBehaviour
{
    [SerializeField] List<UpgradeData> synergyUpgrades;

    public void AddSynergyUpgradeToPool(WeaponData weaponData)
    {
        if(synergyUpgrades == null)
        {
            synergyUpgrades = new List<UpgradeData>();
        }

        synergyUpgrades.Add(weaponData.synergyUpgrade);
    }

    public UpgradeData GetSynergyUpgrade()
    {
        if (synergyUpgrades.Count == 0) return null;

        int index = Random.Range(0, synergyUpgrades.Count);
        UpgradeData pickedUpgrade = synergyUpgrades[index];
        if (GetComponent<WeaponContainer>().CheckSynergyWeaponActivated(pickedUpgrade))
            return null;
        return pickedUpgrade;
    }

    // 시너지웨폰 키워드로 무기를 찾아 시너지웨폰 활성화
    public void ActivateSynergyWeapon(UpgradeData upgradeData)
    {
        GetComponent<WeaponContainer>().SetSynergyWeaponActive(upgradeData.weaponData);
         Debug.Log(upgradeData.weaponData.SynergyWeapon + "이 활성화 되었습니다.");
    }
}
