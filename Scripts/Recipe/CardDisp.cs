using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// equipped text는 카드슬롯이 생성될 때 정해짐. 
// 장착되거나 해제되면 Equipment Panel Manager에서 업데이트
// 지금은 Instantiate로 카드를 생성하고 destroy 하니까 상관없지만
// 오브젝트 풀을 사용하면 비활성화 할 때 모든 animator=null, setactive=false 로 해야 함
public class CardDisp : MonoBehaviour, IEquipSpriteAnim
{
    [SerializeField] protected Transform cardBaseContainer; // 등급 5개
    [SerializeField] protected Transform starContainer;
    [SerializeField] protected Image charImage;
    [SerializeField] protected Image charFaceImage;
    [SerializeField] protected GameObject charFaceExpression;
    [SerializeField] Animator charAnim;
    [SerializeField] Animator[] equipmentAnimators;
    [SerializeField] Image[] equipmentImages;
    [SerializeField] RectTransform headMain;
    bool needToOffset;
    [SerializeField] protected GameObject equippedText; // 카드가 장착이 되어있는지(오리/장비 모두)
    [SerializeField] protected TMPro.TextMeshProUGUI Title;
    [SerializeField] protected TMPro.TextMeshProUGUI TitleShadow;
    [SerializeField] protected TMPro.TextMeshProUGUI Level;

    [SerializeField] protected GameObject starPrefab;
    [SerializeField] protected bool displayEquippedText; // 착용 중 표시를 할지 말지 여부. 인스펙터 창에서 설정
    [SerializeField] GameObject button; // 버튼을 활성, 비활성 하기 위해
    [SerializeField] GameObject haloSelected; // 선택된 카드 주변 Halo
    [SerializeField] GameObject leadTag; // 리드 오리 태그

    CardSpriteAnim cardSpriteAnim;

    [Header("MergedCard")]
    [SerializeField] bool isMergedCard; // 합성된 카드일 때만 타이틀 리본을 보여주기 위해
    [SerializeField] Transform ribbon;
    GameObject[] stars;
    MergedCardDescription mergedCardDescription;


    void Update()
    {
        if(charAnim.gameObject.activeSelf) return;
        // Debug.Log($"char anim이 비활성화 되었습니다.");
    }

    public void InitWeaponCardDisplay(WeaponData weaponData, CardData cardData)
    {
        needToOffset = false;

        // 캐릭터 이미지
        //charImage.sprite = weaponData.charImage;
        charAnim.enabled = true;
        charAnim.gameObject.SetActive(true);
        Debug.Log($"{weaponData.DisplayName}의 char Image가 활성화 되었습니다.");
        charAnim.runtimeAnimatorController = weaponData.Animators.CardImageAnim;
        charFaceExpression.gameObject.SetActive(true);
        if (charFaceImage == null) charFaceImage = charFaceExpression.GetComponent<Image>();
        charFaceImage.sprite = weaponData.faceImage;

        // 데이터로 카드를 display할 때가 아닌 경우라면 여기까지만 진행
        if (cardData == null)
            return;

        // 리드오리 태그
        if (leadTag != null)
        {
            leadTag.gameObject.SetActive(false);
            if (cardData.StartingMember == StartingMember.Zero.ToString())
            {
                // Debug.Log($"{cardData.Name}의 Starting Member 값은 {cardData.StartingMember}입니다.");
                leadTag.gameObject.SetActive(true);
            }
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

            if (mergedCardDescription == null) mergedCardDescription = GetComponent<MergedCardDescription>();
            mergedCardDescription.UpdateSkillDescription(cardData);
        }

        // 카드 레벨 텍스트
        Level.text = "레벨 " + cardData.Level;

        // 오리카드는 착용 중 표시 안 함
        // 장비카드만 착용 중 표시
        SetEquppiedTextActive(false);

        // 버튼 활성화
        button.SetActive(true);
    }

    public void InitItemCardDisplay(Item itemData, CardData cardData, bool onEquipment)
    {
        // 리드오리 태그
        if (leadTag != null)
        {
            leadTag.gameObject.SetActive(false);
        }

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
        Title.text = itemData.Name;
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

    #region Card Sprite Anim 참조
    public void InitSpriteRow()
    {
        if (cardSpriteAnim == null) cardSpriteAnim = GetComponentInChildren<CardSpriteAnim>();
        cardSpriteAnim.Init(equipmentImages);
    }
    public void SetEquipCardDisplay(int index, SpriteRow spriteRow, bool needToOffset, Vector2 offset)
    {
        // need to offset이 참이 되면 더 이상 변화가 없도록 남겨둠
        this.needToOffset = this.needToOffset ? true : needToOffset;

        // offset을 하게 하는 아이템이 탈착 되었을 때를 위한 초기화
        headMain.anchoredPosition = this.needToOffset == false ? Vector2.zero : headMain.anchoredPosition;

        // cardSpriteAnim.Init을 호출해서 해당 index 부위의 애니메이션 이미지들을 저장해 두기
        if (spriteRow == null)
        {
            equipmentImages[index].gameObject.SetActive(false);
        }
        else
        {
            equipmentImages[index].gameObject.SetActive(true);

            // 탈착, 장착이 반복될 수록 계속 offset이 더해지는 것을 막기 위해 원점이 아닐 때는 offset을 해주지 않기
            headMain.anchoredPosition = headMain.anchoredPosition == Vector2.zero ? headMain.anchoredPosition + offset : headMain.anchoredPosition;
            cardSpriteAnim.StoreItemSpriteRow(index, spriteRow); // 이미지들을 저장해 두고 애니메이션 이벤트로 사용
        }
    }
    #endregion

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
        if(equippedText == null) return;
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
        if (isMergedCard) ribbon.gameObject.SetActive(false);

        // 캐릭터 이미지
        cardBaseContainer.gameObject.SetActive(false);
        charImage.gameObject.SetActive(false);
        Debug.Log("char Image를 비활성화 했습니다.");

        // 장비 이미지
        for (int i = 0; i < 4; i++)
        {
            if (equipmentAnimators[i] == null)
                continue;
            equipmentAnimators[i].gameObject.SetActive(false);
        }

        SetEquppiedTextActive(false);

        // 버튼 비활성화
        if(button == null) return;
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
