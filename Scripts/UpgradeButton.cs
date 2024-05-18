using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] Image iconWeapon;
    [SerializeField] Image iconItem;
    [SerializeField] TMPro.TextMeshProUGUI upgradeName;
    [SerializeField] TMPro.TextMeshProUGUI description; // 설명
    [SerializeField] GameObject levelBar; // 무기, 아이템 레벨 별
    [SerializeField] GameObject panel_weapon; // 무기 패널 연두색
    [SerializeField] GameObject panel_item; // 아이템 패널, 파란색
    [SerializeField] GameObject panel_synergy; // 시너지 패널, 보라색
    [SerializeField] GameObject panel_instant_items; // 동전, 도넛 패널, 노란색
    [SerializeField] GameObject[] stars; // 활성, 비활성 시킬 별
    [SerializeField] GameObject unSelectionPanel; // 선택되지 않으면 카드를 어둡게 하는 역할
    [SerializeField] GameObject synergyText; // 시너지 아이콘 표시 텍스트
    [SerializeField] List<Image> levelOn;
    [SerializeField] List<Image> levelOff;
    [SerializeField] List<Animator> levelOnAnim;
    [SerializeField] List<Animator> starSelectedEffectAnim;
    [SerializeField] Image synergyCouipleIcon;
    WeaponContainer weaponContainer;
    PassiveItems passiveItems;

    public bool IsClicked { get; private set; } // 여러개의 버튼이 눌러지지 않도록

    Animator anim;
    int cardLevel;

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

        if(upgradeData.weaponData != null)
        {
            iconWeapon.sprite = upgradeData.weaponData.charImage;
            iconWeapon.preserveAspect = true;
            iconWeapon.color = new Color(iconWeapon.color.r, iconWeapon.color.g, iconWeapon.color.b, 1f);
            iconItem.color = new Color(iconItem.color.r, iconItem.color.g, iconItem.color.b, 0);
        }
        else
        {
            if (upgradeData == null) Debug.Log("upgrade Data를 넘겨받지 못했습니다.");
            if (upgradeData.item == null) Debug.Log("upgrade Data의 item이 Null입니다..");
            if (upgradeData.item.charImage == null) Debug.Log("upgrade Data의 item의 charImage가 Null입니다...");
            iconItem.sprite = upgradeData.item.charImage;
            iconItem.preserveAspect = true;
            iconItem.color = new Color(iconItem.color.r, iconItem.color.g, iconItem.color.b, 1f);
            iconWeapon.color = new Color(iconWeapon.color.r, iconWeapon.color.g, iconWeapon.color.b, 0);
            iconItem.SetNativeSize();

        }
        if (upgradeData.Name != "")
        {
            upgradeName.text = upgradeData.Name;
            //upgradeNameShadow.text = upgradeData.Name;
        }

        description.text = upgradeData.description;

        synergyCouipleIcon.color = new Color(1, 1, 1, 0);
        synergyText.SetActive(false);

        

        if (upgradeData.weaponData != null) // 넘겨 받은 업그레이드 데이터가 Weapon 이라면
        {
            if (upgradeData.upgradeType != UpgradeType.SynergyUpgrade) // 시너지 업그레이드가 아닌 경우에만 시너지 커플 표시
            {
                synergyCouipleIcon.color = new Color(1, 1, 1, 1);
                synergyCouipleIcon.sprite = upgradeData.weaponData.SynergyItem.charImage;
                synergyCouipleIcon.preserveAspect = true;
                synergyText.SetActive(true);
            }
        }

        levelBar.SetActive(true);
        if(upgradeData.upgradeType == UpgradeType.WeaponUpgrade) // 무기 업그레이드일 경우
        {
            // 별 5개, 연두 패널
            SetLevelStarAlpha(weaponContainer.GetWeaponLevel(upgradeData.weaponData), StaticValues.MaxGrade);
            panel_item.SetActive(false);
            panel_synergy.SetActive(false);
            panel_weapon.SetActive(true);
            panel_instant_items.SetActive(false);
        }
        else if(upgradeData.upgradeType == UpgradeType.ItemUpgrade || upgradeData.upgradeType == UpgradeType.ItemGet)
        {
            // 별 3개, 파란 패널
            SetLevelStarAlpha(passiveItems.GetItemLevel(upgradeData.item), StaticValues.MaxItemGrade);
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
        iconWeapon.sprite = null;
        iconItem.sprite = null;
        ClearLevelstars();
        IsClicked = false;
        levelBar.SetActive(false);
    }

    void SetLevelStarAlpha(int level, int baseNumbers)
    {
        cardLevel = level;
        // 별을 모두 비활성화 한 후
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetActive(false); // stars는 별 하나의 요소를 모두 담고 있는 그룹이다.
        }

        // baseNumber 갯수만큼 활성화
        for (int i = 0; i < baseNumbers; i++)
        {
            levelOff[i].color = new Color(levelOff[i].color.r, levelOff[i].color.g, levelOff[i].color.b, .2f);
            levelOn[i].gameObject.SetActive(false);
            stars[i].SetActive(true);
        }
        for (int i = 0; i < cardLevel + 1; i++)
        {
            // levelOn[i].color = new Color(levelOn[i].color.r, levelOn[i].color.g, levelOn[i].color.b, 1f);
            levelOn[i].gameObject.SetActive(true);
            levelOnAnim[i].SetTrigger("Reset");
        }
        StarBlinking(cardLevel);
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
        StartCoroutine(StarSelected(cardLevel));
    }

    public void UnSelected() // 선택되지 않은 카드의 행동
    {
        unSelectionPanel.SetActive(true);
        iconWeapon.color = new Color(iconWeapon.color.r, iconWeapon.color.g, iconWeapon.color.b, 0f);
        iconItem.color = new Color(iconItem.color.r, iconItem.color.g, iconItem.color.b, 0f);
    }

    public void ResetUnseletedPanel()
    {
        unSelectionPanel.SetActive(false);
    }
    
    void StarBlinking(int index)
    {
        levelOnAnim[index].SetTrigger("Blink");
    }

    IEnumerator StarSelected(int index)
    {
        yield return new WaitForSecondsRealtime(.05f);
        levelOnAnim[index].SetTrigger("Selected");
        starSelectedEffectAnim[index].SetTrigger("Selected");
    }

    public void ButtonClicked()
    {
        IsClicked = true;
    }

    public void ResetButtonClicked()
    {
        IsClicked = false;
    }
}
