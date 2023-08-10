using System.Collections.Generic;

public class CardClassifier
{
    // 특정 등급의 카드 중 레벨이 최대치인 카드를 찾아 리스트에 담는 메서드
    public List<CardData> GetAvailableCardsForMat(List<CardData> myCardDataList, Card cardOnUpSlot)
    {
        List<CardData> cardsExceptUpCard = new List<CardData>(); // 업그레이드 카드를 제외한 내 모든 카드
        List<CardData> cardsPicked = new List<CardData>(); // 재료가 될 수 있는 카드 목록

        // 업그레이드 카드를 내 모든 카드(myCardDataList)에서 빼준다
        string _grade = cardOnUpSlot.GetCardGrade().ToString();
        string _name = cardOnUpSlot.GetCardGrade().ToString();
        foreach (CardData cardData in myCardDataList)
        {
            if (cardData.Grade == _grade && cardData.Name == _name)
                continue;
            cardsExceptUpCard.Add(cardData);
        }

        // 업그레이드 카드를 뺀 목록 중 업그레이드 카드와 같은 항목이 있다면 재료 목록에 저장
        foreach (CardData cardData in myCardDataList)
        {
            if (cardData.Grade == _grade && cardData.Name == _name)
            {
                cardsPicked.Add(cardData);
            }
        }

        return cardsPicked;
    }
}
