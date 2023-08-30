using System;
using System.Collections;
using UnityEngine;

public class WeaponItemData
{
    public WeaponItemData(WeaponData _weaponData, Item _itemData)
    {
        weaponData = _weaponData;
        itemData = _itemData;
    }
    public WeaponData weaponData;
    public Item itemData;
}

public class SlotUpCard : MonoBehaviour
{
    #region 카드 관련 변수
    CardData cardToUpgrade; // 업그레이드 슬롯에 올라가 있는 카드
    CardData cardToFeed; // 재료로 쓸 카드. 지금 드래그 하는 카드
    bool isEmpty;
    #endregion

    #region 참조 변수
    CardsDictionary cardDictionary;
    CardDataManager cardDataManager;
    [SerializeField] SlotManager slotManager;
    [SerializeField] CardSlot upCardSlot;
    [SerializeField] CardSlot matCardSlot;

    #endregion

    #region 액션 이벤트 - SlotUpCardUI
    public event Action OnCardAcquiredOnUpSlotUI;
    public event Action OnCardAcquiredOnMatSlotUI;
    public event Action OnRefreshUI; // 슬롯을 비우는 등의 리프레시 UI
    public event Action OnUpdateUI; // 매 프레임 업데이트 되는 UI
    public event Action OnUpgradeConfirmation; // 합성 확인 창 UI
    public event Action OnCloseUpgradeConfirmation; // 합성 확인 창 UI 끌 때
    public event Action OnMerging; // 합성 효과 UI
    #endregion

    #region Unity Callback 함수

    void Awake()
    {
        cardDictionary = FindObjectOfType<CardsDictionary>();
        cardDataManager = FindObjectOfType<CardDataManager>();
        isEmpty = true;
        upCardSlot.EmptySlot();
        matCardSlot.EmptySlot();
    }

    void Update()
    {
        // if (GetComponentInChildren<CardSlot>().IsEmpty() == true)
        // {
        //     cardToUpgrade = null;
        //     return; // 업그레이드 슬롯에 카드가 없다면 아무것도 안함
        // }

        // OnUpdateUI?.Invoke();
    }
    #endregion

    #region Acquire Card
    // 업그레이드 슬롯위에 카드를 올릴 때
    // 업그레이드를 할 카드인지 재료 카드인지 구분해서 처리
    public void AcquireCard(CardData cardData)
    {
        if (cardToUpgrade == null) // 업그레이드 슬롯이 비어 있다면
        {
            cardToUpgrade = cardData;
            
            // onCardAcquired
            OnCardAcquiredOnUpSlotUI?.Invoke();

            // 재료카드 패널 열기. SlotManager, SlotAllCards의 함수들 등록
            slotManager.GetIntoMatCards();

            // 업그레이드 슬롯 위 카드 표시
            if (cardData.Type == CardType.Weapon.ToString())
            {
                WeaponData wData = cardDictionary.GetWeaponData(cardData);
                WeaponItemData data = new(wData, null);
                upCardSlot.SetWeaponCard(cardData, data.weaponData, TargetSlot.MatSlot);
            }
            else
            {
                Item iData = cardDictionary.GetItemData(cardData);
                WeaponItemData data = new WeaponItemData(null, iData);
                upCardSlot.SetItemCard(cardData, data.itemData, TargetSlot.MatSlot);
            }
        }
        else
        {
            cardToFeed = cardData;; // 비어 있지 않다면 지금 카드는 재료 카드임

            // 재료 슬롯 위 카드 표시
            if (cardData.Type == CardType.Weapon.ToString())
            {
                WeaponData wData = cardDictionary.GetWeaponData(cardData);
                WeaponItemData data = new(wData, null);
                matCardSlot.SetWeaponCard(cardData, data.weaponData, TargetSlot.MatSlot);
            }
            else
            {
                Item iData = cardDictionary.GetItemData(cardData);
                WeaponItemData data = new WeaponItemData(null, iData);
                matCardSlot.SetItemCard(cardData, data.itemData, TargetSlot.MatSlot);
            }

            OnCardAcquiredOnMatSlotUI?.Invoke();
            OnUpgradeConfirmation?.Invoke();
            slotManager.ClearMatCardsSlots();
        }
    }
    #endregion

