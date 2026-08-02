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
        if (_equipmentType == "Head") return 0;
        if (_equipmentType == "Chest") return 1;
        if (_equipmentType == "Face") return 2;
        if (_equipmentType == "Hand") return 3;
        if (_equipmentType == "Ori") return 4;
        return -1;
    }
}

public class CardClassifier
{
    // 내가 가진 카드들 중 업슬롯에 올라와 있는 카드와 등급이 같으면 추려내는 메서드
    public List<CardData> GetCardsAvailableForMat(List<CardData> myCardsExceptUpCard, CardData upCard)
    {
        List<CardData> cardsPicked = new();

        foreach (CardData card in myCardsExceptUpCard)
        {
            if (MergeConditions.IsValidMaterial(card, upCard, out _))
            {
                cardsPicked.Add(card);
            }
        }
        return cardsPicked;
    }
}