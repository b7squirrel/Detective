using System.Collections.Generic;

public class CardClassifier
{
    // 특정 등급의 카드 중 레벨이 최대치인 카드를 찾아 리스트에 담는 메서드
    public List<CardData> GetCardsAvailableForMat(List<CardData> myCardsExceptUpCard, CardData upCard)
    {
        List<CardData> cardsPicked = new(); // 재료가 될 수 있는 카드 목록

        foreach (CardData card in myCardsExceptUpCard)
        {
            if(card.Grade == upCard.Grade && card.Name == upCard.Name)
            {
                cardsPicked.Add(card);
            }
        }

        return cardsPicked;
    }
}
