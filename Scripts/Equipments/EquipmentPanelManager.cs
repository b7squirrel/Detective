using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

public class EquipmentPanelManager : MonoBehaviour
{
    CardData CardOnDisplay { get; set; } // 디스플레이에 올라가 있는 오리 카드
    [SerializeField] CardData cardToEquip; // Equipment Info에 올라 갈 장비 카드

    int index; // 어떤 장비 슬롯인지
    bool isEquipped; // 장비 정보창에 띄워진 장비가 착용중인지 아닌지 판단. 레벨업을 할 때 오리에게 attr을 적용할지 말지를 결정하기 위해

    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    EquipmentSlotsManager equipmentSlotsManager;
    CardList cardList;
    StatManager statManager;
    CardDisp cardDisp; // Equip info panel이 활성화 되면 클릭한 카드의 disp클래스를 저장(equipped Text 표시를 위해)
    SetCardDataOnSlot setCardDataOnSlot;

    EquipDisplayUI equipDisplayUI;
    [SerializeField] EquipInfoPanel equipInfoPanel;
    [SerializeField] AllField field; // 모든 카드

    [Header("Equipment Slots")]
    PlayerDataManager playerDataManager;
    [SerializeField] TMPro.TextMeshProUGUI upgradeCost;
    [SerializeField] GameObject itemMaxLevel;
    [SerializeField] GameObject itemUpgradeText; // 최고 레벨일 때 업그레이드 텍스트 숨기기
    [SerializeField] CanvasGroup warningLackCanvasGroup; // 아이템 업그레이드 코인 부족 경고 메시지
    [SerializeField] Button upgradeButton;
    [SerializeField] GameObject EquipCoinImage;
    Tween warningLack;
    Tween warningMax;

    [Header("Char Card Slot")]
    [SerializeField] CardSlot oriSlot;
    [SerializeField] TMPro.TextMeshProUGUI charUpgradeCost;
    [SerializeField] GameObject charMaxLevel;
    [SerializeField] GameObject charUpgradeText; // 최고 레벨일 때 업그레이드 텍스트 숨기기
    [SerializeField] CanvasGroup charWarningLackCanvasGroup; // 오리 업그레이드 코인 부족 경고 메시지
    [SerializeField] Button charUpgradeButton;
    [SerializeField] GameObject CharCoinImage;
    [SerializeField] GameObject GearBGToExitField;
    [SerializeField] GameObject GearBGToExitInfo;
    Tween charWarningLack;
    Tween charWarningMax;
    float textOffset = 23.5f;
    Coroutine hideCoroutine;

    CardSlotManager cardSlotManager; // 카드 슬롯 풀

    [SerializeField] AudioClip maxLevelSound;

    void Awake()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        equipDisplayUI = GetComponentInChildren<EquipDisplayUI>();
        setCardDataOnSlot = GetComponent<SetCardDataOnSlot>();
        cardList = FindAnyObjectByType<CardList>();
        cardDictionary = FindAnyObjectByType<CardsDictionary>();
        equipmentSlotsManager = GetComponent<EquipmentSlotsManager>();
        statManager = FindAnyObjectByType<StatManager>();

        cardToEquip = null;

        playerDataManager = FindObjectOfType<PlayerDataManager>();