    #region GetCard
    public CardData GetCardDataToUpgrade()
    {
        return cardToUpgrade;
    }
    public CardData GetCardDataToFeed()
    {
        return cardToFeed;
    }
    #endregion

    #region Check Slot Availability
    public SlotType GetSlotType(CardData cardData) // Draggable에서 카드를 놓을 수 있는지 여부를 판단
    {
        // 최고 레벨 카드는 올릴 수 없음
        if(cardData.Grade == "Legendary")
        {
            Debug.Log("최고 레벨 카드는 업그레이드 할 수 없습니다");
            return SlotType.none;
        }

        // 업그레이드 카드의 경우
        if (cardToUpgrade == null) // 슬롯 위에 카드가 없다면 무조건 올릴 수 있다
        {
            return SlotType.upSlot;
        }

        // 재료카드에 카드가 올라가 있는 경우
        if(cardToFeed != null)
        {
            return SlotType.none;
        }

        // 재료 카드의 경우
        string upgradeCardGrade = cardToUpgrade.Grade;
        string feedCardGrade = cardData.Grade;
        string upgradeCardName = cardToUpgrade.Name;
        string feedCardName = cardData.Name;

        if (upgradeCardGrade != feedCardGrade)
        {
            Debug.Log("같은 등급을 합쳐줘야 합니다");
            return SlotType.none;
        }

        if (upgradeCardName != feedCardName)
        {
            Debug.Log("같은 이름의 카드를 합쳐줘야 합니다.");
            return SlotType.none;
        }

        return SlotType.matSlot;
    }
    #endregion

    #region 업그레이드
    public void UpgradeCard()
    {
        int newCardGrade = int.Parse(cardToUpgrade.Grade) + 1;
        if (newCardGrade > 4) {Debug.Log("업그레이드 된 카드가 최고등급을 넘습니다. 확인 할 것");}

        string newGrade = ((Grade)newCardGrade).ToString();
        string type = cardToUpgrade.Type;

        // 생성된 카드를 내 카드 리스트에 저장
        CardData newCardData = cardDataManager.GenNewCardData(type, newGrade, cardToUpgrade.Name);
        cardDataManager.AddCardToMyCardsList(newCardData);
        cardDataManager.RemoveCardFromMyCardList(cardToUpgrade);// 카드 데이터 삭제
        cardDataManager.RemoveCardFromMyCardList(cardToFeed);

        // 합성 연출 후 강화 성공 패널로
        int index = cardDataManager.MyCardsList.Count-1;
        StartCoroutine(UpgradeUICo(newCardData));
    }

    IEnumerator UpgradeUICo(CardData upgradedCardData)
    {
        // 강화 연출 UI
        OnMerging?.Invoke();
        OnCloseUpgradeConfirmation?.Invoke();

        yield return new WaitForSeconds(.16f);

        // 강화 연출이 끝나면 슬롯 리프레시가 필요함
        
        


        // WeaponData, ItemData
        if(upgradedCardData.Type == CardType.Weapon.ToString())
        {
            WeaponData wData = cardDictionary.GetWeaponData(upgradedCardData);
            WeaponItemData data = new WeaponItemData(wData, null);

            // 강화 성공 패널로
            slotManager.OpenUpgradeSuccesUI(data);
        }
        else
        {
            Item iData = cardDictionary.GetItemData(upgradedCardData);
            WeaponItemData data = new WeaponItemData(null, iData);

            // 강화 성공 패널로
            slotManager.OpenUpgradeSuccesUI(data);
        }
    }
    #endregion

    #region Refresh
    public void ClearUpgradeSlot()
    {
        OnRefreshUI?.Invoke();
    }
    #endregion
}