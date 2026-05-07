using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 판매 패널 전체를 제어합니다.
/// 탭 전환 / 카드 목록 생성 / 선택 관리 / 판매 실행을 담당합니다.
/// </summary>
public class SellPanelManager : MonoBehaviour
{
    // ───── 외부 참조 ─────
    CardDataManager cardDataManager;
    CardList cardList;
    CardsDictionary cardsDictionary;
    PlayerDataManager playerDataManager;
    CardSlotManager cardSlotManager;

    // ───── 가격 계산 ─────
    [Header("판매 가격 데이터")]
    [SerializeField] TextAsset sellPriceDataAsset;
    SellPriceDataReader priceReader = new SellPriceDataReader();
    Equation equation = new Equation();

    // ───── SellCardSlot 풀 ─────
    [Header("카드 슬롯")]
    [SerializeField] GameObject sellCardSlotPrefab;
    [SerializeField] Transform duckContent;   // 오리 탭 ScrollView Content
    [SerializeField] Transform itemContent;   // 아이템 탭 ScrollView Content
    List<SellCardSlot> activeSlots = new List<SellCardSlot>();

    // ───── 탭 ─────
    [Header("탭")]
    [SerializeField] GameObject duckTab;
    [SerializeField] GameObject itemTab;
    [SerializeField] Button duckTabButton;
    [SerializeField] Button itemTabButton;
    bool isDuckTab = true;

    // ───── 선택 상태 ─────
    List<SellCardSlot> selectedSlots = new List<SellCardSlot>();
    int totalSellPrice = 0;

    // ───── UI ─────
    [Header("UI")]
    [SerializeField] TextMeshProUGUI totalPriceText;  // "총 판매가: 1,200"
    [SerializeField] Button sellButton;
    [SerializeField] RectTransform coinIconRect;       // GemCollectFX 타겟용

    // ───── 팝업: 장착 중인 아이템 판매 확인 ─────
    [Header("장착 아이템 판매 팝업")]
    [SerializeField] GameObject equippedItemPopup;
    [SerializeField] PanelTween equippedItemPopupTween;
    [SerializeField] TextMeshProUGUI equippedItemPopupText;
    [SerializeField] Button equippedItemConfirmButton;  // "해제 후 판매"
    [SerializeField] Button equippedItemCancelButton;   // "취소"

    // ───── 팝업: 장비 낀 오리 판매 ─────
    [Header("장비 낀 오리 판매 팝업")]
    [SerializeField] GameObject duckWithEquipPopup;
    [SerializeField] PanelTween duckWithEquipPopupTween;
    [SerializeField] GemCollectFX gemCollectFX;
    [SerializeField] TextMeshProUGUI duckWithEquipPopupText;
    [SerializeField] Button sellWithEquipButton;    // "장비도 함께 판매"
    [SerializeField] Button sellWithoutEquipButton; // "장비는 해제 후 보관"
    [SerializeField] Button duckWithEquipCancelButton; // "취소"

    [Header("오리 1마리 경고")]
    [SerializeField] GameObject lastDuckWarningPopup;
    [SerializeField] PanelTween lastDuckWarningPopupTween;

    [Header("필수 장비 판매 차단 경고")]
    [SerializeField] GameObject essentialItemWarningPopup;
    [SerializeField] PanelTween essentialItemWarningPopupTween;

    // ───── GemCollectFX ─────
    [Header("코인 애니메이션")]

    // ───── 판매 실행 전 임시 저장 ─────
    // 팝업에서 선택이 확정된 후 실제 판매에 사용
    List<SellCardSlot> pendingSellSlots = new List<SellCardSlot>();
    bool pendingSellEquipmentsToo = false; // 오리의 장비도 함께 판매할지

    // ─────────────────────────────────────────
    #region 초기화
    // ─────────────────────────────────────────

    void Awake()
    {
        cardDataManager   = FindObjectOfType<CardDataManager>();
        cardList          = FindObjectOfType<CardList>();
        cardsDictionary   = FindObjectOfType<CardsDictionary>();
        playerDataManager = PlayerDataManager.Instance;
        cardSlotManager   = FindObjectOfType<CardSlotManager>();

        priceReader.Load(sellPriceDataAsset);

        // 팝업 버튼 연결
        equippedItemConfirmButton.onClick.AddListener(OnEquippedItemConfirmed);
        equippedItemCancelButton.onClick.AddListener(OnEquippedItemCanceled);

        sellWithEquipButton.onClick.AddListener(() => OnDuckWithEquipConfirmed(true));
        sellWithoutEquipButton.onClick.AddListener(() => OnDuckWithEquipConfirmed(false));
        duckWithEquipCancelButton.onClick.AddListener(OnDuckWithEquipCanceled);
    }

