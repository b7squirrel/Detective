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

    public void Set(UpgradeData upgradeData)
    {
        icon.sprite = upgradeData.icon;
        if (upgradeData.description != "")
        {
            description.text = upgradeData.description;
        }
    }

    internal void Clean()
    {
        icon.sprite = null;
        
    }
}
