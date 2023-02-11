using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] Transform playerWeaponContainer;
    [SerializeField] WeaponData startingWeapon;

    Character character;

    List<WeaponBase> weapons;
    WeaponContainer weaponContainer;
    Transform container;

    void Awake()
    {
        weapons = new List<WeaponBase>();
        weaponContainer = GetComponent<WeaponContainer>();
        character = GetComponent<Character>();
    }

    private void Start()
    {
        AddWeapon(startingWeapon, true);
    }

    public void AddWeapon(WeaponData weaponData, bool isInitialWeapon)
    {
        container = weaponContainer.GetContainer(weaponData, isInitialWeapon);

        GameObject weaponGameObject = Instantiate(weaponData.weaponBasePrefab, container);
        weaponGameObject.transform.position = container.position;

        WeaponBase weaponBase = weaponGameObject.GetComponent<WeaponBase>();
        weaponBase.InitialWeapon = isInitialWeapon; // 오리 탐정에게 붙는 무기인지 

        // 개별 무기들 부착
        if (weaponData.weaponPrefab != null)
        {
            Transform weaponTool = Instantiate(weaponData.weaponPrefab, container.transform);
            weaponTool.position = weaponGameObject.transform.position;

            //값을 weaponFire등에서 가져갈 수 있도록 weaponBase로 옮겨놓음
            weaponBase.weaponTools = weaponTool.GetComponent<Weapon>();
            weaponBase.ShootPoint = weaponTool.GetComponent<Weapon>().shootPoint;
            weaponBase.anim = weaponTool.GetComponent<Animator>();
        }

        weaponBase.SetData(weaponData);

        weapons.Add(weaponBase);

        weaponBase.AddOwnerCharacter(character);

        Level level = GetComponent<Level>();
        if (level != null)
        {
            level.AddUpgradesIntoTheListOfAvailableUpgrades(weaponData.upgrades);
        }
    }

    internal void UpgradeWeapon(UpgradeData upgradeData)
    {
        WeaponBase weaponUpgrade = weapons.Find(wd => wd.weaponData == upgradeData.weaponData);
        weaponUpgrade.Upgrade(upgradeData);
    }
}