    void OnEnable()
    {
        // 패널이 열릴 때 초기화
        selectedSlots.Clear();
        totalSellPrice = 0;
        UpdateTotalPriceUI();

        CloseAllPopups();

        // 오리 탭부터 시작
        SwitchTab(true);
    }

    void OnDisable()
    {
        ClearAllSlots();
    }

    #endregion

    // ─────────────────────────────────────────
    #region 탭 전환
    // ─────────────────────────────────────────

    public void OnDuckTabButton()  => SwitchTab(true);
    public void OnItemTabButton()  => SwitchTab(false);

    void SwitchTab(bool toDuck)
    {
        isDuckTab = toDuck;

        duckTab.SetActive(toDuck);
        itemTab.SetActive(!toDuck);

        // 선택 초기화
        selectedSlots.Clear();
        totalSellPrice = 0;
        UpdateTotalPriceUI();

        ClearAllSlots();
        GenerateSlots(toDuck ? "Weapon" : "Item");
    }

    #endregion

    // ─────────────────────────────────────────
    #region 슬롯 생성
    // ─────────────────────────────────────────

    void GenerateSlots(string cardType)
    {
        List<CardData> cards = cardDataManager.GetMyCardList()
            .FindAll(x => x.Type == cardType);

        Transform content = isDuckTab ? duckContent : itemContent;

        foreach (CardData data in cards)
        {
            GameObject obj = Instantiate(sellCardSlotPrefab, content);

            // ⭐ 카드 크기 0.5로 설정
            obj.transform.localScale = Vector3.one * 0.5f;

            SellCardSlot slot = obj.GetComponent<SellCardSlot>();

            int price = equation.GetSellPrice(data, priceReader);
            bool equipped = IsCardEquipped(data);

            if (cardType == CardType.Weapon.ToString())
            {
                WeaponData wData = cardsDictionary.GetWeaponItemData(data).weaponData;
                slot.SetupAsDuck(data, wData, price, equipped, OnSlotToggled);

                // ⭐ 장비 스프라이트 적용
                EquipmentCard[] equipCards = cardList.GetEquipmentsCardData(data);
                slot.ApplyEquipSprites(equipCards, cardsDictionary);
            }
            else
            {
                Item iData = cardsDictionary.GetWeaponItemData(data).itemData;
                slot.SetupAsItem(data, iData, price, equipped, OnSlotToggled);
            }

            activeSlots.Add(slot);
        }

        Logger.Log($"[SellPanelManager] {cardType} 슬롯 {activeSlots.Count}개 생성");
    }

