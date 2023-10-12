using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] Transform playerWeaponContainer;
    [SerializeField] WeaponData startingWeapon;
    [SerializeField] Transform[] essentialContainers;

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
        startingWeapon = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        AddWeapon(startingWeapon, true);
    }

    public void AddWeapon(WeaponData weaponData, bool isInitialWeapon)
    {
        container = weaponContainer.CreateContainer(weaponData, isInitialWeapon);

        GameObject weaponGameObject = Instantiate(weaponData.weaponBasePrefab, container);
        weaponGameObject.transform.position = container.position;

        WeaponBase weaponBase = weaponGameObject.GetComponent<WeaponBase>();
        weaponBase.InitialWeapon = isInitialWeapon; // 오리 탐정에게 붙는 무기인지 
        weaponBase.Init(weaponData.stats);

        // 개별 무기들 부착
        if (weaponData.weaponPrefab != null)
        {
            Transform weaponTool = Instantiate(weaponData.weaponPrefab, container.transform);
            weaponTool.position = weaponGameObject.transform.position;

            //값을 weaponFire등에서 가져갈 수 있도록 weaponBase로 옮겨놓음
            weaponBase.weaponTools = weaponTool.GetComponent<Weapon>();
            weaponBase.ShootPoint = weaponBase.weaponTools.shootPoint;
            weaponBase.EffectPoint = weaponBase.weaponTools.effectPoint;
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

        if (isInitialWeapon)
        {
            GetComponent<SyncIdleAnim>().Init(essentialContainers, container, weaponBase.transform);
        }
    }
    public void AddExtraWeaponTool(WeaponData weaponData, WeaponBase weaponBase)
    {
        // 개별 무기들 부착
        if (weaponData.weaponPrefab != null)
        {
            Transform weaponTool = Instantiate(weaponData.weaponPrefab, weaponBase.transform);
            weaponTool.position = weaponBase.transform.position;

            //값을 weaponFire등에서 가져갈 수 있도록 weaponBase로 옮겨놓음
            weaponBase.weaponToolsExtra = weaponTool.GetComponent<Weapon>();
            weaponBase.ShootPointExtra = weaponBase.weaponToolsExtra.shootPoint;
            weaponBase.EffectPointExtra = weaponBase.weaponToolsExtra.effectPoint;
            weaponBase.animExtra = weaponTool.GetComponent<Animator>();
        }
    }

    internal void UpgradeWeapon(UpgradeData upgradeData)
    {
        WeaponBase weaponUpgrade = weapons.Find(wb => wb.weaponData == upgradeData.weaponData);
        weaponUpgrade.Upgrade(upgradeData);
    }
}
