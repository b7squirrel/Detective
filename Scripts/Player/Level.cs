using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    int level = 1;
    int experience = 0;
    [SerializeField] ExperienceBar experienceBar;
    [SerializeField] UpgradePanelManager upgradeManager;

    [SerializeField] List<UpgradeData> upgrades;
    [SerializeField] List<UpgradeData> randomPool = new List<UpgradeData>();
    List<UpgradeData> selectedUpgrads;

    [SerializeField] List<UpgradeData> acquiredUpgrades;

    WeaponManager weaponManager;
    PassiveItems passiveItems;

    [SerializeField] List<UpgradeData> upgradesAvailableOnStart;

    [SerializeField] List<UpgradeData> instantUpgrade = new List<UpgradeData>();

    int To_Level_Up
    {
        get
        {
            return level * 1000;
        }
    }

    void Awake()
    {
        weaponManager = GetComponent<WeaponManager>();
        passiveItems = GetComponent<PassiveItems>();
    }

    void Start()
    {
        experienceBar.UpdateExperienceSlider(experience, To_Level_Up);
        experienceBar.SetLevelText(level);
        AddUpgradesIntoTheListOfAvailableUpgrades(upgradesAvailableOnStart);
    }

    public void AddExperience(int expAmount)
    {
        experience += expAmount;
        CheckLevelUp();
        experienceBar.UpdateExperienceSlider(experience, To_Level_Up);
    }

    private void CheckLevelUp()
    {
        if (experience >= To_Level_Up)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        if (selectedUpgrads == null)
        {
            selectedUpgrads = new List<UpgradeData>();
        }
        selectedUpgrads.Clear();
        selectedUpgrads.AddRange(GetRandomUpgrades());
        upgradeManager.OpenPanel(selectedUpgrads);
        experience -= To_Level_Up;
        level++;
        experienceBar.SetLevelText(level);
    }

    // 알을 통해 무기를 얻을 경우
    public void GetWeapon(UpgradeData data)
    {
        if (selectedUpgrads == null)
        {
            selectedUpgrads = new List<UpgradeData>();
        }
        selectedUpgrads.Clear();
        selectedUpgrads.Add(data);
        Upgrade(0);
    }

    // LevelUp()을 통해 무기를 얻거나 업그레이드 하는 경우
    public void Upgrade(int selectedUpgradeID)
    {
        UpgradeData upgradeData = selectedUpgrads[selectedUpgradeID];

        if (acquiredUpgrades == null) { acquiredUpgrades = new List<UpgradeData>(); }

        switch (upgradeData.upgradeType)
        {
            case UpgradeType.WeaponUpgrade:
                weaponManager.UpgradeWeapon(upgradeData);
                break;
            case UpgradeType.ItemUpgrade:
                passiveItems.UpgradeItem(upgradeData);
                break;
            case UpgradeType.WeaponGet:
                weaponManager.AddWeapon(upgradeData.weaponData, false);
                break;
            case UpgradeType.ItemGet:
                passiveItems.Equip(upgradeData.item);
                AddUpgradesIntoTheListOfAvailableUpgrades(upgradeData.item.upgrades);
                break;
            case UpgradeType.Heal:
                GetComponent<Character>().Heal(upgradeData.itemStats.hp);
                break;
            case UpgradeType.Coin:
                GetComponent<Coins>().Add(upgradeData.itemStats.coins);
                break;

        }

        acquiredUpgrades.Add(upgradeData);
        upgrades.Remove(upgradeData);
    }

    void ShuffleRandomPool(List<UpgradeData> randomPool)
    {
        // 업그레이드 목록을 뒤섞고 나서 GetUpgrads에서 차례로 빼냄.
        // GetUpgrades에서 섞으면 목록이 중복될 수 있음.
        for (int i = randomPool.Count - 1; i > 0; i--)
        {
            int x = Random.Range(0, i + 1);
            UpgradeData shuffleElement = randomPool[i];
            randomPool[i] = randomPool[x];
            randomPool[x] = shuffleElement;
        }
    }

    List<UpgradeData> GetRandomUpgrades()
    {
        randomPool.Clear();
        List<UpgradeData> upgradeList = new List<UpgradeData>();

        for (int i = 0; i < upgrades.Count; i++)
        {
            randomPool.Add(upgrades[i]);
        }

        ShuffleRandomPool(randomPool);

        for (int index = 0; index < randomPool.Count; index++)
        {
            upgradeList.Add(randomPool[index]);

            for (int i = randomPool.Count - 1; i > index; i--)
            {
                if (randomPool[i].id == randomPool[index].id)
                {
                    randomPool.Remove(randomPool[i]);
                }
            }

            if(upgradeList.Count == 3) return upgradeList;
        }
        
        // 부족한 슬롯만큼 달콤우유나 동전을 추가
        List<UpgradeData> lacks = new List<UpgradeData>();
        for (int i = 0; i < 3 - randomPool.Count; i++)
        {
            lacks.Add(instantUpgrade[Random.Range(0, instantUpgrade.Count)]);
        }
        upgradeList.AddRange(lacks);

        return upgradeList;
    }

    public bool HavingWeapon(UpgradeData item)
    {
        for (int i = acquiredUpgrades.Count - 1; i > 0; i--)
        {
            if(acquiredUpgrades[i].id == item.id)
            return true;
        }
        return false;
    }

    internal void AddUpgradesIntoTheListOfAvailableUpgrades(List<UpgradeData> upgradesToAdd)
    {
        if (upgradesToAdd == null)
            return;

        this.upgrades.AddRange(upgradesToAdd);
    }
}
