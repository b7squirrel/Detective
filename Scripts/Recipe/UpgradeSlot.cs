using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSlot : MonoBehaviour
{
    [SerializeField] public List<CardSo> cardSo;
    [SerializeField] GameObject cardPrefab; // 이걸로 새로운 카드를 생성

    Card cardToUpgrade; // 업그레이드 슬롯에 올라가 있는 카드
    Card cardToFeed; // 재료로 쓸 카드. 지금 드래그 하는 카드
    Transform previousParentOfPointerDrag; // 업그레이드 슬롯에 올려놓은 카드가 되돌아갈 위치
    bool isAvailable; // 카드가 슬롯 위에 올라올 수 있는지 여부 (업그레이드 카드든 재료 카드든)


    ItemGrade.grade upgradeCardGrade;
    ItemGrade.grade feedCardGrade;

    void Update()
    {
        if (GetComponentInChildren<Card>() == null)
        {
            cardToUpgrade = null;
            Debug.Log("업그레이드 슬롯 위에 카드가 없습니다.");
        }
    }

    public void AcquireCard(Card card) // 업그레이드 슬롯위에 카드를 올릴 때
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

    public bool Available()
    {
        isAvailable = true;
        if (cardToUpgrade != null)
        {
            upgradeCardGrade = cardToUpgrade.GetCardGrad();
        }

        if (cardToFeed != null)
        {
            feedCardGrade = cardToFeed.GetCardGrad();
        }

        if (upgradeCardGrade != feedCardGrade)
        {
            Debug.Log("같은 등급을 합쳐줘야 합니다");
            isAvailable = false;
        }

        if ((int)upgradeCardGrade == 4)
        {
            Debug.Log("전설 등급은 더 이상 강화할 수 없습니다.");
            isAvailable = false;
        }

        return isAvailable;
    }

    void UpgradeCard()
    {
        if (isAvailable == false)
            return;

        int newCardGrade = (int)cardToUpgrade.GetCardGrad() + 1;

        Destroy(cardToUpgrade.gameObject);
        Destroy(cardToFeed.gameObject);

        GameObject newCard = Instantiate(cardPrefab, transform);
        RectTransform newCardRec = newCard.GetComponent<RectTransform>();
        newCardRec = GetComponent<RectTransform>();
        newCard.GetComponent<Card>().SetCardData(cardSo[0], newCardGrade);

        Debug.Log("카드가 한 단께 높은 등급으로 합성되었습니다");
    }

    public void SetPrevParent(Transform prevParent)
    {
        previousParentOfPointerDrag = prevParent;
    }

    public Card TempGetCardOnSlot()
    {
        return cardToUpgrade;
    }
}
