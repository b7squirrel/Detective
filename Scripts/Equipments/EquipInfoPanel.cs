using UnityEngine;

public class EquipInfoPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI grade;
    [SerializeField] TMPro.TextMeshProUGUI Name;
    [SerializeField] TMPro.TextMeshProUGUI Level;
    [SerializeField] TMPro.TextMeshProUGUI attribute;
    [SerializeField] UnityEngine.UI.Image itemImage;
    [SerializeField] Animator anim;
    [SerializeField] UnityEngine.UI.Image attributeImage;
    [SerializeField] Sprite atkIcon, hpIcon;
    [SerializeField] GameObject equipButton, unEquipButton;

    [SerializeField] CardsDictionary cardDictionary;
    public CardDisp cardDisp;

    // 처음 패널이 활성화 되면 초기화
    public void SetPanel(CardData cardData, Item itemData, CardDisp _cardDisp, bool isEquipButton)
    {
        this.cardDisp = _cardDisp;
        
        grade.text = cardData.Grade;
        Name.text = cardData.Name;
        Level.text = cardData.Level;
        if(cardData.EquipmentType == EquipmentType.Hand.ToString()) // 무기 카드라면
        {
            attributeImage.sprite = atkIcon;
            attribute.text = cardData.Atk;
        }
        else
        {
            attributeImage.sprite = hpIcon;
            attribute.text = cardData.Hp;
        }
        
        WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(cardData);
        itemImage.sprite = weaponItemData.itemData.charImage;
        anim.runtimeAnimatorController = itemData.CardItemAnimator.CardImageAnim;
        anim.SetTrigger("Card");

        equipButton.SetActive(isEquipButton);
        unEquipButton.SetActive(!isEquipButton);
    }
    // 레벨업을 하면 레벨과 속성을 업데이트
    public void UpdatePanel(string _level, string _attribute)
    {
        Level.text = new Convert().StringToInt(_level).ToString();
        attribute.text = new Convert().StringToInt(_attribute).ToString();
    }
}
