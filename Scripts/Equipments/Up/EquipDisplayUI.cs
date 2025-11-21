using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class EquipDisplayUI : MonoBehaviour
{
    [SerializeField] Transform cardBaseContainer; // 5레벨
    [SerializeField] Transform starContainer;
    [SerializeField] Transform ribbon;
    [SerializeField] GameObject halo;
    [SerializeField] GameObject titleRibbon;
    [SerializeField] GameObject titleRibbonShadow;
    [SerializeField] GameObject SkillDescriptionPanel;
    [SerializeField] protected TextMeshProUGUI Title;
    [SerializeField] protected TextMeshProUGUI Level;
    [SerializeField] protected TextMeshProUGUI LevelShadow;
    [SerializeField] protected TextMeshProUGUI SkillName;
    [SerializeField] protected TextMeshProUGUI SkillDescription;
    [SerializeField] protected GameObject starPrefab;
    GameObject[] stars;
    SetCardDataOnSlot setCardDataOnSlot; // 카드 데이터와 슬롯을 넘겨 받아서 슬롯에 카드를 표시

    [SerializeField] TextMeshProUGUI atk, hp;
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] GameObject atkLabel, hpLabel;
    [SerializeField] GameObject charButton;
    [SerializeField] GameObject backButton;
    [SerializeField] GameObject charUpgradeButton; // 디스플레이되는 오리카드 업그레이드 버튼

    [SerializeField] CanvasGroup charWarningLackCanvasGroup;

    [Header("Debug")]
    [SerializeField] GameObject[] testParts;

    float initAtkFontSize, initHpFontSize;
    Tween atkPopTween, hpPopTween;

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
        titleRibbonShadow.SetActive(true);
        Title.text = dispName;
        // 카드 레벨 텍스트
        Level.text = "레벨 " + charCardData.Level;
        LevelShadow.text = Level.text;

        // 디버그
        if (testParts != null)
        {
            for (int i = 0; i < testParts.Length; i++)
            {
                testParts[i].SetActive(false);
            }
        }

        // 기존 애니메이션 정리
        atkPopTween?.Kill();
        hpPopTween?.Kill();

        atk.text = currentAttr.Atk.ToString();
        hp.text = currentAttr.Hp.ToString();

        // 초기 폰트 사이즈 저장 (한 번만)
        if (initAtkFontSize == 0) initAtkFontSize = atk.fontSize;
        if (initHpFontSize == 0) initHpFontSize = hp.fontSize;

        // 폰트 사이즈 초기화
        atk.fontSize = initAtkFontSize;
        hp.fontSize = initHpFontSize;

        charButton.SetActive(true);
        backButton.SetActive(true);
        charUpgradeButton.SetActive(true);
    }

    public void SetAtkHpStats(int _currentAtk, int _currnetHp)
    {
        atk.text = _currentAtk.ToString();
        hp.text = _currnetHp.ToString();

        // 기존 애니메이션 중단
        atkPopTween?.Kill();
        hpPopTween?.Kill();

        // 초기화 후 애니메이션
        atk.fontSize = initAtkFontSize;
        hp.fontSize = initHpFontSize;

        atkPopTween = PopFontSizeTween(atk, initAtkFontSize);
        hpPopTween = PopFontSizeTween(hp, initHpFontSize);
    }

    Tween PopFontSizeTween(TextMeshProUGUI text, float originalSize)
    {
        float targetSize = originalSize + 12f;
        return DOTween.To(() => text.fontSize, x => text.fontSize = x, targetSize, 0.1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => 
            {
                text.fontSize = originalSize;
            });
    }
    
    public void SetLevelUI(CardData cardOnDisplay)
    {
        Level.text = "LV " + cardOnDisplay.Level;
        LevelShadow.text = Level.text;
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
        titleRibbonShadow.SetActive(false);

        Level.text = "";
        LevelShadow.text = "";
        Title.text = "";

        atkLabel.SetActive(false);
        hpLabel.SetActive(false);

        charButton.SetActive(false);
        charUpgradeButton.SetActive(false);

        // Tween 정리
        atkPopTween?.Kill();
        hpPopTween?.Kill();
        
        // 폰트 사이즈 초기화
        if (initAtkFontSize > 0) atk.fontSize = initAtkFontSize;
        if (initHpFontSize > 0) hp.fontSize = initHpFontSize;

        GetComponentInParent<EquipmentPanelManager>().TempKillAllTweens();
        charWarningLackCanvasGroup.gameObject.SetActive(false);
    }

    public void OnDisplay(CardData cardData)
    {
        atkLabel.SetActive(true);
        hpLabel.SetActive(true);
        charUpgradeButton.SetActive(true);

        charWarningLackCanvasGroup.gameObject.SetActive(true);
    }
}