        warningLackCanvasGroup.alpha = 0;
        charWarningLackCanvasGroup.alpha = 0;
    }

    void OnEnable()
    {
        cardToEquip = null;
        SetAllFieldTypeOf("Weapon");
        DeActivateEquipInfoPanel();
        CardOnDisplay = null;
        // // 패널에 진입하면 기본적으로 리드 오리를 디스플레이 하고 있도록
        // CardOnDisplay = cardDataManager.GetMyCardList().Find(x => x.StartingMember == StartingMember.Zero.ToString());
        // InitDisplay(CardOnDisplay);
        // Debug.Log($"Lead Card Data Name = {CardOnDisplay.Name}");
        // SetAllFieldTypeOf("Item");

        charUpgradeButton.gameObject.SetActive(false);
        ClearAllEquipmentSlots(); // logic, UI 모두 처리

        if (hideCoroutine != null) StopCoroutine(hideCoroutine); // 경고 메시지 표시 도중 패널을 나갔다면, 돌아왔을 때 메시지를 없애기

        warningLackCanvasGroup.alpha = 0;
        charWarningLackCanvasGroup.alpha = 0;
    }

    // 장비 필드에서 오리 카드를 클릭하면 equip Slot Action에서 호출
    // 오리 카드를 equip display에 보여준다
    public void InitDisplay(CardData oriCardDataToDisplay)
    {
        GearBGToExitField.SetActive(true); // 백 버튼, bg to exit field 버튼, 디스플레이된 오리카드 버튼을 눌렀을 때 비활성화
        equipDisplayUI.OnDisplay(oriCardDataToDisplay); // 디스플레이 활성
        CardOnDisplay = oriCardDataToDisplay; // 디스플레이 되는 카드의 card data
        equipmentSlotsManager.InitEquipSlots(oriCardDataToDisplay); // 오리 카드의 Data대로 장비 슬롯 설정 
        equipDisplayUI.SetWeaponDisplay(oriCardDataToDisplay, 
            equipmentSlotsManager.GetCurrentAttribute(), 
            cardDictionary.GetDisplayName(oriCardDataToDisplay)); // 오리 카드 및 Attr

        // 나중에는 이 항목들을 EquipDispUI에 옮겨야 한다. SetWeaponDisplay에 포함되도록
        int level = CardOnDisplay.Level;
        UpdateUpgradeCost(level, charUpgradeCost);
        UpdateButtonState(charUpgradeButton, true);

        setCardDataOnSlot.PutCardDataIntoSlot(oriCardDataToDisplay, oriSlot);

        isEquipped = false;
        Debug.Log("Card on Display = " + CardOnDisplay.Name);

    }

    public void SetAllFieldTypeOf(string cardType)
    {
        cardToEquip = null;

        List<CardData> card = new();

        string fieldAnimTrigger = cardType == "Weapon" ? "EquipW" : "EquipI";

        // 아이템 카드는 착용되어 있지 않는 것들만 보여주기
        if (cardType == CardType.Weapon.ToString())
        {
            ClearAllEquipmentSlots(); // logic, UI 모두 처리

            card = cardDataManager.GetMyCardList().FindAll(x => x.Type == cardType); // field에는 오리만 보여줌
                                                                                     //card.Remove(CardOnDisplay);
        }
        else if(cardType == CardType.Item.ToString())
        {
            foreach (var item in cardList.GetEquipmentCardsList())
            {
                if (item.IsEquipped) // 다른 오리에 장착된 카드는 보여주지 않음
                {
                    continue;
                    // 장착된 카드일 경우
                    // 카드에 있는 반투명 Equipped 활성화 시키기
                    // slot type 추가 : Equipped
                    // Equipped 카드는 터치하면 장착을 해제할 것인지 팝업을 띄움
                    // 장착에서는 아이템만 Equipped 처리하면 됨
                    // 장착해제를 선택하면 장착되어 있던 오리에게서 장착해제가 됨
                }

                // 범용이거나 해당 오리에 바인딩 되어 있는 장비라면 필드에 추가
                if (item.CardData.BindingTo == "All")
                {
                    // 범용이어도 필수 장비 슬롯과 겹치면서 해당 오리에 바인딩 되어 있지 않다면 빼기
                    if (item.CardData.EquipmentType == CardOnDisplay.EssentialEquip
                        && item.CardData.BindingTo != CardOnDisplay.Name)
                        continue;
                    card.Add(item.CardData);
                    continue;
                }
                if (item.CardData.BindingTo == CardOnDisplay.Name)
                {
                    card.Add(item.CardData);
                    continue;
                }
            }
        }

        field.GenerateAllCardsOfType(card, "Equip");

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim(fieldAnimTrigger); // 오리 혹은 아이템 필드를 보여주기.
    }
    void ClearAllEquipmentSlots()
    {
        // Display의 장비 슬롯들을 모두 비우기
        equipmentSlotsManager.ClearEquipSlots(); // logic
        equipDisplayUI.OffDisplay(); // UI
        oriSlot.EmptySlot(); // 슬롯 비활성화
    }

    // info panel 의 equip 버튼
    public void OnEquipButton()
    {
        // 디스플레이 되는 charCard의 equipments
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(CardOnDisplay);

        // 장착하려는 장비 부위에 이미 다른 장비가 장착되어 있다면 CardList에서 그 장비를 해제하고
        if (equipmentSlotsManager.IsEmpty(index) == false)
        {
            Debug.Log("장비가 이미 있습니다. 교체합니다.");
            cardList.UnEquip(CardOnDisplay, equipmentCards[index]);
            equipmentSlotsManager.EmptyEquipSlot(index);
        }

        // 새로운 장비 장착
        cardList.Equip(CardOnDisplay, cardToEquip);
        Item itemData = cardDictionary.GetWeaponItemData(cardToEquip).itemData;
        equipmentSlotsManager.SetEquipSlot(index, itemData, cardToEquip);

        SetAllFieldTypeOf("Item");
        setCardDataOnSlot.PutCardDataIntoSlot(CardOnDisplay, oriSlot); // 오리 디스플레이 갱신

        UpdateCardSlotOfPool(CardOnDisplay); // 카드 슬롯 풀의 슬로 그림도 업데이트

        cardList.DelayedSaveEquipments();
        DeActivateEquipInfoPanel();
    }

    // public void EquipCard(CardData oriCard, CardData equipCard)
    // {
    //     cardList.Equip(oriCard, equipCard);
    //     cardList.DelayedSaveEquipments();
    // }

    // info panel의 UnEquip 버튼
    public void OnUnEquipButton()
    {
        // 장비 해제
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(CardOnDisplay);
        cardList.UnEquip(CardOnDisplay, equipmentCards[index]);
        cardList.DelayedSaveEquipments();

        equipmentSlotsManager.EmptyEquipSlot(index);

        cardToEquip = null;

        cardDisp.SetEquppiedTextActive(false);
        SetAllFieldTypeOf("Item");
        setCardDataOnSlot.PutCardDataIntoSlot(CardOnDisplay, oriSlot); // 오리 디스플레이 갱신

        UpdateCardSlotOfPool(CardOnDisplay); // 카드 슬롯 풀의 슬로 그림도 업데이트
        
        DeActivateEquipInfoPanel();
    }

    // 카드 슬롯 풀의 그림도 업데이트
    void UpdateCardSlotOfPool(CardData cardData)
    {
        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.UpdateCardDisplay(CardOnDisplay);
    }

    // equip slot action 에서 호출
    public void ActivateEquipInfoPanel(CardData itemCardData, CardDisp cardDisp, bool isEquipButton, EquipmentType equipType)
    {
        GearBGToExitInfo.SetActive(true); // x 버튼, bg to exit info버튼으로 비활성화
        index = new Convert().EquipmentTypeToInt(itemCardData.EquipmentType);
        isEquipped = !isEquipButton; // equip button을 띄운다는 것은 field에 있는 장비 카드라는 뜻이므로

        Item iData = cardDictionary.GetWeaponItemData(itemCardData).itemData;

        equipInfoPanel.gameObject.SetActive(true);
        cardSlotManager.SettrigerAnim("Off"); // 필드 끄기


        bool isEssential = false;
        if (CardOnDisplay.EssentialEquip == equipType.ToString())
        {
            isEssential = true;
        }
        equipInfoPanel.SetPanel(itemCardData, iData, cardDisp, isEquipButton, isEssential);
        cardToEquip = itemCardData;
        this.cardDisp = cardDisp;

        warningLackCanvasGroup.alpha = 0;

        int level = itemCardData.Level;
        UpdateUpgradeCost(level, upgradeCost);

        UpdateButtonState(charUpgradeButton, true);
        UpdateButtonState(upgradeButton, false);
    }

    public void DeActivateEquipInfoPanel()
    {
        equipInfoPanel.gameObject.SetActive(false);
        cardSlotManager.SettrigerAnim("EquipI"); // 필드 끄기, 인포는 무조건 아이템이니까 아이템으로 돌아감

        this.cardDisp = null;
    }
    
    #region Display에 보여지는 오리카드의 업그레이드
    public void UpgradeCardOnDisplay()
    {
        charUpgradeButton.GetComponent<ButtonEffect>().ShoutldBeInitialSound = true;

        int level = CardOnDisplay.Level;
        int amountToUpgrade = GetAmountToUpgrade(level);
        int candyNumbers = playerDataManager.GetCurrentCandyNumber();

        if (amountToUpgrade > candyNumbers)
        {
            // 업그레이드 버튼 사운드 다르게 
            charUpgradeButton.GetComponent<ButtonEffect>().ShoutldBeInitialSound = false;

            // 코인 부족 경고 메시지 띄우고 종료해서 업그레이드가 되지 않도록 하기
            charWarningLack = charWarningLackCanvasGroup.DOFade(1, 1f);

            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideWarning(charWarningLackCanvasGroup));
            return;
        }

        // 가지고 있는 재화에서 업그레이드 비용 빼주고 데이터 저장
        playerDataManager.AddCandyNumber(-amountToUpgrade);

        // 레벨업 하고 card data에 저장
        statManager.LevelUp(CardOnDisplay);

        // 레벨업된 수치를 Level UI에 반영
        equipDisplayUI.SetLevelUI(CardOnDisplay);

        // 레벨업 된 수치를 Atk, Hp UI에 반영
        equipDisplayUI.SetAtkHpStats(CardOnDisplay.Atk, CardOnDisplay.Hp);

        UpdateUpgradeCost(level, charUpgradeCost);
        if (CheckIfMaxLevel(CardOnDisplay)) SoundManager.instance.Play(maxLevelSound); // 최고레벨이면 MaxLevel 사운드 재생
        UpdateButtonState(charUpgradeButton, true);
    }
    #endregion

    #region info panel의 업그레이드 버튼
    public void UpgradeCard()
    {
        upgradeButton.GetComponent<ButtonEffect>().ShoutldBeInitialSound = true;

        int level = cardToEquip.Level;
        int amountToUpgrade = GetAmountToUpgrade(level);
        int candyNumbers = playerDataManager.GetCurrentCandyNumber();

        // 코인이 부족하면 경고 메시지를 띄우고 종료
        if (amountToUpgrade > candyNumbers)
        {
            // 업그레이드 버튼 사운드 다르게 
            upgradeButton.GetComponent<ButtonEffect>().ShoutldBeInitialSound = false;

            // 업그레이드가 가능하지 않게 하기
            warningLack = warningLackCanvasGroup.DOFade(1, 1f);

            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideWarning(warningLackCanvasGroup));
            return;
        }

        // 가지고 있는 재화에서 업그레이드 비용 빼주고 데이터 저장
        playerDataManager.AddCandyNumber(-amountToUpgrade);

        // 레벨업 하고 card data에 저장
        statManager.LevelUp(cardToEquip);

        // 장착되어 있는 장비를 레벨업 하는 경우라면 바로바로 currentAttr을 업데이트
        if (isEquipped)
        {
            equipmentSlotsManager.InitEquipSlots(CardOnDisplay);
        }

        UpdateUpgradeCost(level, upgradeCost);
        if (CheckIfMaxLevel(cardToEquip)) SoundManager.instance.Play(maxLevelSound); // 최고레벨이면 MaxLevel 사운드 재생
        UpdateButtonState(upgradeButton, false);
    }

    /// <summary>
    /// 업그레이드 비용을 UI에 표시
    /// </summary>
    void UpdateUpgradeCost(int _level, TMPro.TextMeshProUGUI _upgradeCost)
    {
        _upgradeCost.text = "X " + GetAmountToUpgrade(_level).ToString();
    }

    // 카드의 레벨에 대응하는 업그레이드 비용 계산
    int GetAmountToUpgrade(int level)
    {
        return level * 3;
    }
    #endregion


    #region 버튼 상태 업데이트
    // 조건에 따라 업그레이드 버튼을 활성/비활성 시킴
    void UpdateButtonState(Button button, bool isChar)
    {
        // CharCoinImage.SetActive(true);
        // EquipCoinImage.SetActive(true);
        // charUpgradeText.SetActive(true);

        // charMaxLevel.SetActive(false);
        // itemMaxLevel.SetActive(false);

        if (isChar)
        {
            if (CardOnDisplay.Level == StaticValues.MaxLevel)
            {
                charMaxLevel.SetActive(true);
                charUpgradeText.SetActive(false);
                charUpgradeCost.text = "";
                CharCoinImage.SetActive(false);
                button.interactable = false;
                return;
            }
            else
            {
                charMaxLevel.SetActive(false);
                charUpgradeText.SetActive(true);
                CharCoinImage.SetActive(true);
            }
        }
        else
        {
            if (cardToEquip.Level == StaticValues.MaxLevel)
            {
                itemMaxLevel.SetActive(true);
                itemUpgradeText.SetActive(false);
                upgradeCost.text = "";
                EquipCoinImage.SetActive(false);
                button.interactable = false;
                return;
            }
            else
            {
                itemMaxLevel.SetActive(false);
                itemUpgradeText.SetActive(true);
                // upgradeCost.text = "";
                EquipCoinImage.SetActive(true);
            }
        }
        button.interactable = true;
    }
    bool CheckIfMaxLevel(CardData _cardData)
    {
        if (_cardData.Level == StaticValues.MaxLevel)
            return true;
        return false;
    }
    #endregion

    IEnumerator HideWarning(CanvasGroup canvasGroupToHide)
    {
        yield return new WaitForSeconds(2f);
        canvasGroupToHide.DOFade(0, 1f);
    }
    public void TempKillAllTweens()
    {
        DOTween.KillAll();
    }
}
