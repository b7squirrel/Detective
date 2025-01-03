using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [SerializeField] protected Transform cardBaseContainer;
    [SerializeField] protected Transform starContainer;
    [SerializeField] protected Image charImage;
    [SerializeField] protected TextMeshProUGUI Level;
    [SerializeField] protected GameObject starPrefab;
    public void InitWeaponCardDisplay(WeaponData weaponData)
    {
        // 별과 카드 색깔
        int intGrade = (int)weaponData.grade;
        SetNumStar(intGrade + 1);

        for (int i = 0; i < 5; i++)
        {
            cardBaseContainer.GetChild(i).gameObject.SetActive(false);
        }
        cardBaseContainer.GetChild(intGrade).gameObject.SetActive(true);

        // 카드 레벨 텍스트
        Level.text = "LV1";

        // 캐릭터 이미지
        charImage.sprite = weaponData.charImage;
    }

    public void InitItemCardDisplay(Item itemData)
    {
        // 별과 카드 색깔
        int intGrade = (int)itemData.grade;
        SetNumStar(intGrade + 1);
        cardBaseContainer.GetChild(intGrade).gameObject.SetActive(true);

        // 카드 레벨 텍스트
        Level.text = "LV1";

        // 캐릭터 이미지
        charImage.sprite = itemData.charImage;
    }

    public void UpdateCard(int level)
    {
        Level.text = "LV" + level.ToString();
    }

    protected virtual void SetNumStar(int numStars)
    {
        // 등급만큼 별 생성하고 별리스트에 넣기
        for (int i = 0; i < numStars; i++)
        {
            Instantiate(starPrefab, starContainer);
        }
    }
}
