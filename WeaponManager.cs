using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] Transform playerWeaponContainer;
    [SerializeField] WeaponData startingWeapon;
    [SerializeField] Transform faceGroupToFollow;

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
        WeaponData wd = null;
        if(isInitialWeapon)
        {
            wd = GameManager.instance.startingDataContainer.GetLeadWeaponData();
            container = weaponContainer.CreateContainer(wd, isInitialWeapon);
        }
        else
        {
            wd = weaponData;
            container = weaponContainer.CreateContainer(weaponData, isInitialWeapon);
        }

        // WeaponBase Prefab - 특정 오리 (Punch, Tesla..)
        // Weapon Prefab - 특정 오리에 붙는 무기 (Rifle, Tesla Tower...)
        GameObject weaponGameObject = Instantiate(weaponData.weaponBasePrefab, container);
        weaponGameObject.transform.position = container.position;

        WeaponContainerAnim wa = container.GetComponent<WeaponContainerAnim>();
        wa.ParentWeaponObjectTo((int)wd.equipmentType, weaponGameObject.transform); // Head, Chest, Face, Hand 순서 EquipmentType

        WeaponBase weaponBase = weaponGameObject.GetComponent<WeaponBase>();
        //weaponBase.InitialWeapon = isInitialWeapon; // Player 리드 오리에게 붙는 무기인지 
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

            // 시너지 등으로 무기가 추가된다면 weaponManager.AddExtraWeaponTool에서 추가
            wa.SetWeaponToolSpriteRenderer(weaponTool.GetComponentInChildren<SpriteRenderer>(), 0);
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

            WeaponContainerAnim wa = container.GetComponent<WeaponContainerAnim>();
            wa.ParentWeaponObjectTo((int)wd.equipmentType, weaponTool.transform);
            wa.SetWeaponToolSpriteRenderer(weaponTool.GetComponentInChildren<SpriteRenderer>(), 1);
        }
    }

    internal void UpgradeWeapon(UpgradeData upgradeData)
    {
        WeaponBase weaponUpgrade = weapons.Find(wb => wb.weaponData == upgradeData.weaponData);
        weaponUpgrade.Upgrade(upgradeData);
    }
}
