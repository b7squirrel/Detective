using UnityEngine;
using UnityEngine.UI;

public class EquipDisplayUI : MonoBehaviour
{
    [SerializeField] Image charImage;
    [SerializeField] TMPro.TextMeshProUGUI atk, hp;
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] GameObject atkLabel, hpLabel;

    public void SetWeaponDisply(CardData charCardData, OriAttribute currentAttr)
    {
        WeaponData wd = cardDictionary.GetWeaponData(charCardData);
        charImage.sprite = wd.charImage;

        atk.text = currentAttr.Atk.ToString();
        hp.text = currentAttr.Hp.ToString();
    }

    public void OffDisplay()
    {
        atkLabel.SetActive(false);
        hpLabel.SetActive(false);
        charImage.color = new Color(1, 1, 1, 0);
    }
    public void OnDisplay(CardData cardData)
    {
        atkLabel.SetActive(true);
        hpLabel.SetActive(true);
        charImage.color = new Color(1, 1, 1, 1);
    }
}
