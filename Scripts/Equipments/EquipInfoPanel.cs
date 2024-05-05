using UnityEngine;

public class EquipInfoPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI grade;
    [SerializeField] TMPro.TextMeshProUGUI Name;
    [SerializeField] TMPro.TextMeshProUGUI Level;
    [SerializeField] TMPro.TextMeshProUGUI attribute;
    [SerializeField] UnityEngine.UI.Image NameLabel;
    [SerializeField] UnityEngine.UI.Image NameLabelGlow;
    [SerializeField] UnityEngine.UI.Image itemImage;
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
        grade.color = MyGrade.GradeColors[cardData.Grade];
        NameLabel.color = MyGrade.GradeColors[cardData.Grade];
        NameLabelGlow.color = MyGrade.GradeGlowColors[cardData.Grade];
        Name.text = cardData.Name;
        SetItemCardBase(cardData.Grade);
        Level.text = "LV " + cardData.Level.ToString() + " / " + StaticValues.MaxLevel.ToString();

        if (cardData.Atk != 0)
        {
            attributeImage.sprite = atkIcon;
            attribute.text = "+ " + cardData.Atk.ToString();
            Debug.Log("ATK = " + cardData.Atk.ToString());
        }
        else if (cardData.Hp != 0)
        {
            attributeImage.sprite = hpIcon;
            attribute.text = "+ " + cardData.Hp.ToString();
        }

        WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(cardData);
        itemImage.sprite = weaponItemData.itemData.charImage;
        anim.runtimeAnimatorController = itemData.CardItemAnimator.CardImageAnim;
        anim.SetTrigger("Card");

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
        Level.text = _level.ToString();
        attribute.text = _attribute.ToString();
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
}
