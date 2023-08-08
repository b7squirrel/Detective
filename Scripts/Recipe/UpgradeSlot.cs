using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSlot : MonoBehaviour
{
    Card cardToUpgrade; // 업그레이드 슬롯에 올라가 있는 카드
    Card cardToFeed; // 재료로 쓸 카드. 지금 드래그 하는 카드
    Transform previousParentOfPointerDrag; // 업그레이드 슬롯에 올려놓은 카드가 되돌아갈 위치
    bool isAvailable; // 카드가 슬롯 위에 올라올 수 있는지 여부 (업그레이드 카드든 재료 카드든)

    

    CardsDictionary cardDictionary;
    CardDataManager cardDataManager;
    UpgradePanelSlotsManager upgradePanelSlotsManager;
    [SerializeField] UpgradeSuccessUI upgradeSuccessUI; // 왜인지 모르겠지만 FindObject... 가 안됨.

    void Awake()
    {
        cardDictionary = FindObjectOfType<CardsDictionary>();
        cardDataManager = FindObjectOfType<CardDataManager>();
        upgradePanelSlotsManager = GetComponentInParent<UpgradePanelSlotsManager>();
    }

    void Update()
    {
        if (GetComponentInChildren<Card>() == null)
        {
            cardToUpgrade = null;
        }
    }

    // 업그레이드 슬롯위에 카드를 올릴 때
    // 업그레이드를 할 카드인지 재료 카드인지 구분해서 처리
    public void AcquireCard(Card card)
    {
        if (cardToUpgrade == null) // 업그레이드 슬롯이 비어 있다면
        {
            cardToUpgrade = card;
        }
        else
        {
            AcquireMeterial(card); // 비어 있지 않다면 지금 카드는 재료 카드임
            UpgradeCard();
        }
    }

    void AcquireMeterial(Card card) // 재료가 되는 카드를 덮으면
    {
        cardToFeed = card;
    }

    public bool Available(Card card)
    {
        isAvailable = true;

        if (cardToUpgrade == null) // 슬롯 위에 카드가 없다면 무조건 올릴 수 있다
        {
            Debug.Log("슬롯 위에 카드가 없어서 그냥 올려요");
            return true;

        }

        ItemGrade.grade upgradeCardGrade = cardToUpgrade.GetCardGrade();
        ItemGrade.grade feedCardGrade = card.GetCardGrade();
        string upgradeCardName = cardToUpgrade.GetCardName();
        string feedCardName = card.GetCardName();

        Debug.Log("업그레이드 카드 등급 = " + upgradeCardGrade + "재료 카드 등급 = " + feedCardGrade);

        if (upgradeCardGrade != feedCardGrade)
        {
            Debug.Log("같은 등급을 합쳐줘야 합니다");
            isAvailable = false;
            return isAvailable;
        }

        if ((int)upgradeCardGrade == 4)
        {
            Debug.Log("전설 등급은 더 이상 강화할 수 없습니다.");
            isAvailable = false;
            return isAvailable;
        }
        if(upgradeCardName != feedCardName)
        {
            Debug.Log("같은 이름의 카드를 합쳐줘야 합니다.");
            isAvailable = false;
            return isAvailable;
        }

        return isAvailable;
    }

    void UpgradeCard()
    {
        if (isAvailable == false)
            return;

        int newCardGrade = (int)cardToUpgrade.GetCardGrade() + 1;
        string newGrade = ((ItemGrade.grade)newCardGrade).ToString();
        string type = (cardToUpgrade.GetCardType()).ToString();


        cardDataManager.RemoveCardFromMyCardList(cardToUpgrade);// 카드 데이터 삭제
        cardDataManager.RemoveCardFromMyCardList(cardToFeed);
        Destroy(cardToUpgrade.gameObject); // 실제 오브젝트 삭제
        Destroy(cardToFeed.gameObject);

        GameObject newCard = cardDictionary.GenCard(type, newGrade, cardToUpgrade.GetCardName());
        newCard.transform.SetParent(transform);
        newCard.transform.position = transform.position;
        newCard.transform.localScale = Vector3.one;
        
        Card upgraded = newCard.GetComponent<Card>();
        AddCardToList(upgraded);

        upgradeSuccessUI.gameObject.SetActive(true); // 강화 성공 패널 활성화
        upgradeSuccessUI.SetCard(upgraded); // 강화 성공 카드 초기화
    }

    public void SetPrevParent(Transform prevParent)
    {
        previousParentOfPointerDrag = prevParent;
    }

    public Card TempGetCardOnSlot()
    {
        return cardToUpgrade;
    }

    void AddCardToList(Card _card)
    {
        string type = _card.GetCardType().ToString();
        string grade = _card.GetCardGrade().ToString();
        string Name = _card.GetCardName();
        cardDataManager.AddCardToMyCardsList(type, grade, Name);
    }

    // 업그레이드 패널 최상단의 UpgradePanelSlotsManager의 OnRefresh 유니티 이벤트에 등록되어 있음
    public void ClearUpgradeSlot()
    {
        // 데이터는 이미 MyCardsList에 저장되었으니 gameObject는 제거해도 됨
        if (GetComponentInChildren<Card>() == null)
            return;
        Destroy(GetComponentInChildren<Card>().gameObject);
    }


}
