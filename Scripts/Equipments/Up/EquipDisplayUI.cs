using System.Collections;
using UnityEngine;

public class EquipDisplayUI : MonoBehaviour
{
    [SerializeField] Transform cardBaseContainer; // 5레벨
    [SerializeField] Transform starContainer;
    [SerializeField] Transform ribbon;
    [SerializeField] GameObject halo;
    [SerializeField] GameObject titleRibbon;
    [SerializeField] GameObject SkillDescriptionPanel;
    [SerializeField] protected TMPro.TextMeshProUGUI Title;
    [SerializeField] protected TMPro.TextMeshProUGUI Level;
    [SerializeField] protected TMPro.TextMeshProUGUI SkillName;
    [SerializeField] protected TMPro.TextMeshProUGUI SkillDescription;
    [SerializeField] protected GameObject starPrefab;
    GameObject[] stars;
    [SerializeField] Animator charImage;
    [SerializeField] WeaponContainerAnim weaponContainerAnim;
    [SerializeField] Animator[] EquipmentImages;
    [SerializeField] SpriteRenderer[] EquipmentSprites;
    [SerializeField] TMPro.TextMeshProUGUI atk, hp;
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] GameObject atkLabel, hpLabel;
    [SerializeField] GameObject charButton;
    [SerializeField] GameObject backButton;
    [SerializeField] GameObject charUpgradeButton; // 디스플레이되는 오리카드 업그레이드 버튼

    [SerializeField] CanvasGroup charWarningLackCanvasGroup;

    float initAtkFontSize, initHpFontSize;
    Coroutine atkPopCo, hpPopCo;

    public void SetWeaponDisplay(CardData charCardData, OriAttribute currentAttr, string dispName)
    {
        // 별과 카드 색깔
        cardBaseContainer.gameObject.SetActive(true);
        transform.gameObject.SetActive(true);

        int intGrade = charCardData.Grade;
        int intEvoStage = charCardData.EvoStage;
        SetNumStar(intEvoStage + 1);

        SkillDescriptionPanel.SetActive(true);
        SkillName.text = Skills.SkillNames[charCardData.PassiveSkill - 1];
        SkillName.color = MyGrade.GradeColors[charCardData.Grade];
        SkillDescription.text = Skills.SkillDescriptions[charCardData.PassiveSkill - 1];

        for (int i = 0; i < 5; i++)
        {
            if (i == intGrade)
            {
                cardBaseContainer.GetChild(intGrade).gameObject.SetActive(true);
                ribbon.GetChild(intGrade).gameObject.SetActive(true);
                continue;
            }
            cardBaseContainer.GetChild(i).gameObject.SetActive(false);
            ribbon.GetChild(i).gameObject.SetActive(false);
        }

        // 카드 이름 텍스트
        titleRibbon.SetActive(true);
        Title.text = dispName;
        // 카드 레벨 텍스트
        Level.text = "LV " + charCardData.Level;

        // Halo
        //halo.SetActive(true);

        // 캐릭터 이미지
        charImage.gameObject.SetActive(true);
        WeaponData wd = cardDictionary.GetWeaponItemData(charCardData).weaponData;
        // charImage.sprite = wd.charImage;
        charImage.runtimeAnimatorController = wd.Animators.InGamePlayerAnim;

        if (atkPopCo != null)
            StopCoroutine(atkPopCo);

        if (hpPopCo != null)
            StopCoroutine(hpPopCo);

        atk.text = currentAttr.Atk.ToString();
        hp.text = currentAttr.Hp.ToString();

        initAtkFontSize = atk.fontSize;
        initHpFontSize = hp.fontSize;

        charButton.SetActive(true);
        backButton.SetActive(true);
        charUpgradeButton.SetActive(true);
    }
    public void SetAtkHpStats(int _currentAtk, int _currnetHp)
    {
        atk.text = _currentAtk.ToString();
        hp.text = _currnetHp.ToString();

        if (atkPopCo != null)
            StopCoroutine(atkPopCo);

        if (hpPopCo != null)
            StopCoroutine(hpPopCo);

        atkPopCo = StartCoroutine(PopFontSize(true));
        hpPopCo = StartCoroutine(PopFontSize(false));
    }
    IEnumerator PopFontSize(bool _atk)
    {
        if (_atk)
        {
            atk.fontSize = initAtkFontSize;
            atk.fontSize += 10f;
            yield return new WaitForSeconds(.1f);
            atk.fontSize = initAtkFontSize;
        }
        else
        {
            hp.fontSize = initHpFontSize;
            hp.fontSize += 10f;
            yield return new WaitForSeconds(.1f);
            hp.fontSize = initHpFontSize;
        }

    }
    // 오리 위에 장착된 장비 표시/ 숨기기
    public void SetEquipmentDisplay(CardData itemCardData, bool isAdding)
    {
        Item data = cardDictionary.GetWeaponItemData(itemCardData).itemData;
        int index = new Convert().EquipmentTypeToInt(itemCardData.EquipmentType);

        if (isAdding)
        {
            EquipmentImages[index].gameObject.SetActive(true);
            if (itemCardData.EssentialEquip == "Essential")
            {
                //Debug.Log("Essential = " + data.Name);
                //EquipmentImages[index].runtimeAnimatorController
                //                      = data.CardItemAnimator.InGamePlayerAnim;
                EquipmentSprites[index].sprite = data.equippedImage;

            }
            else
            {
                EquipmentSprites[index].sprite = data.equippedImage;
            }
            RestartAnim();
        }
        else
        {
            EquipmentImages[index].gameObject.SetActive(false);

        }

        charButton.SetActive(true);
    }
    public void SetLevelUI(CardData cardOnDisplay)
    {
        Level.text = "LV " + cardOnDisplay.Level;

    }
    // 오리의 idle과 타이밍을 맞추기 위해서 장비가 장착될 때마다 애니메이션 리셋
    void RestartAnim()
    {
        for (int i = 0; i < EquipmentImages.Length; i++)
        {
            if (EquipmentImages[i].gameObject.activeSelf)
                EquipmentImages[i].Rebind();
        }
        charImage.Rebind();
    }

    protected virtual void SetNumStar(int numStars)
    {
        if (stars == null)
        {
            // 5개 만들어서 비활성화
            stars = new GameObject[5];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Instantiate(starPrefab, starContainer);
                stars[i].SetActive(false);
            }
        }

        // 등급만큼 별 활성화하고 별리스트에 넣기
        for (int i = 0; i < numStars; i++)
        {
            stars[i].SetActive(true);
        }
    }

    public void OffDisplay()
    {
        // Base Container 비활성화
        cardBaseContainer.gameObject.SetActive(false);

        // 스킬 설명 패널 비활성화
        SkillDescriptionPanel.SetActive(false);

        // 뒤로 가기 버튼 비 활성화
        backButton.SetActive(false);

        // 별 비활성화
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i].activeSelf)
                    stars[i].SetActive(false);
            }
        }

        // Halo
        halo.SetActive(false);

        // 카드 레벨 텍스트
        titleRibbon.SetActive(false);

        Level.text = "";
        Title.text = "";

        atkLabel.SetActive(false);
        hpLabel.SetActive(false);
        // charImage.color = new Color(1, 1, 1, 0);
        for (int i = 0; i < EquipmentImages.Length; i++)
        {
            EquipmentImages[i].gameObject.SetActive(false);
        }
        charImage.gameObject.SetActive(false);

        charButton.SetActive(false);
        charUpgradeButton.SetActive(false);

        GetComponentInParent<EquipmentPanelManager>().TempKillAllTweens();
        charWarningLackCanvasGroup.gameObject.SetActive(false);


    }
    public void OnDisplay(CardData cardData)
    {
        atkLabel.SetActive(true);
        hpLabel.SetActive(true);
        charUpgradeButton.SetActive(true);

        charWarningLackCanvasGroup.gameObject.SetActive(true);
        // charImage.color = new Color(1, 1, 1, 1);
    }
}
