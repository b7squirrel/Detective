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
    List<UpgradeData> selectedUpgrads;

    [SerializeField] List<UpgradeData> acquiredUpgrades;

    WeaponManager weaponManager;
    PassiveItems passiveItems;

    [SerializeField] List<UpgradeData> upgradesAvailableOnStart;

    int To_Level_Up
    {
        get
        {
            return level * 1000;
        }
    }

    void Awake()
    {
        weaponManager= GetComponent<WeaponManager>();
        passiveItems= GetComponent<PassiveItems>();
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
        selectedUpgrads.AddRange(GetUpgrades(3));
        upgradeManager.OpenPanel(selectedUpgrads);
        experience -= To_Level_Up;
        level++;
        experienceBar.SetLevelText(level);
    }
    public void Upgrade(int selectedUpgradeID)
    {
        UpgradeData upgradeData = selectedUpgrads[selectedUpgradeID];

        if (acquiredUpgrades == null) { acquiredUpgrades= new List<UpgradeData>(); }

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
        }

        acquiredUpgrades.Add(upgradeData);
        upgrades.Remove(upgradeData);
    }

    public void ShuffleUpgrades()
    {
        // 업그레이드 목록을 뒤섞고 나서 GetUpgrads에서 차례로 빼냄.
        // GetUpgrades에서 섞으면 목록이 중복될 수 있음.
        for (int i = upgrades.Count - 1; i > 0; i--)
        {
            int x = Random.Range(0, i + 1);
            UpgradeData shuffleElement = upgrades[i];
            upgrades[i] = upgrades[x];
            upgrades[x] = shuffleElement;
        }
    }

    List<UpgradeData> CheckID()
    {
        ShuffleUpgrades();

        List<int> ids = new List<int>();
        List<UpgradeData> pick = new List<UpgradeData>();
        int index = upgrades.Count - 1;
        pick.Add(upgrades[index]);
        index--;

        // id 별로 upgrades에서 count만틈 추출
        while (pick.Count < 3) // 원하는 count 갯수만큼 pick이 차면 끝냄
        {

            for (int i = pick.Count - 1; i > 0; i--)// 지금까지 골라진 pick에 같은 id가 있는지 검사
            {
                if (upgrades[index].id != pick[i].id) // 같은 id가 아니라면
                {
                    pick.Add(upgrades[index]); // pick에 추가
                }
            }

            // foreach (UpgradeData item in pick) 
            // {
            //     if (upgrades[index].id != item.id) // 같은 id가 아니라면
            //     {
            //         pick.Add(upgrades[index]); // pick에 추가
            //     }
            // }

            index--;
        }

        Debug.Log("Pick Count = " + pick.Count);
        return pick;
    }
    public List<UpgradeData> GetUpgrades(int count)
    {
        ShuffleUpgrades();

        List<UpgradeData> upgradeList = new List<UpgradeData>();

        if (count > upgrades.Count)
        {
            count = upgrades.Count;
        }

        for (int i = 0; i < count; i++)
        {
            upgradeList.Add(upgrades[i]);
        }

        return upgradeList;
    }


    internal void AddUpgradesIntoTheListOfAvailableUpgrades(List<UpgradeData> upgradesToAdd)
    {
        if (upgradesToAdd == null)
            return;

        this.upgrades.AddRange(upgradesToAdd);
    }
}
