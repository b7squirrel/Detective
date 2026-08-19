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
    [SerializeField] GameObject additionalSkillPanel; // ★ New Card Stats > Additional Skill 오브젝트 연결
    GameObject[] stars;
    MergedCardDescription mergedCardDescription;

    [Header("Ribbon Auto Width")]
    [SerializeField] float ribbonPadding = 60f;  // 리본 양 끝 장식 여백 (실제로 보면서 조절)
    [SerializeField] float ribbonMinWidth = 200f; // 텍스트가 짧을 때 리본 최소 너비

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

        // ★ 언어가 바뀌면 텍스트 길이도 바뀌므로 리본 너비도 다시 계산
        UpdateRibbonWidth();
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
        UpdateRibbonWidth(); // ★ 리본 너비를 타이틀 텍스트에 맞게 조절

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

            // ★ 무기 카드는 스킬 설명 패널 표시
            if (additionalSkillPanel != null) additionalSkillPanel.SetActive(true);

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
        UpdateRibbonWidth(); // ★ 리본 너비를 타이틀 텍스트에 맞게 조절

        // ★ 아이템은 스킬이 없으므로 스킬 설명 패널 숨김
        if (additionalSkillPanel != null) additionalSkillPanel.SetActive(false);

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

        // ★ 오리카드(isMergedCard)와 동일하게 리본 처리
        if (isMergedCard)
        {
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
        }

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

    // ★ 리본 너비를 Title 텍스트 길이에 맞게 자동 조절
    void UpdateRibbonWidth()
    {
        if (ribbon == null || Title == null) return;

        RectTransform ribbonRect = ribbon as RectTransform;
        if (ribbonRect == null) return;

        // 현재 폭 제약 없이 텍스트가 필요로 하는 실제 너비 계산
        Vector2 preferredSize = Title.GetPreferredValues(Title.text, 0, 0);

        float newWidth = Mathf.Max(preferredSize.x + ribbonPadding, ribbonMinWidth);
        ribbonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }

    protected virtual void SetNumStar(int numStars)
    {
        Debug.Log($"[CardDisp:{gameObject.name}] SetNumStar 호출됨. numStars={numStars}, InstanceID={GetInstanceID()}");
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

    // ⭐ 추가: 슬롯이 비어있어도 클릭은 가능해야 하는 경우(예: 로비 리드/동료 슬롯)를 위해
    // 버튼 활성 상태만 별도로 강제 설정. EmptyCardDisplay()가 button도 함께 꺼버리므로,
    // 빈 상태에서도 탭 가능하게 하려면 EmptyCardDisplay() 이후에 이 메서드로 다시 켜줘야 함.
    public void SetButtonActive(bool active)
    {
        if (button == null) return;
        button.SetActive(active);
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

        if (additionalSkillPanel != null) additionalSkillPanel.SetActive(false); // ★ 카드 비울 때 스킬 패널도 숨김

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