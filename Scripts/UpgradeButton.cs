using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
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
    [SerializeField] GameObject titleGroup; // 선택되지 않은 카드의 타이틀을 숨기기 위해. 어두운 레이어를 올릴 때 불편함
    [SerializeField] GameObject newWeaponText; // 새로운 오리! 텍스트
    [SerializeField] GameObject newItemText; // 새로운 아이템! 텍스트
    [SerializeField] Image titleSticker; // 타이틀 포스트잇 색깔을 조절하기 위해
    [SerializeField] Color weaponStickerColor; // 오리 포스트잇 색깔
    [SerializeField] Color itemStickerColor; // 아이템 포스트잇 색깔
    [SerializeField] Color synergyStickerColor; // 시너지 오리 포스트잇 색깔
    [SerializeField] Color instantStickerColor; // 인스탄트 아이템 포스트잇 색깔
    [SerializeField] List<Image> levelOn;
    [SerializeField] List<Image> levelOff;
    [SerializeField] List<Animator> levelOnAnim;
    [SerializeField] List<Animator> starSelectedEffectAnim;
    [SerializeField] GameObject synergyGroup; // 일반 오리카드가 아니면 비활성화 하기 위해
    [SerializeField] GameObject synergyText; // 시너지 아이콘 표시 텍스트
    [SerializeField] Image synergyCouipleIcon;
    [SerializeField] UpgradePanelWeaponIcon upgradePanelWeaponIcon; // 오리와 장비. 애니메이션
    WeaponContainer weaponContainer;
    PassiveItems passiveItems;

    public bool IsClicked { get; private set; } // 여러개의 버튼이 눌러지지 않도록

    Animator anim;
    int cardLevel;

    void OnEnable()
    {


        ClearLevelstars();
        levelBar.SetActive(false);
        unSelectionPanel.SetActive(false);
        upgradePanelWeaponIcon.gameObject.SetActive(false);

    }

    public void Set(UpgradeData upgradeData)
    {
        if (anim == null) anim = GetComponent<Animator>();
        anim.SetTrigger("Reset");

        titleGroup.SetActive(true);
        // 일단 비활성화 하고 더 아래쪽에서 조건에 따라 표시하거나 비활성화 한채로 두기
        newWeaponText.SetActive(false);
        newItemText.SetActive(false);

        if (upgradeData.weaponData != null)
        {
            iconItem.color = new Color(iconItem.color.r, iconItem.color.g, iconItem.color.b, 0);


            upgradePanelWeaponIcon.gameObject.SetActive(true);
            upgradePanelWeaponIcon.InitWeaponIcon(upgradeData.weaponData); // 오리 아이콘 셋업

            // 시너지 업그레이드일 때는 시너지 이름 표시
            if (upgradeData.upgradeType != UpgradeType.SynergyUpgrade)
            {
                upgradeName.text = upgradeData.weaponData.DisplayName;
            }
            else
            {
                upgradeName.text = upgradeData.weaponData.SynergyDispName;
            }
        }
        else // 오리가 아닌 카드들이라면
        {
            if (upgradeData == null) Debug.Log("upgrade Data를 넘겨받지 못했습니다.");
            if (upgradeData.item == null) Debug.Log("upgrade Data의 item이 Null입니다..");
            if (upgradeData.item.charImage == null) Debug.Log("upgrade Data의 item의 charImage가 Null입니다...");
            iconItem.sprite = upgradeData.item.charImage;
            iconItem.preserveAspect = true;
            iconItem.color = new Color(iconItem.color.r, iconItem.color.g, iconItem.color.b, 1f);
            upgradePanelWeaponIcon.gameObject.SetActive(false);
            iconItem.SetNativeSize();

            if (upgradeData.item.DisplayName != "")
                upgradeName.text = upgradeData.item.DisplayName;
        }
        //if (upgradeData.DisplayName != "")
        //{
        //    upgradeName.text = upgradeData.weaponData.DisplayName;
        //    //upgradeNameShadow.text = upgradeData.Name;
        //}

        description.text = upgradeData.description;

        synergyGroup.SetActive(false);
        synergyCouipleIcon.color = new Color(1, 1, 1, 0);
        synergyText.SetActive(false);

        levelBar.SetActive(true);

        if (upgradeData.weaponData != null) // 넘겨 받은 업그레이드 데이터가 Weapon 이라면
        {
            if (upgradeData.upgradeType != UpgradeType.SynergyUpgrade) // 시너지 업그레이드가 아닌 경우에만 시너지 커플 표시
            {
                synergyGroup.SetActive(true); // 일반 오리일 때만 시너지 포스트잇 표시
                synergyCouipleIcon.color = new Color(1, 1, 1, 1);
                synergyCouipleIcon.sprite = upgradeData.weaponData.SynergyItem.charImage;
                synergyCouipleIcon.preserveAspect = true;
                synergyText.SetActive(true);
            }
        }

        if (upgradeData.upgradeType == UpgradeType.WeaponUpgrade) // 무기 업그레이드일 경우
        {
            // 별 5개, 연두 패널
            if (weaponContainer == null) weaponContainer = Player.instance.GetComponent<WeaponContainer>();
            SetLevelStarAlpha(weaponContainer.GetWeaponLevel(upgradeData.weaponData), StaticValues.MaxGrade);
            panel_item.SetActive(false);
            panel_synergy.SetActive(false);
            panel_weapon.SetActive(true);
            panel_instant_items.SetActive(false);

            // 처음 획득한 카드라면 새로운 오리! 텍스트 표시
            if (weaponContainer.GetWeaponLevel(upgradeData.weaponData) == 0) newWeaponText.SetActive(true);

            titleSticker.color = weaponStickerColor;
        }
        else if (upgradeData.upgradeType == UpgradeType.ItemUpgrade || upgradeData.upgradeType == UpgradeType.ItemGet)
        {
            // 별 3개, 파란 패널
            if (passiveItems == null) passiveItems = Player.instance.GetComponent<PassiveItems>();
            SetLevelStarAlpha(passiveItems.GetItemLevel(upgradeData.item), StaticValues.MaxItemGrade);
            panel_item.SetActive(true);
            panel_synergy.SetActive(false);
            panel_weapon.SetActive(false);
            panel_instant_items.SetActive(false);

            // 처음 획득한 카드라면 새로운 아이템! 텍스트 표시
            if (passiveItems.GetItemLevel(upgradeData.item) == 0) newItemText.SetActive(true);

            titleSticker.color = itemStickerColor;
        }
        else if (upgradeData.upgradeType == UpgradeType.SynergyUpgrade)
        {
            Debug.Log("Synergy");
            panel_item.SetActive(false);
            panel_weapon.SetActive(false);
            panel_synergy.SetActive(true);
            levelBar.SetActive(false);
            panel_instant_items.SetActive(false);

            titleSticker.color = synergyStickerColor;
        }
        else if (upgradeData.upgradeType == UpgradeType.Coin || upgradeData.upgradeType == UpgradeType.Heal)
        {
            panel_item.SetActive(false);
            panel_weapon.SetActive(false);
            panel_synergy.SetActive(false);
            levelBar.SetActive(false);
            panel_instant_items.SetActive(true);

            titleSticker.color = instantStickerColor;
        }
    }

    internal void Clean()
    {
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
        // iconWeapon.color = new Color(iconWeapon.color.r, iconWeapon.color.g, iconWeapon.color.b, 0f);
        // iconItem.color = new Color(iconItem.color.r, iconItem.color.g, iconItem.color.b, 0f);

        titleGroup.SetActive(false);
        synergyGroup.SetActive(false);
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