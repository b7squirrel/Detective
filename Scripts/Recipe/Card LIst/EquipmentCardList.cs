using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentCard
{
    public EquipmentCard(CardData _cardData)
    {
        CardData = _cardData;
        isEquipped = false;
    }
    public CardData CardData;
    public bool isEquipped;
}

public class EquipmentCardList : MonoBehaviour
{
    
}
