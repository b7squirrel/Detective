using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour
{
    SlotsAllCards slotsAllCards;
    SlotsMatCards slotsMatCards;
    SlotUpCard slotUpCard;

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
        slotsAllCards.ClearMyCardsSlots();
        slotsMatCards.ClearmatCardsSlots();
        slotUpCard.ClearUpgradeSlot();
    }
    // MainMenuManger가 Awake에서 모든 패널을 비활성화 시키기 전에 
    // Refresh를 실행하는 것을 방지하기 위해 (NullReference)
    // 0.01초 후에 리프레시를 하도록 했음
    #endregion

    #region MyCards, MatCards 전환
    public void GetIntoMatCards()
    {
        slotsAllCards.gameObject.SetActive(false);
        slotsMatCards.gameObject.SetActive(true);

        // MatCards 리스트 생성
        GenerateMatCardsList();
    }

    public void GetIntoMyCards()
    {
        slotsAllCards.gameObject.SetActive(true);
        slotsMatCards.gameObject.SetActive(false);
        RefreshUpgradePanel();
    }

    public void BackToMatCards()
    {
        slotsAllCards.gameObject.SetActive(false);
        slotsMatCards.gameObject.SetActive(true);

        // MatCards 리스트 복구
        List<Card> matCardsList = new();
        matCardsList.AddRange(slotsMatCards.GetMMatCards());
        slotsMatCards.ClearmatCardsSlots();
        slotsMatCards.SetMatCards(matCardsList);
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
    public void OpenUpgradeSuccesUI()
    {
        upgradeSuccessUI.gameObject.SetActive(true);
    }
    public void CloseUpgradeSuccessUI()
    {
        upgradeSuccessUI.gameObject.SetActive(false);
    }
    #endregion
}
