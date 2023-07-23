using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [SerializeField] public List<CardSo> cardSo;
    [SerializeField] GameObject cardPrefab; // 이걸로 새로운 카드를 생성

    Card cardToUpgrade; // 업그레이드 슬롯에 올라가 있는 카드
    Card cardToFeed; // 재료로 쓸 카드. 지금 드래그 하는 카드

    public void SetCardToUpgrade(Card card)
    {
        cardToUpgrade = card;
    }

    public void SetCardToFeed(Card card)
    {
        cardToFeed = card;
    }

    public void UpgradeCard()
    {
        ItemGrade.grade upgradeCardGrade = cardToUpgrade.GetCardSo().grade;
        ItemGrade.grade feedCardGrade = cardToUpgrade.GetCardSo().grade;

        if(upgradeCardGrade != feedCardGrade)
        {
            Debug.Log("같은 등급을 합쳐줘야 합니다");
            return;
        }

        Debug.Log("카드가 한 단께 높은 등급으로 합성되었습니다");
    }
}
