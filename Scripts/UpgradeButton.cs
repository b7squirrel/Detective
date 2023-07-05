using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMPro.TextMeshProUGUI upgradeName;
    [SerializeField] TMPro.TextMeshProUGUI description; // 설명 첫 번째 줄
    [SerializeField] TMPro.TextMeshProUGUI description2; // 설명 두 번째 줄
    [SerializeField] GameObject levelBar; // 무기, 아이템 레벨 별
    [SerializeField] GameObject panel_weapon; // 무기 패널 연두색
    [SerializeField] GameObject panel_item; // 아이템 패널, 파란색
    [SerializeField] GameObject panel_synergy; // 시너지 패널, 보라색
    [SerializeField] GameObject panel_instant_items; // 동전, 도넛 패널, 노란색
    [SerializeField] GameObject[] stars; // 활성, 비활성 시킬 별
    [SerializeField] GameObject unSelectionPanel; // 선택되지 않으면 카드를 어둡게 하는 역할
    [SerializeField] List<Image> levelOn;
    [SerializeField] List<Image> levelOff;
    WeaponContainer weaponContainer;
    PassiveItems passiveItems;

    Animator anim;

    void Awake()
    {
        weaponContainer = Player.instance.GetComponent<WeaponContainer>();
        passiveItems = Player.instance.GetComponent<PassiveItems>();

        ClearLevelstars();
        levelBar.SetActive(false);
        unSelectionPanel.SetActive(false);

        anim = GetComponent<Animator>();
    }

    public void Set(UpgradeData upgradeData)
    {
        anim.SetTrigger("Reset");

        icon.sprite = upgradeData.icon;
        if (upgradeData.Name != "")
        {
            upgradeName.text = upgradeData.Name;
        }

        description.text = upgradeData.description;
        description2.text = upgradeData.description2;

        levelBar.SetActive(true);
        if(upgradeData.upgradeType == UpgradeType.WeaponUpgrade) // 무기 업그레이드일 경우
        {
            // 별 5개, 연두 패널
            SetLevelStarAlpha(weaponContainer.GetWeaponLevel(upgradeData.weaponData), 5);
            panel_item.SetActive(false);
            panel_synergy.SetActive(false);
            panel_weapon.SetActive(true);
            panel_instant_items.SetActive(false);
        }
        else if(upgradeData.upgradeType == UpgradeType.ItemUpgrade || upgradeData.upgradeType == UpgradeType.ItemGet)
        {
            // 별 3개, 파란 패널
            SetLevelStarAlpha(passiveItems.GetItemLevel(upgradeData.item), 3);
            panel_item.SetActive(true);
            panel_synergy.SetActive(false);
            panel_weapon.SetActive(false);
            panel_instant_items.SetActive(false);
        }
        else if(upgradeData.upgradeType == UpgradeType.SynergyUpgrade)
        {
            Debug.Log("Synergy");
            panel_item.SetActive(false);
            panel_weapon.SetActive(false);
            panel_synergy.SetActive(true);
            levelBar.SetActive(false);
            panel_instant_items.SetActive(false);
        }
        else if(upgradeData.upgradeType == UpgradeType.Coin || upgradeData.upgradeType == UpgradeType.Heal)
        {
            panel_item.SetActive(false);
            panel_weapon.SetActive(false);
            panel_synergy.SetActive(false);
            levelBar.SetActive(false);
            panel_instant_items.SetActive(true);
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
        // 별을 모두 비활성화 한 후
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetActive(false);
        }

        // baseNumber 갯수만큼 활성화
        for (int i = 0; i < baseNumbers; i++)
        {
            levelOff[i].color = new Color(levelOff[i].color.r, levelOff[i].color.g, levelOff[i].color.b, 1f);
            stars[i].SetActive(true);
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

    // 업그레이드 버튼 매니져에서 참조
    public void Selected()
    {
        anim.SetTrigger("Select");
    }

    public void UnSelected()
    {
        unSelectionPanel.SetActive(true);
    }

    public void ResetUnseletedPanel()
    {
        unSelectionPanel.SetActive(false);
    }
    
}
