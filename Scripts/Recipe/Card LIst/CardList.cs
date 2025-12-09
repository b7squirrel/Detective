using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharCard
{
    public CharCard(CardData _cardData)
    {
        CardData = _cardData;
        equipmentCards = new EquipmentCard[4];
        IsEquipped = false;
        for (int i = 0; i < equipmentCards.Length; i++)
        {
            equipmentCards[i] = null;
        }
        cardName = _cardData.Name + "_" + _cardData.Grade;
    }
    public string cardName;
    public int numberOfEquipments;
    public CardData CardData;
    public EquipmentCard[] equipmentCards;
    public int totalHp, totalAtk;
    public bool IsEquipped;
}

[System.Serializable]
public class EquipmentCard
{
    public EquipmentCard(CardData _cardData)
    {
        CardData = _cardData;
        IsEquipped = false;

        cardName = _cardData.Name + "_" + _cardData.Grade;
        EquippedWho = null;
    }
    public CardData EquippedWho;
    public string cardName;
    public string equippedWho;
    public CardData CardData;
    public bool IsEquipped;
}

public class CardList : MonoBehaviour
{
    [SerializeField] List<CharCard> charCards;
    [SerializeField] List<EquipmentCard> equipmentCards;
    Dictionary<int, CharCard> charCardDict;
    Dictionary<int, EquipmentCard> equipCardDict;

    CardDataManager cardDataManager;
    EquipmentDataManager equipmentDataManager;

    Convert converter;

    void Awake()
    {
        cardDataManager = GetComponent<CardDataManager>();
        equipmentDataManager = GetComponent<EquipmentDataManager>();
        converter = new Convert();
    }

    // ⭐ 배치 모드 중 즉시 카드 추가
    public void AddCardImmediately(CardData cardData)
    {
        if (cardData == null) return;
        
        // Dictionary 초기화 확인
        if (charCardDict == null) charCardDict = new Dictionary<int, CharCard>();
        if (equipCardDict == null) equipCardDict = new Dictionary<int, EquipmentCard>();
        if (charCards == null) charCards = new List<CharCard>();
        if (equipmentCards == null) equipmentCards = new List<EquipmentCard>();
        
        if (cardData.Type == CardType.Weapon.ToString())
        {
            CharCard _charCard = new CharCard(cardData);
            charCards.Add(_charCard);
            charCardDict[_charCard.CardData.ID] = _charCard;
            Logger.Log($"[CardList] 오리 카드 즉시 추가: {cardData.Name} (ID: {cardData.ID})");
        }
        else if (cardData.Type == CardType.Item.ToString())
        {
            EquipmentCard equipCard = new EquipmentCard(cardData);
            equipmentCards.Add(equipCard);
            equipCardDict[equipCard.CardData.ID] = equipCard;
            Logger.Log($"[CardList] 장비 카드 즉시 추가: {cardData.Name} (ID: {cardData.ID})");
        }
    }

    public void Equip(CardData charData, CardData equipData)
    {
        if (charData == null || equipData == null)
        {
            Logger.LogError("Cannot equip: charData or equipData is null");
            return;
        }

        CharCard charCard = FindCharCard(charData);
        EquipmentCard equipmentCard = FindEquipmentCard(equipData);

        if (charCard == null || equipmentCard == null)
        {
            Logger.LogError($"Cannot equip: charCard or equipmentCard not found in lists");
            return;
        }

        int index = converter.EquipmentTypeToInt(equipData.EquipmentType);
        charCard.equipmentCards[index] = equipmentCard;
        equipmentCard.IsEquipped = true;
        equipmentCard.EquippedWho = charCard.CardData;

        equipmentCard.equippedWho = equipmentCard.EquippedWho.Name;
        charCard.numberOfEquipments++;
        charCard.IsEquipped = true;

        EquipStats(charCard, equipData);
        equipmentDataManager.UpdateEquipment(charCard, index);
    }
    
    public void UnEquip(CardData charData, EquipmentCard _equipmentCard)
    {
        CharCard charCard = FindCharCard(charData);

        int index = converter.EquipmentTypeToInt(_equipmentCard.CardData.EquipmentType);
        charCard.equipmentCards[index] = null;
        _equipmentCard.IsEquipped = false;
        charCard.numberOfEquipments--;
        charCard.IsEquipped = charCard.numberOfEquipments > 0;

        UnEquipStats(charCard, _equipmentCard.CardData);

        equipmentDataManager.UpdateEquipment(charCard, index);
    }

