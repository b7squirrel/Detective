using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharCard
{
    public CharCard(CardData _cardData)
    {
        CardData = _cardData;
        equipments = new string[4];
    }
    public CardData CardData;
    public string[] equipments;
}

public class CharCardList : MonoBehaviour
{
    
}
