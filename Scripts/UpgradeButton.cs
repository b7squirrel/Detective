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

    void Awake()
    {
        weaponContainer = Player.instance.GetComponent<WeaponContainer>();

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
        if(upgradeData.upgradeType == UpgradeType.WeaponUpgrade)
        {
            SetLevelStarAlpha(weaponContainer.GetWeaponLevel(upgradeData.weaponData));
            Debug.Log(upgradeData.Name + "현재레벨 = " + weaponContainer.GetWeaponLevel(upgradeData.weaponData));
        }
    }

    internal void Clean()
    {
        icon.sprite = null;
        ClearLevelstars();
        levelBar.SetActive(false);
        
    }

    void SetLevelStarAlpha(int level)
    {
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
