using UnityEngine;
using DG.Tweening;
using TMPro;

public class EquipInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI grade;
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] TextMeshProUGUI Level;
    [SerializeField] TextMeshProUGUI attribute;
    [SerializeField] TextMeshProUGUI skillName;
    [SerializeField] TextMeshProUGUI skillDescription;
    [SerializeField] TextMeshProUGUI skillEvoLevel;
    [SerializeField] UnityEngine.UI.Image NameLabel;
    [SerializeField] UnityEngine.UI.Image GradeLabel;
    [SerializeField] UnityEngine.UI.Image NameLabelGlow;
    [SerializeField] UnityEngine.UI.Image itemImage;
    [SerializeField] UnityEngine.UI.Image skillLabel;
    [SerializeField] Animator anim;
    [SerializeField] UnityEngine.UI.Image attributeImage;
    [SerializeField] Sprite atkIcon, hpIcon;
    [SerializeField] GameObject equipButton, unEquipButton;
    [SerializeField] GameObject[] itemCardBase;

    [SerializeField] CardsDictionary cardDictionary;
    public CardDisp cardDisp;

    float initLevelFontSize, initAttributeFontSize;
    Tween levelPopTween, attributePopTween;

    // ★ 현재 표시 중인 데이터 저장
    private CardData currentCardData;
    private Item currentItemData;

    void Awake()
    {
        // 초기 폰트 사이즈 저장
        if (Level != null) initLevelFontSize = Level.fontSize;
        if (attribute != null) initAttributeFontSize = attribute.fontSize;
        
        // ★ 언어 변경 이벤트 구독
        LocalizationManager.OnLanguageChanged += UpdateText;
    }

    void OnDestroy()
    {
        // 파괴될 때 Tween 정리
        levelPopTween?.Kill();
        attributePopTween?.Kill();
        
        // ★ 언어 변경 이벤트 구독 해제
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    // ★ 텍스트 업데이트 메서드
    void UpdateText()
    {
        if (currentCardData == null || currentItemData == null) return;
        
        // 등급
        grade.text = LocalizationManager.Game.gradeNames[currentCardData.Grade];
        
        // 아이템 이름
        Name.text = LocalizationManager.Item.GetItemDisplayName(currentItemData.Name);
        
        // 레벨
        Level.text = LocalizationManager.Game.level + " " + 
                     currentCardData.Level.ToString() + " / " + 
                     StaticValues.MaxLevel.ToString();
        
        // 스킬 이름 및 설명
        if (currentCardData.PassiveSkill >= 0 && currentCardData.PassiveSkill < Skills.SkillNames.Length)
        {
            int passiveSkillIndex = currentCardData.PassiveSkill - 1;
            skillName.text = LocalizationManager.Item.itemSkillNames[passiveSkillIndex];
            skillDescription.text = LocalizationManager.Item.itemSkillDescriptions[passiveSkillIndex];
        }
    }

    // 처음 패널이 활성화 되면 초기화
    public void SetPanel(CardData cardData, Item itemData, CardDisp _cardDisp, bool isEquipButton, bool isEssential)
    {
        Logger.LogWarning($"item data = {itemData.DisplayName}");
        
        // ★ 현재 데이터 저장
        currentCardData = cardData;
        currentItemData = itemData;
        
        this.cardDisp = _cardDisp;

        // ★ 다국어 적용
        grade.text = LocalizationManager.Game.gradeNames[cardData.Grade];
        Name.text = LocalizationManager.Item.GetItemDisplayName(itemData.Name);
        
        NameLabel.color = MyGrade.GradeColors[cardData.Grade];
        GradeLabel.color = MyGrade.GradeColors[cardData.Grade];
        NameLabelGlow.color = MyGrade.GradeGlowColors[cardData.Grade];
        SetItemCardBase(cardData.Grade);
        
        // ★ 다국어 적용
        Level.text = LocalizationManager.Game.level + " " + 
                     cardData.Level.ToString() + " / " + 
                     StaticValues.MaxLevel.ToString();

        GetPassiveSkillLevel(cardData);

        grade.color = Color.black;
        Name.color = Color.black;
        if (cardData.Grade == 4)
        {
            grade.color = Color.white;
            Name.color = Color.white;
        }

        if (cardData.Atk != 0)
        {
            attributeImage.sprite = atkIcon;
            attribute.text = "+ " + cardData.Atk.ToString();
        }
        else if (cardData.Hp != 0)
        {
            attributeImage.sprite = hpIcon;
            attribute.text = "+ " + cardData.Hp.ToString();
        }

        WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(cardData);
        itemImage.sprite = weaponItemData.itemData.charImage;
        anim.enabled = false;

        equipButton.SetActive(isEquipButton);

        if (isEssential)
        {
            unEquipButton.SetActive(false);
            return;
        }

        unEquipButton.SetActive(!isEquipButton);

        // 초기 폰트 사이즈 저장 (Awake에서 못 가져온 경우 대비)
        if (initLevelFontSize == 0) initLevelFontSize = Level.fontSize;
        if (initAttributeFontSize == 0) initAttributeFontSize = attribute.fontSize;

        // 기존 애니메이션 정리
        levelPopTween?.Kill();
        attributePopTween?.Kill();

        // 폰트 사이즈 초기화
        Level.fontSize = initLevelFontSize;
        attribute.fontSize = initAttributeFontSize;
    }
    
    // 레벨업을 하면 레벨과 속성을 업데이트
    public void UpdatePanel(int _level, int _attribute)
    {
        if (currentCardData != null)
        {
            // 레벨 데이터 업데이트
            currentCardData.Level = _level;
        }
        
        // ★ 다국어 적용
        Level.text = LocalizationManager.Game.level + " " + 
                     _level.ToString() + " / " + 
                     StaticValues.MaxLevel.ToString();
        attribute.text = _attribute.ToString();

        Logger.Log("Ugraded");

        // 기존 애니메이션 중단
        levelPopTween?.Kill();
        attributePopTween?.Kill();

        // 폰트 사이즈 초기화 후 애니메이션
        Level.fontSize = initLevelFontSize;
        attribute.fontSize = initAttributeFontSize;

        levelPopTween = PopFontSizeTween(Level, initLevelFontSize);
        attributePopTween = PopFontSizeTween(attribute, initAttributeFontSize);
    }

    Tween PopFontSizeTween(TextMeshProUGUI text, float originalSize)
    {
        float targetSize = originalSize + 7f;
        return DOTween.To(() => text.fontSize, x => text.fontSize = x, targetSize, 0.1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => 
            {
                text.fontSize = originalSize;
            });
    }

    void SetItemCardBase(int _index)
    {
        for (int i = 0; i < itemCardBase.Length; i++)
        {
            if (i == _index)
            {
                itemCardBase[i].SetActive(true);
                continue;
            }
            itemCardBase[i].SetActive(false);
        }
    }

    void GetPassiveSkillLevel(CardData _cardData)
    {
        if (_cardData.PassiveSkill >= 0 && _cardData.PassiveSkill < Skills.SkillNames.Length)
        {
            // ★ 다국어 적용
            int passiveSkillIndex = _cardData.PassiveSkill - 1;
            skillName.text = LocalizationManager.Item.itemSkillNames[passiveSkillIndex];
            skillDescription.text = LocalizationManager.Item.itemSkillDescriptions[passiveSkillIndex];

            skillLabel.color = MyGrade.GradeColors[_cardData.Grade];
        }
    }
}