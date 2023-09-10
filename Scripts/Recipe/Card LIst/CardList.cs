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
    EquipmentDataManager equipmentDataManager;

    void Awake()
    {
        cardDataManager = GetComponent<CardDataManager>();
        equipmentDataManager = GetComponent<EquipmentDataManager>();
    }

    public void Equip(CardData charData, CardData equipData)
    {
        CharCard charCard = FindCharCard(charData);
        EquipmentCard equipmentCard = FindEquipmentCard(equipData);

        int index = new EquipmentTypeConverter().ConvertStringToInt(equipData.EquipmentType);
        charCard.equipmentCards[index] = equipmentCard;
        equipmentCard.IsEquipped = true;

        equipmentDataManager.UpdateEquipment(charCard); // 데이터 업데이트 및 저장
    }
    public void UnEquip(CardData charData, EquipmentCard _equipmentCard)
    {
        CharCard charCard = FindCharCard(charData);

        int index = 
                new EquipmentTypeConverter().ConvertStringToInt(_equipmentCard.CardData.EquipmentType);
        charCard.equipmentCards[index] = null;
        _equipmentCard.IsEquipped = false;

        equipmentDataManager.UpdateEquipment(charCard);// 데이터 업데이트 및 저장
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
    public EquipmentCard[] GetEquipmentsCardData(CardData charCardData)
    {
        return FindCharCard(charCardData).equipmentCards;
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


                // charCards에 장비 데이터 로드해서 장착하기
                // 장착시킨 장비는 isEqupped로 설정하기
            }
            else if(item.Type == CardType.Item.ToString())
            {
                EquipmentCard equipCard = new(item);
                equipmentCards.Add(equipCard);
            }
        }
    }
    void LoadEquipmentData(CharCard _charCard)
    {
        // 해당 장비 데이터를 찾아서
        List<CardEquipmentData> myEquipmentData = equipmentDataManager.GetMyEquipmentsList();
        string charID = _charCard.CardData.ID;
        CardEquipmentData equipData = myEquipmentData.Find(x => x.IDs[0] == charID);

        // ID로 각각의 EquipmentCard를 찾아서 
        EquipmentCard[] equipCard = new EquipmentCard[4];
        for (int i = 0; i < 4; i++)
        {
            
        }
    }
}