    public CharCard FindCharCard(CardData charCardData)
    {
        if (charCardDict != null && charCardDict.TryGetValue(charCardData.ID, out CharCard card))
            return card;

        Logger.Log("Can't find ID " + charCardData.ID);
        return null;
    }
    
    public EquipmentCard FindEquipmentCard(CardData equipCardData)
    {
        if (equipCardDict != null && equipCardDict.TryGetValue(equipCardData.ID, out EquipmentCard card))
            return card;

        Logger.Log("Can't find ID " + equipCardData.ID);
        return null;
    }
    
    public EquipmentCard[] GetEquipmentsCardData(CardData charCardData)
    {
        CharCard charCard = FindCharCard(charCardData);
        if (charCard == null) return new EquipmentCard[4];
        return charCard.equipmentCards;
    }

    public void InitCardList()
    {
        charCards = new();
        equipmentCards = new();
        charCardDict = new();
        equipCardDict = new();

        List<CardData> myCardList = cardDataManager.GetMyCardList();

        for (int i = 0; i < myCardList.Count; i++)
        {
            if (myCardList[i].Type == CardType.Weapon.ToString())
            {
                CharCard _charCard = new(myCardList[i]);
                charCards.Add(_charCard);
                charCardDict[_charCard.CardData.ID] = _charCard;
            }
            else if (myCardList[i].Type == CardType.Item.ToString())
            {
                EquipmentCard equipCard = new(myCardList[i]);
                equipmentCards.Add(equipCard);
                equipCardDict[equipCard.CardData.ID] = equipCard;
            }
        }

        for (int i = 0; i < charCards.Count; i++)
        {
            LoadEquipmentData(charCards[i]);
        }
        
        DelayedSaveEquipments();
    }
    
    void LoadEquipmentData(CharCard _charCard)
    {
        if (_charCard == null)
        {
            Logger.LogError("Cannot load equipment data: charCard is null");
            return;
        }

        List<CardEquipmentData> myEquipmentData = equipmentDataManager.GetMyEquipmentsList();

        if (myEquipmentData == null || myEquipmentData.Count == 0)
            return;

        CardEquipmentData equipData = myEquipmentData.Find(x => x.charID == _charCard.CardData.ID);

        if (equipData == null)
            return;

        for (int i = 0; i < 4; i++)
        {
            if (equipData.IDs[i] > 0)
            {
                EquipmentCard equipCard = FindCardDataByID(equipData.IDs[i]);
                if (equipCard != null)
                {
                    Equip(_charCard.CardData, equipCard.CardData);
                }
                else
                {
                    Logger.LogWarning($"Equipment with ID {equipData.IDs[i]} not found");
                }
            }
        }
    }
    
    EquipmentCard FindCardDataByID(int cardID)
    {
        if (equipCardDict != null && equipCardDict.TryGetValue(cardID, out EquipmentCard card))
            return card;

        return null;
    }

    void EquipStats(CharCard _charCard, CardData _equipCard)
    {
        _charCard.totalHp += _equipCard.Hp;
        _charCard.totalAtk += _equipCard.Atk;
    }
    
    void UnEquipStats(CharCard _charCard, CardData _equipCard)
    {
        _charCard.totalHp -= _equipCard.Hp;
        _charCard.totalAtk -= _equipCard.Atk;
    }

    public List<EquipmentCard> GetEquipmentCardsList()
    {
        return equipmentCards;
    }

    public void ImmediateSaveEquipment()
    {
        equipmentDataManager.ImmediateSave();
    }
    
    public void DelayedSaveEquipments()
    {
        equipmentDataManager.DelayedSave();
        Logger.Log("Save On Card List");
    }

    public List<CardData> GetEquipCardDataOf(CardData oriCardData)
    {
        CharCard charCard = FindCharCard(oriCardData);
        if (charCard == null)
        {
            Logger.LogWarning($"[CardList] CharCard not found for {oriCardData.Name}");
            return new List<CardData>();
        }
        
        Logger.Log($"{charCard.cardName}");
        List<CardData> equipCardDatas = new();
        for (int i = 0; i < 4; i++)
        {
            if(charCard.equipmentCards[i] == null) continue;
            equipCardDatas.Add(charCard.equipmentCards[i].CardData);
        }

        return equipCardDatas;
    } 
}