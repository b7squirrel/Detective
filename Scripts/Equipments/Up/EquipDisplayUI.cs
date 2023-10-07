using UnityEngine;
using UnityEngine.UI;

public class EquipDisplayUI : MonoBehaviour
{
    [SerializeField] Transform cardBaseContainer; // ��� 5�� 
    [SerializeField] Transform starContainer;
    [SerializeField] protected TMPro.TextMeshProUGUI Title;
    [SerializeField] protected TMPro.TextMeshProUGUI Level;
    [SerializeField] protected GameObject starPrefab;
    GameObject[] stars;
    [SerializeField] Animator charImage;
    [SerializeField] Animator[] EquipmentImages;
    [SerializeField] Image HeadImage;
    [SerializeField] Image ChestImage;
    [SerializeField] Image FaceImage;
    [SerializeField] Image HandImage;
    [SerializeField] TMPro.TextMeshProUGUI atk, hp;
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] GameObject atkLabel, hpLabel;
    [SerializeField] GameObject button;

    public void SetWeaponDisplay(CardData charCardData, OriAttribute currentAttr)
    {
        // ���� ī�� ����
        cardBaseContainer.gameObject.SetActive(true);

        int intGrade = new Convert().GradeToInt(charCardData.Grade);
        SetNumStar(intGrade + 1);

        for (int i = 0; i < 5; i++)
        {
            cardBaseContainer.GetChild(i).gameObject.SetActive(false);
        }
        cardBaseContainer.GetChild(intGrade).gameObject.SetActive(true);

        // ī�� �̸� �ؽ�Ʈ
        Title.text = charCardData.Name;
        // ī�� ���� �ؽ�Ʈ
        Level.text = "LV1";

        // ���� �̹���
        charImage.gameObject.SetActive(true);
        WeaponData wd = cardDictionary.GetWeaponItemData(charCardData).weaponData;
        // charImage.sprite = wd.charImage;
        charImage.runtimeAnimatorController = wd.Animators.CardImageAnim;

        atk.text = currentAttr.Atk.ToString();
        hp.text = currentAttr.Hp.ToString();

        button.SetActive(true);
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

        button.SetActive(true);
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

    protected virtual void SetNumStar(int numStars)
    {
        if (stars == null)
        {
            // 5�� ���� ��Ȱ��ȭ
            stars = new GameObject[5];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Instantiate(starPrefab, starContainer);
                stars[i].SetActive(false);
            }
        }

        // ��޸�ŭ �� Ȱ��ȭ�ϰ� ������Ʈ�� �ֱ�
        for (int i = 0; i < numStars; i++)
        {
            stars[i].SetActive(true);
        }
    }

    public void OffDisplay()
    {
        // ī�� Base Container ��Ȱ��ȭ
        cardBaseContainer.gameObject.SetActive(false);

        // �� ��Ȱ��ȭ
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i].activeSelf)
                    stars[i].SetActive(false);
            }
        }

        // ī�� ���� �ؽ�Ʈ
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

        button.SetActive(false);
    }
    public void OnDisplay(CardData cardData)
    {
        atkLabel.SetActive(true);
        hpLabel.SetActive(true);
        // charImage.color = new Color(1, 1, 1, 1);
    }
}
