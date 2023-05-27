using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyManager : MonoBehaviour
{
    public static SynergyManager instance;

    void Awake()
    {
        instance = this;
    }
    // 시너지웨폰 키워드로 무기를 찾아 시너지웨폰 활성화
    public void ActivateSynergyWeapon(WeaponData weaponData)
    {   
         GetComponent<WeaponContainer>().SetSynergyWeaponActive(weaponData);
         Debug.Log(weaponData.Name + "이 활성화 되었습니다.");
    }
}
