using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour
{
    SlotsAllCards slotsAllCards;
    SlotsMatCards slotsMatCards;
    SlotUpCard slotUpCard;
    SlotUpCardUI slotUpCardUI;

    UpgradeSuccessUI upgradeSuccessUI;

    #region 유니티 콜백 함수
    void Awake()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            child.gameObject.SetActive(true);
        }

        slotsAllCards = GetComponentInChildren<SlotsAllCards>();
        slotsMatCards = GetComponentInChildren<SlotsMatCards>();
        slotUpCard = GetComponentInChildren<SlotUpCard>();
        slotUpCardUI = GetComponentInChildren<SlotUpCardUI>();
        upgradeSuccessUI = GetComponentInChildren<UpgradeSuccessUI>();
    }

    void OnEnable()
    {
        GetIntoMyCards();
    }
    #endregion

    #region Refresh

    // 업그레이드 연출이 끝나면, 업그레이드 패널을 떠났다가 되돌아오면(Disabled, Enabled)
    // 업그레이드 패널을 갱신한다
    public void RefreshUpgradePanel()
    {
        ClearMyCardsSlots();
        ClearMatCardsSlots();
        ClearUpgradeSlots();
    }
    
    public void ClearMyCardsSlots()
    {
        slotsAllCards.ClearMyCardsSlots();
    }

    public void ClearMatCardsSlots()
    {
        slotsMatCards.ClearmatCardsSlots();
    }

    public void ClearUpgradeSlots()
    {
        slotUpCard.ClearUpgradeSlot();
    }
    #endregion

    #region MyCards, MatCards 전환
    public void GetIntoMatCards()
    {
        slotsAllCards.gameObject.SetActive(false);
        slotsMatCards.gameObject.SetActive(true);

        // MatCards 리스트 생성
        GenerateMatCardsList();

        // upgrade 슬롯에 카드가 올라온 상태
        // slotUpCardUI.UpSlotState = true; 
    }

    public void GetIntoMyCards()
    {
        slotsAllCards.gameObject.SetActive(true);
        slotsMatCards.gameObject.SetActive(false);
        RefreshUpgradePanel();

        // upgrade 슬롯에 카드가 올라온 상태
        // slotUpCardUI.UpSlotState = false; 
    }

    public void BackToMatCards()
    {
        slotsAllCards.gameObject.SetActive(false);
        slotsMatCards.gameObject.SetActive(true);

        // MatCards 리스트 복구
        List<Card> matCardsList = new();
        matCardsList.AddRange(slotsMatCards.GetMMatCards());
        slotsMatCards.ClearmatCardsSlots(); // 재료 슬롯들의 카드만 갱신
        slotsMatCards.SetMatCards(matCardsList);

        slotUpCardUI.ActivateDarkPanel(false);

        // upgrade 슬롯에 카드가 올라온 상태
        // slotUpCardUI.UpSlotState = true;
    }

    public void GenerateMatCardsList()
    {
        // 슬롯 위의 카드들
        List<Card> myCards = new();
        myCards.AddRange(slotsAllCards.GetCarsOnSlots()); 

        // 업그레이드 슬롯 위의 카드
        Card cardOnUpSlot = slotUpCard.GetCardToUpgrade(); 

        // 슬롯 위의 카드들 중 업그레이드 슬롯 카드와 같은 이름, 같은 등급을 가진 카드를 추려냄
        List<Card> picked = new();
        picked.AddRange(new CardClassifier().GetCardsAvailableForMat(myCards, cardOnUpSlot));

        slotsMatCards.SetMatCards(picked);
    }
    #endregion

    #region Upgrade Success UI
    public void OpenUpgradeSuccesUI(WeaponItemData data)
    {
        slotsMatCards.gameObject.SetActive(false);



        upgradeSuccessUI.gameObject.SetActive(true);
        upgradeSuccessUI.SetCard(data); // 강화 성공 카드 초기화
        // 계속하기 버튼을 누르면 AllCardsPanel로 가면서 refresh를 하므로 여기서는 필요 없다

        // slotUpCardUI.UpSlotState = false; 
    }
    public void CloseUpgradeSuccessUI()
    {
        upgradeSuccessUI.gameObject.SetActive(false);
    }
    #endregion
}
