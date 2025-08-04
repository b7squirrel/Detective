using System.Collections;
using UnityEngine;

public class EquipInfoPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI grade;
    [SerializeField] TMPro.TextMeshProUGUI Name;
    [SerializeField] TMPro.TextMeshProUGUI Level;
    [SerializeField] TMPro.TextMeshProUGUI attribute;
    [SerializeField] TMPro.TextMeshProUGUI skillName;
    [SerializeField] TMPro.TextMeshProUGUI skillDescription;
    [SerializeField] TMPro.TextMeshProUGUI skillEvoLevel;
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

    // 처음 패널이 활성화 되면 초기화
    public void SetPanel(CardData cardData, Item itemData, CardDisp _cardDisp, bool isEquipButton, bool isEssential)
    {
        this.cardDisp = _cardDisp;

        grade.text = MyGrade.mGrades[cardData.Grade].ToString();
        Name.text = itemData.DisplayName;
        // grade.color = MyGrade.GradeColors[cardData.Grade];
        NameLabel.color = MyGrade.GradeColors[cardData.Grade];
        GradeLabel.color = MyGrade.GradeColors[cardData.Grade];
        NameLabelGlow.color = MyGrade.GradeGlowColors[cardData.Grade];
        SetItemCardBase(cardData.Grade);
        Level.text = "LV " + cardData.Level.ToString() + " / " + StaticValues.MaxLevel.ToString();

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
        // Debug.Log($"클릭한 장비는 {weaponItemData.itemData.Name} 입니다");
        itemImage.sprite = weaponItemData.itemData.charImage;
        // anim.runtimeAnimatorController = itemData.CardItemAnimator.CardImageAnim;
        anim.enabled = false;
        // anim.SetTrigger("Card");

        equipButton.SetActive(isEquipButton);

        if (isEssential)
        {
            unEquipButton.SetActive(false);
            return;
        }

        unEquipButton.SetActive(!isEquipButton);
    }
    
    // 레벨업을 하면 레벨과 속성을 업데이트
    public void UpdatePanel(int _level, int _attribute)
    {
        Level.text = "LV " + _level.ToString() + " / " + StaticValues.MaxLevel.ToString();
        attribute.text = _attribute.ToString();

        Debug.Log("Ugraded");
        StartCoroutine(PopFontSize(Level));
        StartCoroutine(PopFontSize(attribute));

    }
    IEnumerator PopFontSize(TMPro.TMP_Text _text)
    {
        float initFontSize = _text.fontSize;
        _text.fontSize = _text.fontSize + 7f;
        yield return new WaitForSeconds(.1f);
        _text.fontSize = initFontSize;
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
            skillName.text = Skills.SkillNames[_cardData.PassiveSkill].ToString();
            skillDescription.text = Skills.SkillDescriptions[_cardData.PassiveSkill];
            skillLabel.color = MyGrade.GradeColors[_cardData.Grade];
            int skillFullNumber = GameManager.instance.startingDataContainer.GetSkillName();
            if (skillFullNumber % 10 == 0)
            {
                skillEvoLevel.text = "I";
            }
            if (skillFullNumber % 10 == 1)
            {
                skillEvoLevel.text = "II";
            }
            if (skillFullNumber % 10 == 2)
            {
                skillEvoLevel.text = "III";
            }
        }
    }
}
