using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
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

        equipmentDataManager.UpdateEquipment(charCard, index); // 데이터 업데이트 및 저장
    }
    public void UnEquip(CardData charData, EquipmentCard _equipmentCard)
    {
        CharCard charCard = FindCharCard(charData);

        int index =
                new EquipmentTypeConverter().ConvertStringToInt(_equipmentCard.CardData.EquipmentType);
        charCard.equipmentCards[index] = null;
        _equipmentCard.IsEquipped = false;

        equipmentDataManager.UpdateEquipment(charCard, index);// 데이터 업데이트 및 저장
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

    // weapon과 item을 분리해서 저장
    public void InitCardList()
    {
        charCards = new();
        equipmentCards = new();

        List<CardData> myCardList = cardDataManager.GetMyCardList();

        foreach (var item in myCardList)
        {
            if (item.Type == CardType.Weapon.ToString())
            {
                CharCard _charCard = new(item);
                charCards.Add(_charCard);
                
            }
            else if (item.Type == CardType.Item.ToString())
            {
                EquipmentCard equipCard = new(item);
                equipmentCards.Add(equipCard);
            }
        }

        // 모든 카드가 분류되고 나면 장비 데이터대로 장비 장착하기
        foreach (var item in charCards)
        {
            LoadEquipmentData(item);
        }
    }
    // charCards에 장비 데이터 로드해서 장착하기
    // 장착시킨 장비는 isEqupped로 설정하기
    void LoadEquipmentData(CharCard _charCard)
    {
        // 매개 변수로 받은 오리카드의 장비 데이터를 찾아서 equipData에 저장
        List<CardEquipmentData> myEquipmentData = equipmentDataManager.GetMyEquipmentsList();

        if (myEquipmentData.Count == 0) return;

        CardEquipmentData equipData = myEquipmentData.Find(x => x.charID == _charCard.CardData.ID);

        if (equipData == null) return;
        // ID로 각각의 EquipmentCard를 찾아서 장비 카드만 모아둔 equipmentCards 리스트에서 찾아 준다
        for (int i = 0; i < 4; i++)
        {
            Debug.Log("id of the card to equip = " + equipData.IDs[i]);
            if (equipData.IDs[i] != "")
            {
                CardData cardToEquip = FindCardDataByID(equipData.IDs[i]);
                Debug.Log("name of the card to equip = " + cardToEquip.Name);
                Equip(_charCard.CardData, cardToEquip);
            }
            else if(equipData.IDs[i] == "")
            {
                Debug.Log("장비가 장착되어 있지 않습니다.");
            }
        }
        // 저장되어 있는 데이터를 가져와서 반영하는 것이므로 또 저장할 필요가 없다.
    }
    CardData FindCardDataByID(string cardID)
    {
        EquipmentCard equipmentCard = equipmentCards.Find(x => x.CardData.ID == cardID);
        return equipmentCard.CardData;
    }

}
