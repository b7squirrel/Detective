using System.Collections.Generic;

public class CardClassifier
{
    // 특정 등급의 카드 중 레벨이 최대치인 카드를 찾아 리스트에 담는 메서드
    public List<Card> GetCardsAvailableForMat(List<Card> myCardsExceptUpCard, Card upCard)
    {
        List<Card> cardsPicked = new List<Card>(); // 재료가 될 수 있는 카드 목록

        foreach (Card card in myCardsExceptUpCard)
        {
            if(card.GetCardGrade() == upCard.GetCardGrade() && card.GetCardName() == upCard.GetCardName())
            {
                cardsPicked.Add(card);
            }
        }

        return cardsPicked;
    }
}
