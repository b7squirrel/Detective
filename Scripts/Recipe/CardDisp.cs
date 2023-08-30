using UnityEditor.ShaderGraph;
using UnityEngine;

public class CardDisp : MonoBehaviour
{
    [SerializeField] protected Transform cardBaseContainer;
    [SerializeField] protected Transform starContainer;
    [SerializeField] protected UnityEngine.UI.Image charImage;
    [SerializeField] protected TMPro.TextMeshProUGUI Level;
    [SerializeField] protected GameObject starPrefab;
    GameObject[] stars;

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
        charImage.color = new Color(1, 1, 1, 1);
        charImage.sprite = weaponData.charImage;

        cardBaseContainer.gameObject.SetActive(true);
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

    protected virtual void SetNumStar(int numStars)
    {
        if(stars == null) 
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

    public void EmptyCardDisplay()
    {
        // 별 비활성화
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i].activeSelf)
                    stars[i].SetActive(false);
            }
        }

        // 카드 레벨 텍스트
        Level.text = "";

        // 캐릭터 이미지
        charImage.color = new Color(1, 1, 1, 0);
        cardBaseContainer.gameObject.SetActive(false);
    }
}
