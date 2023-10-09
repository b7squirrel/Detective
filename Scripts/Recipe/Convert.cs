using System;
using System.Collections.Generic;

public class Convert
{
    public int GradeToInt(string _grade)
    {
        Grade[] allGrade = (Grade[])Enum.GetValues(typeof(Grade));

        for (int i = 0; i < allGrade.Length; i++)
        {
            if(allGrade[i].ToString() == _grade)
            {
                return i;
            }
        }
        return -1;
    }
    public int StringToInt(string _value)
    {
        int.TryParse(_value, out int intValue);
        return intValue;
    }
    public int EquipmentTypeToInt(string _equipmentType)
    {
        EquipmentType[] allEquipType = (EquipmentType[])Enum.GetValues(typeof(EquipmentType));

        for (int i = 0; i < allEquipType.Length; i++)
        {
            if(allEquipType[i].ToString() == _equipmentType)
            {
                return i;
            }
        }
        return -1;
    }
}
// Check if a specific equipment card is an essential card
public class CheckIsEssentialItem
{
    public bool IsEssential(CardData equipmentCard, List<CardData> cardPool)
    {
        if (equipmentCard.BindingTo == "All")
            return false;

        CardData charCardData = cardPool.Find(x => x.Name == equipmentCard.BindingTo);
        string essentialEquip = charCardData.EssentialEquip;
        if (essentialEquip == equipmentCard.EquipmentType)
        {
            return true;
        }
        return false;
    }
}

public class CardClassifier
{
    // 내가 가진 카드들 중 업슬롯에 올라와 있는 카드와 이름과 등급이 같으면 추려내는 메서드
    public List<CardData> GetCardsAvailableForMat(List<CardData> myCardsExceptUpCard, CardData upCard)
    {
        List<CardData> cardsPicked = new(); // List of Cards that can be used as material
        string essentialEquip = upCard.EssentialEquip;

        foreach (CardData card in myCardsExceptUpCard)
        {
            if (card.Grade == upCard.Grade && card.Name == upCard.Name)
            {
                cardsPicked.Add(card);
            }
        }
        return cardsPicked;
    }
}