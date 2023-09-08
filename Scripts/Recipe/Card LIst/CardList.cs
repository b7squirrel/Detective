using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharCard
{
    public CharCard(CardData _cardData)
    {
        CardData = _cardData;
        equipmentCards = new EquipmentCard[4];
    }
    public CardData CardData;
    public EquipmentCard[] equipmentCards;
}

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

public class CardList : MonoBehaviour
{
    [SerializeField] List<CharCard> charCards;
    [SerializeField] List<EquipmentCard> equipmentCard;

    CardDataManager cardDataManager;

    void Awake()
    {
        cardDataManager = GetComponent<CardDataManager>();
    }

    public void Equip(CardData charData, CardData equipData)
    {
        CharCard charCard = FindCharCard(charData);
        EquipmentCard equipmentCard = FindEquipmentCard(equipData);
    }
    CharCard FindCharCard(CardData cardData)
    {
        CharCard card = charCards.Find(x => x.CardData.ID == cardData.ID);
        return card;
    }
    EquipmentCard FindEquipmentCard(CardData cardData)
    {
        EquipmentCard card = equipmentCard.Find(x => x.CardData.ID == cardData.ID);
        return card;
    }

}
