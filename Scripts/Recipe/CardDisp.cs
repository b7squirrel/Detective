using UnityEngine;
using UnityEngine.UI;

public class CardDisp : MonoBehaviour, IEquipSpriteAnim
{
    [SerializeField] protected Transform cardBaseContainer;
    [SerializeField] protected Transform starContainer;
    [SerializeField] protected Image charImage;
    [SerializeField] protected Image charFaceImage;
    [SerializeField] protected GameObject charFaceExpression;
    [SerializeField] Animator charAnim;
    [SerializeField] Image[] equipmentImages;
    [SerializeField] Sprite emptyEquipment;
    [SerializeField] RectTransform headMain;
    bool needToOffset;
    [SerializeField] protected GameObject equippedText;
    [SerializeField] protected TMPro.TextMeshProUGUI Title;
    [SerializeField] protected TMPro.TextMeshProUGUI Level;

    [SerializeField] protected GameObject starPrefab;
    [SerializeField] protected bool displayEquippedText;
    [SerializeField] GameObject button;
    [SerializeField] GameObject haloSelected;
    [SerializeField] GameObject leadTag;

    CardSpriteAnim cardSpriteAnim;

    [Header("MergedCard")]
    [SerializeField] bool isMergedCard;
    [SerializeField] Transform ribbon;
    GameObject[] stars;
    MergedCardDescription mergedCardDescription;

    // ★ 현재 표시 중인 데이터 저장
    private WeaponData currentWeaponData;
    private Item currentItemData;
    private CardData currentCardData;
    private bool isWeaponCard; // true: 무기 카드, false: 아이템 카드

    // ★ 언어 변경 이벤트 구독
    void Awake()
    {
        LocalizationManager.OnLanguageChanged += UpdateText;
    }

    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    // ★ 텍스트 업데이트 메서드
    void UpdateText()
    {
        if (isWeaponCard && currentWeaponData != null)
        {
            // 무기 카드 텍스트 업데이트
            Title.text = LocalizationManager.Char.GetWeaponDisplayName(currentWeaponData.Name);
            
            if (currentCardData != null)
            {
                Level.text = LocalizationManager.Game.level + " " + currentCardData.Level;
            }
        }
        else if (!isWeaponCard && currentItemData != null)
        {
            // 아이템 카드 텍스트 업데이트
            Title.text = LocalizationManager.Item.GetItemDisplayName(currentItemData.Name);
            
            if (currentCardData != null)
            {
                Level.text = LocalizationManager.Game.level + " " + currentCardData.Level;
            }
        }
    }

    public void InitWeaponCardDisplay(WeaponData weaponData, CardData cardData)
    {
        // ★ 현재 데이터 저장
        currentWeaponData = weaponData;
        currentCardData = cardData;
        isWeaponCard = true;

        needToOffset = false;

        // 캐릭터 이미지
        charAnim.enabled = true;
        charAnim.gameObject.SetActive(true);
        charAnim.runtimeAnimatorController = weaponData.Animators.CardImageAnim;
        charFaceExpression.SetActive(true);
        if (charFaceImage == null) charFaceImage = charFaceExpression.GetComponent<Image>();
        charFaceImage.sprite = weaponData.faceImage;
        charImage.SetNativeSize();

        Level.text = "";

        // ★ 다국어 적용
        Title.text = LocalizationManager.Char.GetWeaponDisplayName(weaponData.Name);

        // 데이터로 카드를 display할 때가 아닌 경우라면 여기까지만 진행
        if (cardData == null) return;

        // 리드오리 태그
        SetLeadTagActive(false);
        if (cardData.StartingMember == StartingMember.Zero.ToString())
        {
            SetLeadTagActive(true);
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

            if (mergedCardDescription == null) mergedCardDescription = GetComponent<MergedCardDescription>();
            mergedCardDescription.UpdateSkillDescription(cardData);
        }

        // ★ 다국어 적용
        Level.text = LocalizationManager.Game.level + " " + cardData.Level;

        // 오리카드는 착용 중 표시 안 함
        SetEquppiedTextActive(false);

        // 버튼 활성화
        if(button != null) button.SetActive(true);
    }

    public void InitItemCardDisplay(Item itemData, CardData cardData, bool onEquipment)
    {
        // ★ 현재 데이터 저장
        currentItemData = itemData;
        currentCardData = cardData;
        isWeaponCard = false;

        // 리드오리 태그
        SetLeadTagActive(false);

        // ★ 다국어 적용
        Title.text = LocalizationManager.Item.GetItemDisplayName(itemData.Name);

        charImage.gameObject.SetActive(true);
        charImage.sprite = itemData.charImage;
        charImage.SetNativeSize();
        charImage.rectTransform.localScale = 1f * Vector3.one;
        charAnim.enabled = false;
        charFaceExpression.gameObject.SetActive(false);

        // 데이터로 카드를 display할 때가 아닌 경우라면 여기까지만 진행
        if (cardData == null) return;

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

        // ★ 다국어 적용
        Level.text = LocalizationManager.Game.level + " " + cardData.Level;

        // 임시로 타이틀을 없애보자. 작은 카드 안에 정보가 너무 많음.
        // Title.text = "";

        if (displayEquippedText) SetEquppiedTextActive(onEquipment);

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
        this.needToOffset = this.needToOffset ? true : needToOffset;
        headMain.anchoredPosition = this.needToOffset == false ? Vector2.zero : headMain.anchoredPosition;

        if (spriteRow == null)
        {
            equipmentImages[index].sprite = emptyEquipment;
            equipmentImages[index].SetNativeSize();
            equipmentImages[index].gameObject.SetActive(false);
        }
        else
        {
            equipmentImages[index].gameObject.SetActive(true);
            headMain.anchoredPosition = headMain.anchoredPosition == Vector2.zero ? headMain.anchoredPosition + offset : headMain.anchoredPosition;
            cardSpriteAnim.StoreItemSpriteRow(index, spriteRow);
        }
    }
    #endregion

    protected virtual void SetNumStar(int numStars)
    {
        if (stars == null)
        {
            stars = new GameObject[StaticValues.MaxEvoStage];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = Instantiate(starPrefab, starContainer);
                stars[i].SetActive(false);
            }
        }

        for (int i = 0; i < StaticValues.MaxEvoStage; i++)
        {
            stars[i].SetActive(false);
        }

        for (int i = 0; i < numStars; i++)
        {
            stars[i].SetActive(true);
        }
    }

    void SetLeadTagActive(bool active)
    {
        if (leadTag != null) leadTag.SetActive(active);
    }

    public void SetEquppiedTextActive(bool _isActive)
    {
        if (equippedText == null) return;
        equippedText.SetActive(_isActive);
    }

    public void SetHalo(bool _isActive)
    {
        if (haloSelected == null) return;
        haloSelected.SetActive(_isActive);
    }
    
    public void EmptyCardDisplay()
    {
        // ★ 데이터 초기화
        currentWeaponData = null;
        currentItemData = null;
        currentCardData = null;

        DeactivateStars();

        Level.text = "";
        Title.text = "";
        if (isMergedCard) ribbon.gameObject.SetActive(false);

        if (cardBaseContainer != null) cardBaseContainer.gameObject.SetActive(false);
        if (charImage != null) charImage.gameObject.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            if (equipmentImages[i] == null)
                continue;
            equipmentImages[i].gameObject.SetActive(false);
        }

        SetEquppiedTextActive(false);

        if(button == null) return;
        button.SetActive(false);
    }

    void DeactivateStars()
    {
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