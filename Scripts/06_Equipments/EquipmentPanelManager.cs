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

    CardDataManager cardDataManager => CardDataManager.Instance;
    CardsDictionary cardDictionary => CardsDictionary.Instance;
    EquipmentSlotsManager equipmentSlotsManager;
    CardList cardList => CardList.Instance;
    StatManager statManager;
    CardDisp cardDisp; // Equip info panel이 활성화 되면 클릭한 카드의 disp클래스를 저장(equipped Text 표시를 위해)
    SetCardDataOnSlot setCardDataOnSlot;
    Equation equation;

    EquipDisplayUI equipDisplayUI;
    [SerializeField] EquipInfoPanel equipInfoPanel;
    [SerializeField] AllField field; // 모든 카드

    [Header("Equipment Slots")]
    [SerializeField] ShadowedText upgradeCost;
    [SerializeField] GameObject itemMaxLevel;
    [SerializeField] GameObject itemUpgradeText; // 최고 레벨일 때 업그레이드 텍스트 숨기기
    [SerializeField] CanvasGroup warningLackCanvasGroup; // 아이템 업그레이드 코인 부족 경고 메시지
    [SerializeField] Button upgradeButton;
    [SerializeField] GameObject EquipCoinImage;
    [SerializeField] GameObject warningNoItemToEquip; // 장착 가능한 장비가 없을 때 경고
    [SerializeField] GameObject selectCardGuideText; // "카드를 선택해서 장비를 확인해 보세요!" 문구 오브젝트
    PlayerDataManager playerDataManager;
    Tween warningLack;
    Tween warningMax;

    [Header("Char Card Slot")]
    [SerializeField] CardSlot oriSlot;
    [SerializeField] ShadowedText charUpgradeCost;
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

    [Header("Set Glow")]
    [SerializeField] GameObject oriSlotGlow;        // 오리카드의 Set Glow
    [SerializeField] GameObject[] equipSlotGlows;   // 장비 슬롯 4개의 Set Glow (Head/Chest/Face/Hand 순)
    [SerializeField] GameObject [] otherGlows; // 다른 세트 글로우들. ray, circle...
    SetBonusChecker setBonusChecker;

    [Header("Set Complete Sound")]
    [SerializeField] AudioClip setCompleteSound;

    [Header("코인 부족 경고 팝업")]
    [SerializeField] GameObject lackOfCoinWarningPopup; // Canvas에 미리 만들어둔 팝업
    PanelTween lackOfCoinWarningPopupTween; // PanelTween 컴포넌트 캐싱

    CardSlotManager cardSlotManager; // 카드 슬롯 풀

    [SerializeField] AudioClip maxLevelSound;

    // ⭐ SetAllFieldTypeOf 재시도 코루틴 중복 실행 방지용
    Coroutine retrySetAllFieldTypeOfCoroutine;

    void Awake()
    {
        equipDisplayUI = GetComponentInChildren<EquipDisplayUI>();
        setCardDataOnSlot = GetComponent<SetCardDataOnSlot>();
        cardSlotManager = FindObjectOfType<CardSlotManager>();
        equipmentSlotsManager = GetComponent<EquipmentSlotsManager>();
        statManager = FindAnyObjectByType<StatManager>();
        equation = new Equation();

        cardToEquip = null;

        playerDataManager = FindObjectOfType<PlayerDataManager>();

        warningLackCanvasGroup.alpha = 0;
        charWarningLackCanvasGroup.alpha = 0;

        setBonusChecker = FindObjectOfType<SetBonusChecker>();

        // ⭐ PanelTween 컴포넌트 캐싱
        if (lackOfCoinWarningPopup != null)
        {
            lackOfCoinWarningPopupTween = lackOfCoinWarningPopup.GetComponent<PanelTween>();
            if (lackOfCoinWarningPopupTween == null)
            {
                Debug.LogWarning("[EquipmentPanelManager] lackOfCoinWarningPopup에 PanelTween 컴포넌트가 없습니다!");
            }
        }
    }

    void OnEnable()
    {
        cardToEquip = null;
        SetAllFieldTypeOf("Weapon"); // 내부적으로 EquipW 트리거를 세팅함
        DeActivateEquipInfoPanel(false); // ⭐ false: 위에서 이미 EquipW를 세팅했으므로 트리거 재발동(EquipI) 방지
        CardOnDisplay = null;

        charUpgradeButton.gameObject.SetActive(false);
        ClearAllEquipmentSlots(); // logic, UI 모두 처리

        // ⭐ 탭에 들어올 때마다 안내 문구 활성화
        if (selectCardGuideText != null)
            selectCardGuideText.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine); // 경고 메시지 표시 도중 패널을 나갔다면, 돌아왔을 때 메시지를 없애기

        warningLackCanvasGroup.alpha = 0;
        charWarningLackCanvasGroup.alpha = 0;

        // ⭐ null 체크
        if (lackOfCoinWarningPopup != null)
        {
            lackOfCoinWarningPopup.SetActive(false);
        }

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.InitialSortingByGrade();

        GearTutorialController.instance?.OnGearPanelEntered();
    }

    void OnDisable()
    {
        // ⭐ 장비 탭을 벗어나면 안내 문구 무조건 비활성화
        if (selectCardGuideText != null)
            selectCardGuideText.SetActive(false);

        // ⭐ 주의: 여기서 ForceFieldOff()를 호출하지 않습니다.
        //    Slot Containers의 Off 리셋은 MainMenuManager.SetTabPos()에서
        //    탭 전환 시점에 중앙집중적으로 한 번만 처리합니다.
        //    여기서 다시 호출하면 ActivatePanel()의 반복문 순서에 따라
        //    다른 탭이 방금 세팅한 "위로 올라오는" 트리거를 덮어써버리는 문제가 생깁니다.

        // ⭐ 탭을 나갈 때 재시도 코루틴이 대기 중이면 정리
        if (retrySetAllFieldTypeOfCoroutine != null)
        {
            StopCoroutine(retrySetAllFieldTypeOfCoroutine);
            retrySetAllFieldTypeOfCoroutine = null;
        }
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

        // ⭐ 카드가 디스플레이에 올라가면 안내 문구 숨기기
        if (selectCardGuideText != null)
            selectCardGuideText.SetActive(false);

        // 나중에는 이 항목들을 EquipDispUI에 옮겨야 한다. SetWeaponDisplay에 포함되도록
        int level = CardOnDisplay.Level;
        UpdateUpgradeCost(CardOnDisplay, charUpgradeCost);
        UpdateButtonState(charUpgradeButton, true);

        setCardDataOnSlot.PutCardDataIntoSlot(oriCardDataToDisplay, oriSlot);

        isEquipped = false;
        Logger.Log("Card on Display = " + CardOnDisplay.Name);

        GearTutorialController.instance?.OnDuckSelected();
        UpdateSetGlow(); 
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

            // ★ cardDataManager 또는 카드 리스트가 아직 준비되지 않았을 때
            //   경고 로그를 남기고, 준비될 때까지 기다렸다가 재시도
            if (cardDataManager == null)
            {
                Debug.LogWarning("[EquipmentPanelManager] cardDataManager(CardDataManager.Instance)가 아직 null입니다. " +
                    "초기화가 끝나는 대로 SetAllFieldTypeOf를 재시도합니다.");
                StartRetrySetAllFieldTypeOf(cardType);
                return;
            }

            var myCardList = cardDataManager.GetMyCardList();
            if (myCardList == null)
            {
                Debug.LogWarning("[EquipmentPanelManager] cardDataManager.GetMyCardList()가 아직 null입니다. " +
                    "카드 데이터(세이브 로드)가 준비되는 대로 SetAllFieldTypeOf를 재시도합니다.");
                StartRetrySetAllFieldTypeOf(cardType);
                return;
            }

            card = myCardList.FindAll(x => x.Type == cardType); // field에는 오리만 보여줌
        }
        else if (cardType == CardType.Item.ToString())
        {
            var allItems = cardList.GetEquipmentCardsList();
            Debug.Log($"[EquipmentPanelManager] GetEquipmentCardsList().Count = {allItems.Count}");
            foreach (var i in allItems)
                Debug.Log($"[EquipmentPanelManager]   - ID:{i.CardData.ID} Name:{i.CardData.Name} IsEquipped:{i.IsEquipped} BindingTo:{i.CardData.BindingTo}");
            foreach (var item in allItems)
            {
                if (item.IsEquipped) // 다른 오리에 장착된 카드는 보여주지 않음
                {
                    continue;
                }

                // 범용이거나 해당 오리에 바인딩 되어 있는 장비라면 필드에 추가
                if (item.CardData.BindingTo == "All")
                {
                    // 범용이어도 필수 장비 슬롯과 겹치면서 해당 오리에 바인딩 되어 있지 않다면 빼기
                    if (item.CardData.EquipmentType == CardOnDisplay.EssentialEquip
                        && item.CardData.BindingTo != CardOnDisplay.Name)
                        continue;

                    Debug.Log($"[EquipmentPanelManager] Item 후보 추가: {item.CardData.Name} (ID:{item.CardData.ID}), IsEquipped={item.IsEquipped}, BindingTo={item.CardData.BindingTo}, CardOnDisplay={CardOnDisplay.Name}");
                    card.Add(item.CardData);
                    
                    continue;
                }
                if (item.CardData.BindingTo == CardOnDisplay.Name)
                {
                    Debug.Log($"[EquipmentPanelManager] Item 후보 추가: {item.CardData.Name} (ID:{item.CardData.ID}), IsEquipped={item.IsEquipped}, BindingTo={item.CardData.BindingTo}, CardOnDisplay={CardOnDisplay.Name}");
                    card.Add(item.CardData);
                    continue;
                }
            }
        }

        // 조건에 맞는 카드가 있는지 확인
        warningNoItemToEquip.SetActive(false);

        if (card.Count == 0)
        {
            warningNoItemToEquip.SetActive(true);
        }

        field.GenerateAllCardsOfType(card, "Equip");

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim(fieldAnimTrigger); // 오리 혹은 아이템 필드를 보여주기.
    }

    // ⭐ 재시도 코루틴 시작 (중복 실행 방지)
    void StartRetrySetAllFieldTypeOf(string cardType)
    {
        if (retrySetAllFieldTypeOfCoroutine != null)
            StopCoroutine(retrySetAllFieldTypeOfCoroutine);

        retrySetAllFieldTypeOfCoroutine = StartCoroutine(RetrySetAllFieldTypeOfCo(cardType));
    }

    // ⭐ 데이터가 준비될 때까지 기다렸다가 같은 요청을 다시 실행
    IEnumerator RetrySetAllFieldTypeOfCo(string cardType)
    {
        yield return new WaitUntil(() =>
            GameInitializer.IsInitialized &&
            cardDataManager != null &&
            cardDataManager.GetMyCardList() != null);

        retrySetAllFieldTypeOfCoroutine = null;

        // 이 시점엔 데이터가 준비되어 있으므로,
        // 정상적으로 카드 목록 생성 + 애니메이션 트리거까지 처리됨
        SetAllFieldTypeOf(cardType);
    }

    void ClearAllEquipmentSlots()
    {
        // Display의 장비 슬롯들을 모두 비우기
        equipmentSlotsManager.ClearEquipSlots(); // logic
        equipDisplayUI.OffDisplay(); // UI
        oriSlot.EmptySlot(); // 슬롯 비활성화
        SetGlowActive(false);
    }

    #region Info패널 장착/해제 버튼
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

            UpdateCardSlotOfPool(equipmentSlotsManager.GetSlotCardData(index)); // 해제되는 장비의 그림도 업데이트 isEquipped 태그 관련
        }

        // 새로운 장비 장착
        cardList.Equip(CardOnDisplay, cardToEquip);
        Item itemData = cardDictionary.GetWeaponItemData(cardToEquip).itemData;
        equipmentSlotsManager.SetEquipSlot(index, itemData, cardToEquip);

        // ⭐ false: SettrigerAnim 스킵, 아래 SetAllFieldTypeOf에서 1번만 호출
        DeActivateEquipInfoPanel(false);

        equipDisplayUI.PopCharImage();

        SetAllFieldTypeOf("Item");
        setCardDataOnSlot.PutCardDataIntoSlot(CardOnDisplay, oriSlot); // 오리 디스플레이 갱신

        UpdateCardSlotOfPool(CardOnDisplay); // 카드 슬롯 풀의 슬로 그림도 업데이트
        UpdateCardSlotOfPool(equipmentCards[index].CardData); // 장착하는 장비의 그림도 업데이트. isEquipped

        cardList.DelayedSaveEquipments();
        DeActivateEquipInfoPanel();

        // ✅ Step2일 때만 진행
        if (TutorialManager.instance != null &&
            TutorialManager.instance.CurrentStep == TutorialStep.Step2_GearUnlocked)
        {
            TutorialManager.instance.AdvanceStep(); // → Step3_MergeUnlocked
        }

        // ⭐ 장비 장착 즉시 클라우드 강제 저장
        // ⭐ 딜레이 저장 완료 후 클라우드 저장
        StartCoroutine(SaveToCloudAfterDelay());
        UpdateSetGlow();
    }

    // info panel의 UnEquip 버튼
    public void OnUnEquipButton()
    {
        // 해제될 장비 카드의 카드 데이터를 미리 저장
        EquipmentCard[] equipmentCards = cardList.GetEquipmentsCardData(CardOnDisplay);
        CardData unequippedCardData = equipmentCards[index]?.CardData;

        // 장비 해제
        cardList.UnEquip(CardOnDisplay, equipmentCards[index]);
        cardList.DelayedSaveEquipments();
        equipmentSlotsManager.EmptyEquipSlot(index);

        cardToEquip = null;
        cardDisp.SetEquppiedTextActive(false);
        SetAllFieldTypeOf("Item");

        setCardDataOnSlot.PutCardDataIntoSlot(CardOnDisplay, oriSlot); // 오리 디스플레이 갱신

        UpdateCardSlotOfPool(CardOnDisplay); // 카드 슬롯 풀의 슬로 그림도 업데이트
        UpdateCardSlotOfPool(unequippedCardData); //해제하는 장비의 그림도 업데이트. isEquipped

        equipDisplayUI.UnEquipCharImage();
        DeActivateEquipInfoPanel();

        // ⭐ 장비 해제 즉시 클라우드 강제 저장
        // ⭐ 딜레이 저장 완료 후 클라우드 저장
        StartCoroutine(SaveToCloudAfterDelay());
        UpdateSetGlow();
    }
    private IEnumerator SaveToCloudAfterDelay()
    {
        // DelayedSave의 0.04초보다 약간 더 기다림
        yield return new WaitForSeconds(0.1f);
        CloudSaveManager.Instance?.ForceSaveToCloud();
    }
    #endregion

    // 카드 슬롯 풀의 그림도 업데이트
    void UpdateCardSlotOfPool(CardData cardData)
    {
        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.UpdateCardDisplay(cardData);
    }

    // equip slot action 에서 호출
    public void ActivateEquipInfoPanel(CardData itemCardData, CardDisp cardDisp, bool isEquipButton, EquipmentType equipType)
    {
        // GearBGToExitInfo.SetActive(true); // x 버튼, bg to exit info버튼으로 비활성화
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

        UpdateUpgradeCost(itemCardData, upgradeCost);

        UpdateButtonState(charUpgradeButton, true);
        UpdateButtonState(upgradeButton, false);

        GearTutorialController.instance?.OnItemSelected();
    }

    public void DeActivateEquipInfoPanel(bool triggerAnim = true)
    {
        equipInfoPanel.gameObject.SetActive(false);
        if (triggerAnim)
        {
            if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
            cardSlotManager.SettrigerAnim("EquipI"); // 필드 끄기, 인포는 무조건 아이템이니까 아이템으로 돌아감
        }

        this.cardDisp = null;
    }

    #region Display에 보여지는 오리카드의 업그레이드
    public void UpgradeCardOnDisplay()
    {
        charUpgradeButton.GetComponent<ButtonEffect>().ShoutldBeInitialSound = true;

        int amountToUpgrade = GetAmountToUpgrade(CardOnDisplay);
        int candyNumbers = playerDataManager.GetCurrentCoinNumber();

        if (amountToUpgrade > candyNumbers)
        {
            // 업그레이드 버튼 사운드 다르게 
            charUpgradeButton.GetComponent<ButtonEffect>().ShoutldBeInitialSound = false;

            // ⭐ 코인 부족 경고 팝업 표시
            ShowLackOfCoinWarning();
            return;
        }

        // 가지고 있는 재화에서 업그레이드 비용 빼주고 데이터 저장
        playerDataManager.AddCoin(-amountToUpgrade);

        // 레벨업 하고 card data에 저장
        statManager.LevelUp(CardOnDisplay);

        // 레벨업된 수치를 Level UI에 반영
        equipDisplayUI.SetLevelUI(CardOnDisplay);

        // 레벨업 된 수치를 Atk, Hp UI에 반영 - 오리만의 수치(Card On Display)가 아니라 장비의 수치까지 합쳐져야 함
        equipmentSlotsManager.UpdateCurrentAttribute(CardOnDisplay);
        equipDisplayUI.SetAtkHpStats(equipmentSlotsManager.GetCurrentAttribute().Atk, equipmentSlotsManager.GetCurrentAttribute().Hp);

        UpdateUpgradeCost(CardOnDisplay, charUpgradeCost);
        if (CheckIfMaxLevel(CardOnDisplay))
        {
            SoundManager.instance.Play(maxLevelSound); // 최고레벨이면 MaxLevel 사운드 재생
            equipDisplayUI.PlayMaxLevelPop();          // ⭐ 레벨 텍스트 팝 연출
            equipDisplayUI.PlayCharDispMaxPop();       // ⭐ Char Disp 전체 팝 연출
        }
        UpdateButtonState(charUpgradeButton, true);

        // 필드의 해당 카드도 업데이트
        UpdateCardSlotOfPool(CardOnDisplay);

        CloudSaveManager.Instance?.SaveToCloud(); // ⭐ 추가
    }
    #endregion

    #region info panel의 업그레이드 버튼
    public void UpgradeCard()
    {
        upgradeButton.GetComponent<ButtonEffect>().ShoutldBeInitialSound = true;

        int amountToUpgrade = GetAmountToUpgrade(cardToEquip);
        int candyNumbers = playerDataManager.GetCurrentCoinNumber();

        // 코인이 부족하면 경고 메시지를 띄우고 종료
        if (amountToUpgrade > candyNumbers)
        {
            // 업그레이드 버튼 사운드 다르게 
            upgradeButton.GetComponent<ButtonEffect>().ShoutldBeInitialSound = false;

            // ⭐ 코인 부족 경고 팝업 표시
            ShowLackOfCoinWarning();
            return;
        }

        // 가지고 있는 재화에서 업그레이드 비용 빼주고 데이터 저장
        playerDataManager.AddCoin(-amountToUpgrade);

        // 레벨업 하고 card data에 저장
        statManager.LevelUp(cardToEquip);

        // 장착되어 있는 장비를 레벨업 하는 경우라면 바로바로 currentAttr을 업데이트
        if (isEquipped)
        {
            // ⭐ UpdateCurrentAttribute 후 SetAtkHpStats 명시적 호출
            equipmentSlotsManager.UpdateCurrentAttribute(CardOnDisplay);
            equipDisplayUI.SetAtkHpStats(
                equipmentSlotsManager.GetCurrentAttribute().Atk,
                equipmentSlotsManager.GetCurrentAttribute().Hp
            );
        }

        UpdateUpgradeCost(cardToEquip, upgradeCost);
        if (CheckIfMaxLevel(cardToEquip)) SoundManager.instance.Play(maxLevelSound); // 최고레벨이면 MaxLevel 사운드 재생
        UpdateButtonState(upgradeButton, false);

        CloudSaveManager.Instance?.SaveToCloud(); // ⭐ 추가
    }

    /// <summary>
    /// 업그레이드 비용을 UI에 표시
    /// </summary>
    void UpdateUpgradeCost(CardData cardData, ShadowedText _upgradeCost)
    {
        _upgradeCost.text = "X " + GetAmountToUpgrade(cardData).ToString();
    }

    /// <summary>
    /// 카드의 Grade와 Level에 대응하는 업그레이드 비용 계산
    /// Define.cs의 Equation.GetUpgradeCost 메서드를 사용
    /// </summary>
    int GetAmountToUpgrade(CardData cardData)
    {
        return equation.GetUpgradeCost(cardData.Level, cardData.Grade, cardData.EvoStage);
    }
    #endregion

    #region 세트 글로우 효과
    void UpdateSetGlow()
    {
        if (CardOnDisplay == null || setBonusChecker == null)
        {
            SetGlowActive(false);
            return;
        }

        bool isSetComplete = setBonusChecker.GetSetBonus(CardOnDisplay) != null;
        SetGlowActive(isSetComplete);
        if(isSetComplete) PlaySetCompleteSound();
    }

    void SetGlowActive(bool active)
    {
        if (oriSlotGlow != null)
            oriSlotGlow.SetActive(active);

        if (equipSlotGlows != null)
        {
            for (int i = 0; i < equipSlotGlows.Length; i++)
            {
                if (equipSlotGlows[i] != null)
                    equipSlotGlows[i].SetActive(active);
            }
        }

        if (otherGlows != null)
        {
            for (int i = 0; i < otherGlows.Length; i++)
            {
                otherGlows[i].SetActive(active);
            }
        }
    }

    void PlaySetCompleteSound()
    {
        SoundManager.instance.Play(setCompleteSound);
    }
    #endregion

    #region 코인 부족 경고 팝업
    /// <summary>
    /// ⭐ 코인 부족 경고 팝업 표시
    /// </summary>
    void ShowLackOfCoinWarning()
    {
        if (lackOfCoinWarningPopup == null)
        {
            Debug.LogError("[EquipmentPanelManager] lackOfCoinWarningPopup이 할당되지 않았습니다!");
            return;
        }

        // PanelTween이 있으면 애니메이션과 함께 표시
        if (lackOfCoinWarningPopupTween != null)
        {
            lackOfCoinWarningPopupTween.ShowWithScale();
        }
        else
        {
            // PanelTween이 없으면 그냥 활성화
            lackOfCoinWarningPopup.SetActive(true);
        }
    }
    #endregion

    #region 버튼 상태 업데이트
    // 조건에 따라 업그레이드 버튼을 활성/비활성 시킴
    void UpdateButtonState(Button button, bool isChar)
    {
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