    void ClearAllSlots()
    {
        foreach (SellCardSlot slot in activeSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        activeSlots.Clear();
    }

    #endregion

    // ─────────────────────────────────────────
    #region 선택 관리
    // ─────────────────────────────────────────

    /// <summary>
    /// SellCardSlot에서 선택/해제될 때 호출되는 콜백
    /// </summary>
    void OnSlotToggled(SellCardSlot slot, bool isSelected)
    {
        if (isSelected)
        {
            if (!selectedSlots.Contains(slot))
                selectedSlots.Add(slot);
        }
        else
        {
            selectedSlots.Remove(slot);
        }

        // 총 판매가 재계산
        totalSellPrice = 0;
        foreach (SellCardSlot s in selectedSlots)
            totalSellPrice += s.GetSellPrice();

        UpdateTotalPriceUI();
    }

    void UpdateTotalPriceUI()
    {
        if (totalPriceText != null)
            totalPriceText.text = totalSellPrice.ToString("N0");

        // 선택된 게 없으면 판매 버튼 비활성
        if (sellButton != null)
            sellButton.interactable = selectedSlots.Count > 0;
    }

    #endregion

    // ─────────────────────────────────────────
    #region 판매 버튼
    // ─────────────────────────────────────────

    /// <summary>
    /// 판매 버튼 클릭
    /// </summary>
    public void OnSellButton()
    {
        if (selectedSlots.Count == 0) return;

        // ── 오리 탭 ──
        if (isDuckTab)
        {
            // ⭐ Weapon 타입만 정확히 필터링
            int totalDuckCount = cardDataManager.GetMyCardList()
                .FindAll(x => x.Type == CardType.Weapon.ToString()).Count;

            int selectedDuckCount = selectedSlots.Count;

            Logger.Log($"[SellPanelManager] 전체 오리: {totalDuckCount}, 선택한 오리: {selectedDuckCount}");

            if (selectedSlots.Count >= totalDuckCount)
            {
                ShowLastDuckWarning();
                return;
            }

            // 장비가 달린 오리가 있는지 확인
            List<SellCardSlot> ducksWithEquip = selectedSlots
                .FindAll(s => HasEquipment(s.GetCardData()));

            if (ducksWithEquip.Count > 0)
            {
                int totalEquipCount = 0;
                foreach (SellCardSlot s in ducksWithEquip)
                    totalEquipCount += GetEquipmentCount(s.GetCardData());

                ShowDuckWithEquipPopup(ducksWithEquip.Count, totalEquipCount);
                return;
            }

            // 장비 없는 오리만 선택 → 바로 판매
            ExecuteSell(new List<SellCardSlot>(selectedSlots), false);
            return;
        }

        // ── 아이템 탭 ──

        // 필수 장비 판매 차단
        List<SellCardSlot> essentialSlots = selectedSlots
            .FindAll(s => s.GetCardData().BindingTo != "All");

        if (essentialSlots.Count > 0)
        {
            ShowEssentialItemWarning(essentialSlots.Count);
            return;
        }

        // 장착 중인 아이템 확인
        List<SellCardSlot> equippedSlots = selectedSlots
            .FindAll(s => s.IsEquipped());

        if (equippedSlots.Count > 0)
        {
            ShowEquippedItemPopup(equippedSlots.Count);
            return;
        }

        // 장착 중 없으면 바로 판매
        ExecuteSell(new List<SellCardSlot>(selectedSlots), false);
    }
    #endregion

    // ─────────────────────────────────────────
    #region 팝업: 장착 중인 아이템
    // ─────────────────────────────────────────

    void ShowEquippedItemPopup(int count)
    {
        pendingSellSlots = new List<SellCardSlot>(selectedSlots);

        string msg = count == 1
            ? "선택한 장비가 오리에게 장착 중입니다.\n해제 후 판매할까요?"
            : $"선택한 장비 중 {count}개가 장착 중입니다.\n모두 해제 후 판매할까요?";

        equippedItemPopupText.text = msg;
        equippedItemPopupTween.ShowWithScale();
    }

    void OnEquippedItemConfirmed()
    {
        equippedItemPopupTween.HideWithScale();
        ExecuteSell(pendingSellSlots, false);
    }

    void OnEquippedItemCanceled()
    {
        equippedItemPopupTween.HideWithScale();
        pendingSellSlots.Clear();
    }

    #endregion

    // ─────────────────────────────────────────
    #region 팝업: 장비 낀 오리
    // ─────────────────────────────────────────

    void ShowDuckWithEquipPopup(int duckCount, int equipCount)
    {
        pendingSellSlots = new List<SellCardSlot>(selectedSlots);

        string msg = duckCount == 1
            ? $"선택한 오리에 장비가 {equipCount}개 장착되어 있습니다."
            : $"선택한 오리 {duckCount}마리에 장비가 총 {equipCount}개 장착되어 있습니다.";

        duckWithEquipPopupText.text = msg;
        duckWithEquipPopupTween.ShowWithScale();
    }

    void OnDuckWithEquipConfirmed(bool sellEquipmentsToo)
    {
        duckWithEquipPopupTween.HideWithScale();
        ExecuteSell(pendingSellSlots, sellEquipmentsToo);
    }

    void OnDuckWithEquipCanceled()
    {
        duckWithEquipPopupTween.HideWithScale();
        pendingSellSlots.Clear();
    }

    #endregion

    // ─────────────────────────────────────────
    #region 오리 1마리 경고
    // ─────────────────────────────────────────

    void ShowLastDuckWarning()
    {
        if (lastDuckWarningPopupTween != null)
            lastDuckWarningPopupTween.ShowWithScale();
        else if (lastDuckWarningPopup != null)
            lastDuckWarningPopup.SetActive(true);

        Logger.Log("[SellPanelManager] 오리가 1마리 이상 남아야 합니다.");
    }
    #endregion

    // ─────────────────────────────────────────
    #region 필수 무기 장비 판매 금지 경고
    // ─────────────────────────────────────────
    void ShowEssentialItemWarning(int count)
    {
        if (essentialItemWarningPopupTween != null)
            essentialItemWarningPopupTween.ShowWithScale();
        else if (essentialItemWarningPopup != null)
            essentialItemWarningPopup.SetActive(true);

        Logger.Log($"[SellPanelManager] 필수 장비 {count}개는 판매할 수 없습니다.");
    }
    #endregion 

    // ─────────────────────────────────────────
    #region 판매 실행
    // ─────────────────────────────────────────

    /// <summary>
    /// 실제 판매 처리
    /// sellEquipmentsToo: 오리의 장비도 함께 판매할지
    /// </summary>
    void ExecuteSell(List<SellCardSlot> slotsToSell, bool sellEquipmentsToo)
    {
        int earnedGold = 0;

        foreach (SellCardSlot slot in slotsToSell)
        {
            CardData data = slot.GetCardData();
            earnedGold += slot.GetSellPrice();

            if (data.Type == CardType.Weapon.ToString())
            {
                // 오리 판매
                SellDuck(data, sellEquipmentsToo, ref earnedGold);
            }
            else
            {
                // 아이템 판매
                SellItem(data);
            }

            // 슬롯 제거
            activeSlots.Remove(slot);
            Destroy(slot.gameObject);
        }

        // 선택 목록 정리
        selectedSlots.RemoveAll(s => slotsToSell.Contains(s));
        totalSellPrice = 0;
        foreach (SellCardSlot s in selectedSlots)
            totalSellPrice += s.GetSellPrice();
        UpdateTotalPriceUI();

        pendingSellSlots.Clear();

        // 골드 지급
        playerDataManager.AddCoin(earnedGold);

        // 코인 촤르르륵 애니메이션
        if (gemCollectFX != null && coinIconRect != null)
        {
            int animCount = Mathf.Clamp(slotsToSell.Count, 1, 10);
            gemCollectFX.PlayGemCollectFX(coinIconRect, animCount, false);
        }

        // 기존 카드 풀 슬롯 제거
        cardSlotManager.InitialSortingByGrade();

        // 장비 데이터 저장
        cardList.DelayedSaveEquipments();

        // ⭐ 클라우드 저장
        // MyCards.txt, MyEquipments.txt, playerData.json이
        // 모두 로컬에 저장된 후 클라우드에 업로드
        CloudSaveManager.Instance?.SaveToCloud();

        Logger.Log($"[SellPanelManager] 판매 완료: {slotsToSell.Count}장, +{earnedGold}골드");
    }

    void SellDuck(CardData duckData, bool sellEquipmentsToo, ref int earnedGold)
    {
        EquipmentCard[] equips = cardList.GetEquipmentsCardData(duckData);
        EquipmentCard[] equipsCopy = new EquipmentCard[equips.Length];
        equips.CopyTo(equipsCopy, 0);

        for (int i = 0; i < equipsCopy.Length; i++)
        {
            if (equipsCopy[i] == null) continue;

            CardData equipCardData = equipsCopy[i].CardData;

            // ⭐ 필수 장비 여부 확인 (해당 오리에 바인딩된 장비)
            bool isEssential = equipCardData.BindingTo == duckData.Name;

            if (sellEquipmentsToo || isEssential)
            {
                // 장비도 함께 판매이거나, 필수 장비는 무조건 판매
                // 필수 장비를 "보관" 선택해도 오리 없이는 쓸 수 없으므로 함께 판매
                earnedGold += equation.GetSellPrice(equipCardData, priceReader);
                cardList.UnEquip(duckData, equipsCopy[i]);
                cardDataManager.RemoveCardFromMyCardList(equipCardData);
                cardSlotManager.DestroySlot(equipCardData.ID);
            }
            else
            {
                // 비필수 장비는 해제 후 보관
                cardList.UnEquip(duckData, equipsCopy[i]);
                cardSlotManager.UpdateCardDisplay(equipCardData);
            }
        }

        cardDataManager.RemoveCardFromMyCardList(duckData);
        cardSlotManager.DestroySlot(duckData.ID);
    }

    void SellItem(CardData itemData)
    {
        // 장착 중이면 먼저 해제
        EquipmentCard equipCard = cardList.FindEquipmentCard(itemData);
        if (equipCard != null && equipCard.IsEquipped && equipCard.EquippedWho != null)
        {
            cardList.UnEquip(equipCard.EquippedWho, equipCard);
            cardSlotManager.UpdateCardDisplay(equipCard.EquippedWho);
        }

        cardDataManager.RemoveCardFromMyCardList(itemData);
        cardSlotManager.DestroySlot(itemData.ID);
    }

    #endregion

    // ─────────────────────────────────────────
    #region 유틸
    // ─────────────────────────────────────────

    bool IsCardEquipped(CardData data)
    {
        if (data.Type == CardType.Item.ToString())
        {
            EquipmentCard ec = cardList.FindEquipmentCard(data);
            return ec != null && ec.IsEquipped;
        }
        return false;
    }

    bool HasEquipment(CardData duckData)
    {
        EquipmentCard[] equips = cardList.GetEquipmentsCardData(duckData);
        foreach (var e in equips)
            if (e != null) return true;
        return false;
    }

    int GetEquipmentCount(CardData duckData)
    {
        int count = 0;
        EquipmentCard[] equips = cardList.GetEquipmentsCardData(duckData);
        foreach (var e in equips)
            if (e != null) count++;
        return count;
    }

    void CloseAllPopups()
    {
        if (equippedItemPopup != null)  equippedItemPopup.SetActive(false);
        if (duckWithEquipPopup != null) duckWithEquipPopup.SetActive(false);
    }

    #endregion
}