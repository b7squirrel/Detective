using UnityEngine;
using UnityEngine.UI;

public class EquipDisplayUI : MonoBehaviour
{
    [SerializeField] Animator charImage;
    [SerializeField] Animator[] EquipmentImages;
    [SerializeField] Image HeadImage;
    [SerializeField] Image ChestImage;
    [SerializeField] Image FaceImage;
    [SerializeField] Image HandImage;
    [SerializeField] TMPro.TextMeshProUGUI atk, hp;
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] GameObject atkLabel, hpLabel;

    public void SetWeaponDisplay(CardData charCardData, OriAttribute currentAttr)
    {
        charImage.gameObject.SetActive(true);
        WeaponData wd = cardDictionary.GetWeaponData(charCardData);
        // charImage.sprite = wd.charImage;
        charImage.runtimeAnimatorController = wd.CardCharAnimator.CardImageAnim;

        atk.text = currentAttr.Atk.ToString();
        hp.text = currentAttr.Hp.ToString();
    }
    public void SetEquipmentDisplay(CardData itemCardData, bool isAdding)
    {
        Item data = cardDictionary.GetWeaponItemData(itemCardData).itemData;
        int index = new Convert().EquipmentTypeToInt(itemCardData.EquipmentType);

        if (isAdding)
        {
            EquipmentImages[index].gameObject.SetActive(true);

            EquipmentImages[index].runtimeAnimatorController = data.CardItemAnimator.CardImageAnim;
            RestartAnim();
        }
        else
        {
            EquipmentImages[index].gameObject.SetActive(false);
        }
    }
    void RestartAnim()
    {
        for (int i = 0; i < EquipmentImages.Length; i++)
        {
            if (EquipmentImages[i].gameObject.activeSelf)
                EquipmentImages[i].Rebind();
        }
        charImage.Rebind();
    }

    public void OffDisplay()
    {
        atkLabel.SetActive(false);
        hpLabel.SetActive(false);
        // charImage.color = new Color(1, 1, 1, 0);
        for (int i = 0; i < EquipmentImages.Length; i++)
        {
            EquipmentImages[i].gameObject.SetActive(false);
        }
        charImage.gameObject.SetActive(false);
    }
    public void OnDisplay(CardData cardData)
    {
        atkLabel.SetActive(true);
        hpLabel.SetActive(true);
        // charImage.color = new Color(1, 1, 1, 1);
    }
}
