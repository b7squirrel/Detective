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

    void Start()
    {
        startingWeapon = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        AddWeapon(startingWeapon, true);
    }

    public void AddWeapon(WeaponData weaponData, bool isInitialWeapon)
    {
        WeaponData wd = null;
        List<Item> item = new();
        if(isInitialWeapon)
        {
            wd = GameManager.instance.startingDataContainer.GetLeadWeaponData();
            container = weaponContainer.CreateContainer(wd, isInitialWeapon);

            // Pause Panel에서 플레이어의 장비 상태를 보여줌
            GameManager.instance.GetComponent<PausePanel>().InitWeaponSlot(wd, true);

            item = GameManager.instance.startingDataContainer.GetItemDatas();

        }
        else
        {
            wd = weaponData;
            container = weaponContainer.CreateContainer(weaponData, isInitialWeapon);
            // Logger.LogError($"[WeaponManager] {wd.DisplayName} 컨테이너를 생성합니다.");

            // Pause Panel에서 동료 오리들의 장비 상태를 보여줌
            GameManager.instance.GetComponent<PausePanel>().InitWeaponSlot(wd, false);
        }

        // WeaponBase Prefab - 특정 오리 (Punch, Tesla..)
        // Weapon Prefab - 특정 오리에 붙는 무기 (Rifle, Tesla Tower...)
        GameObject weaponGameObject = Instantiate(weaponData.weaponBasePrefab, container);
        weaponGameObject.transform.position = container.position;

        WeaponContainerAnim wa = container.GetComponent<WeaponContainerAnim>();

        WeaponBase weaponBase = weaponGameObject.GetComponent<WeaponBase>();

        if (weaponData.weaponPrefab == null)
        {
            wa.ParentWeaponObjectTo((int)wd.equipmentType, weaponGameObject.transform, weaponBase.NeedParent, 1);
        }

        weaponBase.InitialWeapon = isInitialWeapon; // Player 리드 오리에게 붙는 무기인지 
        weaponBase.Init(weaponData.stats, isInitialWeapon);

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
            Sprite sprite = null;
            int _index = (int)wd.equipmentType;

            if (wd.defaultItems != null &&
                                    _index < wd.defaultItems.Length &&
                                    wd.defaultItems[_index] != null &&
                                    wd.defaultItems[_index].spriteRow != null &&
                                    wd.defaultItems[_index].spriteRow.sprites != null &&
                                    wd.defaultItems[_index].spriteRow.sprites.Length > 0)
            {
                sprite = wd.defaultItems[_index].spriteRow.sprites[0];
            }

            // 무기에 스프라이트 주입
            wa.SetWeaponToolSpriteRenderer(weaponTool.GetComponentInChildren<SpriteRenderer>(), sprite);
            // Head, Chest, Face, Hand 순서 EquipmentType
            wa.ParentWeaponObjectTo((int)wd.equipmentType, weaponTool, weaponBase.NeedParent, 2);
        }

        weaponBase.SetData(weaponData);

        weapons.Add(weaponBase);

        weaponBase.AddOwnerCharacter(character);

        Level level = GetComponent<Level>();
        if (level != null)
        {
            level.AddUpgradesIntoTheListOfAvailableUpgrades(weaponData.upgrades);
            // Logger.LogError($"[WeaponManager] {weaponData.DisplayName}를 upgrades 리스트에 추가합니다.");
        }
    }

    // _index는 몇 번째 무기인지 알려줌
    public void AddExtraWeaponTool(WeaponData weaponData, WeaponBase weaponBase, int _index)
    {
        // 개별 무기들 부착
        if (weaponData.weaponPrefab != null)
        {
            Transform weaponTool = Instantiate(weaponData.weaponPrefab, weaponBase.GetComponentInParent<WeaponContainerAnim>().transform);
            weaponTool.position = weaponBase.transform.position;

            //값을 weaponFire등에서 가져갈 수 있도록 weaponBase로 옮겨놓음
            weaponBase.weaponToolsExtra = weaponTool.GetComponent<Weapon>();
            weaponBase.ShootPointExtra = weaponBase.weaponToolsExtra.shootPoint;
            weaponBase.EffectPointExtra = weaponBase.weaponToolsExtra.effectPoint;
            weaponBase.animExtra = weaponTool.GetComponent<Animator>();

            WeaponContainerAnim wa = container.GetComponent<WeaponContainerAnim>();
            wa.ParentWeaponObjectTo((int)weaponData.equipmentType, weaponTool.transform, weaponBase.NeedParent, 3);
            wa.SetExtraWeaponToolSpriteRenderer(weaponTool.GetComponentInChildren<SpriteRenderer>());
        }
    }

    internal void UpgradeWeapon(UpgradeData upgradeData)
    {
        // weapon data를 비교하지 않고 weapon data의 이름을 비교
        // 등급이 높은 오리를 리드로 했을 때도 업그레이드가 잘 될 수 있도록
        WeaponBase weaponUpgrade = weapons.Find(wb => wb.weaponData.Name == upgradeData.weaponData.Name);
        weaponUpgrade.Upgrade(upgradeData);

        WeaponData wd = weaponUpgrade.weaponData;
        string weaponName = wd.Name;
        int level = weaponContainer.GetWeaponLevel(upgradeData.weaponData);

        bool isSynergy = upgradeData.upgradeType == UpgradeType.SynergyUpgrade ? true : false;
        GameManager.instance.GetComponent<PausePanel>().UpdateWeaponLevel(weaponName, level, isSynergy);
    }

    public List<WeaponBase> GetAllWeapons()
    {
        if (weapons == null) return new List<WeaponBase>();
        return weapons;
    }
}
