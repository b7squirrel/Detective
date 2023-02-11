using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUpgradeUIElement : MonoBehaviour
{
    [SerializeField] PlayerPersistentUpgrades upgrades;
    [SerializeField] TextMeshProUGUI upgradeName;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] TextMeshProUGUI price;
    [SerializeField] DataContainer dataContainer;

    void Start()
    {
        UpdateElement();
    }

    public void Upgrade()
    {
        PlayerUpgrades playerUpgrades = dataContainer.upgrades[(int)upgrades];

        if (playerUpgrades.level >= playerUpgrades.max_level)
            return;

        if (dataContainer.coins >= playerUpgrades.costToUpgrades)
        {
            dataContainer.coins -= playerUpgrades.costToUpgrades;
            playerUpgrades.level += 1;
            UpdateElement();
        }
    }

    void UpdateElement()
    {
        PlayerUpgrades playerUpgrade = dataContainer.upgrades[(int)upgrades];
        upgradeName.text = upgrades.ToString();
        level.text = playerUpgrade.level.ToString();
        price.text = playerUpgrade.costToUpgrades.ToString();
    }
}
