using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

[System.Serializable]
public class EquipmentCard
{
    public EquipmentCard(CardData _cardData)
    {
        CardData = _cardData;
        IsEquipped = false;
    }
    public CardData CardData;
    public bool IsEquipped;
}

public class CardList : MonoBehaviour
{
    [SerializeField] List<CharCard> charCards;
    [SerializeField] List<EquipmentCard> equipmentCards;

    CardDataManager cardDataManager;

    void Awake()
    {
        cardDataManager = GetComponent<CardDataManager>();
    }

    public void Equip(CardData charData, CardData equipData)
    {
        CharCard charCard = FindCharCard(charData);
        EquipmentCard equipmentCard = FindEquipmentCard(equipData);

        int index = new EquipmentTypeConverter().ConvertStringToInt(equipData.EquipmentType);
        charCard.equipmentCards[index] = equipmentCard;
        equipmentCard.IsEquipped = true;
    }
    public void UnEquip(CardData charData, CardData equipData)
    {
        CharCard charCard = FindCharCard(charData);
        EquipmentCard equipmentCard = FindEquipmentCard(equipData);

        int index = new EquipmentTypeConverter().ConvertStringToInt(equipData.EquipmentType);
        charCard.equipmentCards[index] = null;
        equipmentCard.IsEquipped = false;
    }
    CharCard FindCharCard(CardData cardData)
    {
        CharCard card = charCards.Find(x => x.CardData.ID == cardData.ID);
        return card;
    }
    public EquipmentCard FindEquipmentCard(CardData cardData)
    {
        EquipmentCard card = equipmentCards.Find(x => x.CardData.ID == cardData.ID);
        return card;
    }
    public void InitCardList()
    {
        charCards = new();
        equipmentCards = new();
        
        List<CardData> myCardList = cardDataManager.GetMyCardList();
        foreach (var item in myCardList)
        {
            if(item.Type == CardType.Weapon.ToString())
            {
                CharCard charCard = new(item);
                charCards.Add(charCard);
            }
            else if(item.Type == CardType.Item.ToString())
            {
                EquipmentCard equipCard = new(item);
                equipmentCards.Add(equipCard);
            }
        }
    }
}
