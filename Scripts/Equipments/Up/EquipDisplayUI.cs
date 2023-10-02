using UnityEngine;
using UnityEngine.UI;

public class EquipDisplayUI : MonoBehaviour
{
    [SerializeField] GameObject charDisp;
    [SerializeField] Animator charImage;
    [SerializeField] Animator HeadImage;
    [SerializeField] Animator ChestImage;
    [SerializeField] Animator FaceImage;
    [SerializeField] Animator HandImage;
    [SerializeField] TMPro.TextMeshProUGUI atk, hp;
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] GameObject atkLabel, hpLabel;

    public void SetWeaponDisply(CardData charCardData, OriAttribute currentAttr)
    {
        WeaponData wd = cardDictionary.GetWeaponData(charCardData);
        // charImage.sprite = wd.charImage;
        charImage.runtimeAnimatorController = wd.CardCharAnimator.CardImageAnim;

        atk.text = currentAttr.Atk.ToString();
        hp.text = currentAttr.Hp.ToString();
    }

    public void OffDisplay()
    {
        atkLabel.SetActive(false);
        hpLabel.SetActive(false);
        // charImage.color = new Color(1, 1, 1, 0);
        // charImage.GetComponent<Image>().color = new Color(1, 1, 1, 0);
        charDisp.SetActive(false);
    }
    public void OnDisplay(CardData cardData)
    {
        atkLabel.SetActive(true);
        hpLabel.SetActive(true);
        // charImage.color = new Color(1, 1, 1, 1);
        // charImage.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        charDisp.SetActive(true);
    }
}
