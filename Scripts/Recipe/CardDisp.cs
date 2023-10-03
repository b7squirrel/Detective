using UnityEngine;

// equipped text는 카드슬롯이 생성될 때 정해짐. 
// 장착되거나 해제되면 Equipment Panel Manager에서 업데이트
public class CardDisp : MonoBehaviour
{
    [SerializeField] protected Transform cardBaseContainer; // 등급 5개
    [SerializeField] protected Transform starContainer;
    [SerializeField] protected UnityEngine.UI.Image charImage;
    [SerializeField] Animator charAnim;
    [SerializeField] Animator[] equipmentAnimators;
    [SerializeField] protected GameObject equippedText; // 카드가 장착이 되어있는지(오리/장비 모두)
    [SerializeField] protected TMPro.TextMeshProUGUI Level;
    [SerializeField] protected GameObject starPrefab;
    [SerializeField] protected bool displayEquippedText; // 착용 중 표시를 할지 말지 여부
    GameObject[] stars;

    public void InitWeaponCardDisplay(WeaponData weaponData, Animator[] equipAnims, bool onEquipment)
    {
        EmptyCardDisplay();
        // 별과 카드 색깔
        cardBaseContainer.gameObject.SetActive(true);

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
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = weaponData.CardCharAnimator.CardImageAnim;

        // 장비 이미지
        for (int i = 0; i < 4; i++)
        {
            if (equipAnims[i] == null)
                continue;

            equipmentAnimators[i].gameObject.SetActive(true);
            equipmentAnimators[i].runtimeAnimatorController = equipAnims[i].runtimeAnimatorController;
        }

        // Launch 슬롯이 아닐 때만 착용 중 표시
        // 업슬롯, 맷슬롯은 착용 중 표시 하지 않음
        // 장비 슬롯은 착용 중 표시
        if (displayEquippedText)
        {
            SetEquppiedTextActive(onEquipment);
        }
    }

    public void InitItemCardDisplay(Item itemData, bool onEquipment)
    {
        // 별과 카드 색깔
        cardBaseContainer.gameObject.SetActive(true);
        int intGrade = (int)itemData.grade;
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
        charImage.sprite = itemData.charImage;


        if (displayEquippedText)
        {
            SetEquppiedTextActive(onEquipment);
        }
    }

    protected virtual void SetNumStar(int numStars)
    {
        if (stars == null)
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

    public Animator[] GetEquipmentAnimators()
    {
        return equipmentAnimators;
    }

    public void SetEquppiedTextActive(bool _isActive)
    {
        equippedText.SetActive(_isActive);
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
        cardBaseContainer.gameObject.SetActive(false);

        // 장비 이미지
        for (int i = 0; i < 4; i++)
        {
            if (equipmentAnimators[i] == null)
                continue;
            equipmentAnimators[i].gameObject.SetActive(false);
        }

        SetEquppiedTextActive(false);
    }
}
