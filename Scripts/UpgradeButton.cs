using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMPro.TextMeshProUGUI description;
    [SerializeField] GameObject levelBar;
    [SerializeField] List<Image> levelOn;
    [SerializeField] List<Image> levelOff;
    WeaponContainer weaponContainer;
    PassiveItems passiveItems;

    void Awake()
    {
        weaponContainer = Player.instance.GetComponent<WeaponContainer>();
        passiveItems = Player.instance.GetComponent<PassiveItems>();

        ClearLevelstars();
        levelBar.SetActive(false);
    }

    public void Set(UpgradeData upgradeData)
    {
        icon.sprite = upgradeData.icon;
        if (upgradeData.description != "")
        {
            description.text = upgradeData.description;
        }

        if (upgradeData.upgradeType == UpgradeType.SynergyUpgrade)
            return;

        levelBar.SetActive(true);
        if(upgradeData.upgradeType == UpgradeType.WeaponUpgrade) // 무기 업그레이드일 경우
        {
            SetLevelStarAlpha(weaponContainer.GetWeaponLevel(upgradeData.weaponData), 5);
        }
        else if(upgradeData.upgradeType == UpgradeType.ItemUpgrade || upgradeData.upgradeType == UpgradeType.ItemGet)
        {
            SetLevelStarAlpha(passiveItems.GetItemLevel(upgradeData.item), 3);
        }
        else if(upgradeData.upgradeType == UpgradeType.SynergyUpgrade)
        {
            levelBar.SetActive(false);
        }
    }

    internal void Clean()
    {
        icon.sprite = null;
        ClearLevelstars();
        levelBar.SetActive(false);
    }

    void SetLevelStarAlpha(int level, int baseNumbers)
    {
        for (int i = 0; i < baseNumbers; i++)
        {
            levelOff[i].color = new Color(levelOff[i].color.r, levelOff[i].color.g, levelOff[i].color.b, 1f);
        }
        for (int i = 0; i < level + 1; i++)
        {
            levelOn[i].color = new Color(levelOn[i].color.r, levelOn[i].color.g, levelOn[i].color.b, 1f);
        }
    }

    void ClearLevelstars()
    {
        foreach (var item in levelOn)
        {
            item.color = new Color(item.color.r, item.color.g, item.color.b, 0f);
        }
        foreach (var item in levelOff)
        {
            item.color = new Color(item.color.r, item.color.g, item.color.b, 0f);
        }
    }
    
}
