using System.Collections.Generic;
using UnityEngine;

// equipped text는 카드슬롯이 생성될 때 정해짐. 
// 장착되거나 해제되면 Equipment Panel Manager에서 업데이트
// 지금은 Instantiate로 카드를 생성하고 destroy 하니까 상관없지만
// 오브젝트 풀을 사용하면 비활성화 할 때 모든 animator=null, setactive=false 로 해야 함
public class CardDisp : MonoBehaviour
{
    [SerializeField] protected Transform cardBaseContainer; // 등급 5개
    [SerializeField] protected Transform starContainer;
    [SerializeField] protected UnityEngine.UI.Image charImage;
    [SerializeField] Animator charAnim;
    [SerializeField] Animator[] equipmentAnimators;
    [SerializeField] protected GameObject equippedText; // 카드가 장착이 되어있는지(오리/장비 모두)
    [SerializeField] protected TMPro.TextMeshProUGUI Title;
    [SerializeField] protected TMPro.TextMeshProUGUI Level;
    [SerializeField] protected GameObject starPrefab;
    [SerializeField] protected bool displayEquippedText; // 착용 중 표시를 할지 말지 여부. 인스펙터 창에서 설정
    [SerializeField] GameObject button; // 버튼을 활성, 비활성 하기 위해
    GameObject[] stars;

    public void InitWeaponCardDisplay(WeaponData weaponData)
    {
        // 별과 카드 색깔
        cardBaseContainer.gameObject.SetActive(true);

        int intGrade = (int)weaponData.grade;
        SetNumStar(intGrade + 1);

        for (int i = 0; i < 5; i++)
        {
            cardBaseContainer.GetChild(i).gameObject.SetActive(false);
        }
        cardBaseContainer.GetChild(intGrade).gameObject.SetActive(true);

        // 카드 이름 텍스트
        Title.text = weaponData.Name;
        // 카드 레벨 텍스트
        Level.text = "LV1";

        // 캐릭터 이미지
        //charImage.sprite = weaponData.charImage;
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = weaponData.Animators.CardImageAnim;

        // 오리카드는 착용 중 표시 안 함
        // 장비카드만 착용 중 표시
        SetEquppiedTextActive(false);

        // 버튼 활성화
        button.SetActive(true);
    }

    public void InitItemCardDisplay(Item itemData, CardData cardData, bool onEquipment)
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

        // 카드 이름 텍스트
        Title.text= itemData.Name;

        // 카드 레벨 텍스트
        Level.text = "LV1";

        // 아이템 이미지
        int index = new Convert().EquipmentTypeToInt(cardData.EquipmentType);
        equipmentAnimators[index].gameObject.SetActive(true);
        equipmentAnimators[index].runtimeAnimatorController = itemData.CardItemAnimator.CardImageAnim;
        equipmentAnimators[index].SetTrigger("Card");

        if (displayEquippedText)
        {
            SetEquppiedTextActive(onEquipment);
        }

        // 버튼 활성화
        button.SetActive(true);
    }

    public void SetRunTimeAnimController(int index, RuntimeAnimatorController animatorController)
    {
        equipmentAnimators[index].gameObject.SetActive(true);
        equipmentAnimators[index].runtimeAnimatorController = animatorController;
        if (animatorController == null)
        { 
            equipmentAnimators[index].gameObject.SetActive(false); 
        }
        charAnim.Rebind();
        for (int i = 0; i < 4; i++)
        {
            if(equipmentAnimators[i].gameObject.activeSelf)
            {
                equipmentAnimators[i].Rebind();
            }
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
        Title.text = "";

        // 캐릭터 이미지
        cardBaseContainer.gameObject.SetActive(false);
        charImage.gameObject.SetActive(false);

        // 장비 이미지
        for (int i = 0; i < 4; i++)
        {
            if (equipmentAnimators[i] == null)
                continue;
            equipmentAnimators[i].gameObject.SetActive(false);
        }

        SetEquppiedTextActive(false);

        // 버튼 비활성화
        button.SetActive(false);
    }
}
