using UnityEngine;
using UnityEngine.UI;

// equipped text는 카드슬롯이 생성될 때 정해짐. 
// 장착되거나 해제되면 Equipment Panel Manager에서 업데이트
// 지금은 Instantiate로 카드를 생성하고 destroy 하니까 상관없지만
// 오브젝트 풀을 사용하면 비활성화 할 때 모든 animator=null, setactive=false 로 해야 함
public class CardDisp : MonoBehaviour
{
    [SerializeField] protected Transform cardBaseContainer; // 등급 5개
    [SerializeField] protected Transform starContainer;
    [SerializeField] protected UnityEngine.UI.Image charImage;
    [SerializeField] protected UnityEngine.UI.Image charFaceImage;
    [SerializeField] protected GameObject charFaceExpression;
    [SerializeField] Animator charAnim;
    [SerializeField] Animator[] equipmentAnimators;
    [SerializeField] Image[] equipmentImages;
    [SerializeField] protected GameObject equippedText; // 카드가 장착이 되어있는지(오리/장비 모두)
    [SerializeField] protected TMPro.TextMeshProUGUI Title;
    [SerializeField] protected TMPro.TextMeshProUGUI TitleShadow;
    [SerializeField] protected TMPro.TextMeshProUGUI Level;

    [SerializeField] protected GameObject starPrefab;
    [SerializeField] protected bool displayEquippedText; // 착용 중 표시를 할지 말지 여부. 인스펙터 창에서 설정
    [SerializeField] GameObject button; // 버튼을 활성, 비활성 하기 위해
    [SerializeField] GameObject haloSelected; // 선택된 카드 주변 Halo
    [SerializeField] GameObject leadTag; // 리드 오리 태그

    [Header("MergedCard")]
    [SerializeField] bool isMergedCard; // 합성된 카드일 때만 타이틀 리본을 보여주기 위해
    [SerializeField] Transform ribbon;
    GameObject[] stars;
    MergedCardDescription mergedCardDescription;

    public void InitWeaponCardDisplay(WeaponData weaponData, CardData cardData)
    {
        // 리드오리 태그
        leadTag.gameObject.SetActive(false);
        if(cardData.StartingMember == StartingMember.Zero.ToString())
        {
            leadTag.gameObject.SetActive(true);
        }

        // 별과 카드 색깔
        cardBaseContainer.gameObject.SetActive(true);

        int intGrade = (int)weaponData.grade;
        int evoStage = cardData.EvoStage;
        SetNumStar(evoStage + 1);

        // 등급에 따른 카드 색깔
        for (int i = 0; i < StaticValues.MaxGrade; i++)
        {
            cardBaseContainer.GetChild(i).gameObject.SetActive(false);
        }
        cardBaseContainer.GetChild(intGrade).gameObject.SetActive(true);

        if (isMergedCard)
        {
            // 타이틀 리본 색깔
            for (int i = 0; i < 5; i++)
            {
                if (i == intGrade)
                {
                    cardBaseContainer.GetChild(intGrade).gameObject.SetActive(true);
                    ribbon.GetChild(intGrade).gameObject.SetActive(true);
                    continue;
                }
                cardBaseContainer.GetChild(i).gameObject.SetActive(false);
                ribbon.GetChild(i).gameObject.SetActive(false);
            }

            ribbon.gameObject.SetActive(true);

            // 카드 이름 텍스트
            Title.text = weaponData.DisplayName;
            TitleShadow.text = Title.text;

            if(mergedCardDescription == null) mergedCardDescription = GetComponent<MergedCardDescription>();
            mergedCardDescription.UpdateSkillDescription(cardData);
        }

        // 카드 레벨 텍스트
        Level.text = "레벨 " + cardData.Level;

        // 캐릭터 이미지
        //charImage.sprite = weaponData.charImage;
        charAnim.enabled = true;
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = weaponData.Animators.CardImageAnim;
        charFaceExpression.gameObject.SetActive(true);
        if(charFaceImage == null) charFaceImage = charFaceExpression.GetComponent<Image>();
        charFaceImage.sprite = weaponData.faceImage;

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
        int intEvoStage = cardData.EvoStage;
        SetNumStar(intEvoStage + 1);
        for (int i = 0; i < StaticValues.MaxGrade; i++)
        {
            cardBaseContainer.GetChild(i).gameObject.SetActive(false);
        }
        cardBaseContainer.GetChild(intGrade).gameObject.SetActive(true);

        // 카드 이름 텍스트
        Title.text= itemData.Name;
        TitleShadow.text = Title.text;

        // 임시로 타이틀을 없애보자. 작은 카드 안에 정보가 너무 많음.
        Title.text = "";
        TitleShadow.text = "";

        // 카드 레벨 텍스트
        Level.text = "레벨 " + cardData.Level;

        // 아이템 이미지
        // int index = new Convert().EquipmentTypeToInt(cardData.EquipmentType);
        // equipmentAnimators[index].gameObject.SetActive(true);
        // equipmentAnimators[index].runtimeAnimatorController = itemData.CardItemAnimator.CardImageAnim;
        // equipmentAnimators[index].SetTrigger("Card");
        charImage.gameObject.SetActive(true);
        charImage.sprite = itemData.charImage;
        charAnim.enabled = false;
        charFaceExpression.gameObject.SetActive(false);



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
    public void SetEquipCardImage(int index, Sprite equipmentImage)
    {
        if (equipmentImage == null)
        {
            equipmentImages[index].gameObject.SetActive(false);
            return;
        }
        equipmentImages[index].gameObject.SetActive(true);
        equipmentImages[index].sprite = equipmentImage;
    }
    protected virtual void SetNumStar(int numStars)
    {
        if (stars == null)
        {
            // 최대 합성 레벨만큼 만들어서 비활성화
            stars = new GameObject[StaticValues.MaxEvoStage];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Instantiate(starPrefab, starContainer);
                stars[i].SetActive(false);
            }
        }

        // 일단 모든 별을 비활성화. 많은 별에서 적은 별로 업데이트 하면 많은 별로 남아있기 때문
        for (int i = 0; i < StaticValues.MaxEvoStage; i++)
        {
            stars[i].SetActive(false);
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

    public void SetHalo(bool _isActive)
    {
        if (haloSelected == null) return;
        haloSelected.SetActive(_isActive);
    }
    public void EmptyCardDisplay()
    {
        // 별 비활성화
        DeactivateStars();

        // 카드 레벨 텍스트
        Level.text = "";
        Title.text = "";
        TitleShadow.text = "";
        if(isMergedCard) ribbon.gameObject.SetActive(false);

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

    void DeactivateStars()
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
    }
}
