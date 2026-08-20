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
    UpgradeData upgradeToRemove;

    public void AddSynergyUpgradeToPool(WeaponData weaponData)
    {
        if(synergyUpgrades == null)
        {
            synergyUpgrades = new List<UpgradeData>();
        }
        if(weaponData.synergyUpgrade == null)
    {
        // Logger.LogError($"{weaponData.name}의 synergyUpgrade가 null입니다!");
        return;
    }

        synergyUpgrades.Add(weaponData.synergyUpgrade);
        // Logger.LogError($"시너지 추가됨: {weaponData.synergyUpgrade.name}, 현재 풀 크기: {synergyUpgrades.Count}");
    }

    public UpgradeData GetSynergyUpgrade()
    {
        if (synergyUpgrades == null || synergyUpgrades.Count == 0) return null;

        // 풀을 순회하며 아직 활성화되지 않은 첫 번째 시너지를 반환
        // (랜덤이 아니라 확정적으로 찾음 — 유효한 후보가 있다면 반드시 걸림)
        for (int i = 0; i < synergyUpgrades.Count; i++)
        {
            UpgradeData candidate = synergyUpgrades[i];
            if (!GetComponent<WeaponContainer>().CheckSynergyWeaponActivated(candidate))
            {
                upgradeToRemove = candidate;
                return candidate;
            }
        }

        // 풀에 있는 모든 항목이 이미 활성화된 경우에만 null
        upgradeToRemove = null;
        return null;
    }

    // 시너지웨폰 키워드로 무기를 찾아 시너지웨폰 활성화
    public void ActivateSynergyWeapon(UpgradeData upgradeData)
    {
        GetComponent<WeaponContainer>().SetSynergyWeaponActive(upgradeData.weaponData);
        //  Debug.Log(upgradeData.weaponData.SynergyWeapon + "이 활성화 되었습니다.");

        synergyUpgrades.Remove(upgradeData); // 리스트에서 시너지 업그레이드 제거
    }
}
