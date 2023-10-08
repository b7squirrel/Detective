using System;
using System.Collections.Generic;
using System.Diagnostics;

// Slot Action
public enum SlotType { Field, Up, Mat, None };
public enum EquipSlotType { FieldOri, FieldEquipment, UpEquipment, None }
public enum LaunchSlotType { Field, Up, None }

// Card Slot
public enum TargetSlot { UpField, MatField, UpSlot, MatSlot } // 클릭되었을 때 이동할 슬롯

// Card Data
public enum CardType { Weapon, Item, none }
public enum Grade { Common, Rare, Epic, Unique, Legendary }
public enum EquipmentType { Head, Chest, Face, Hand, Ori }
public enum EssentialEquip { Head, Chest, Face, Hand, Default }
public enum StartingMember { Zero, First, Second, Third, Forth, Fifth }
public enum DefaultItem { Default }


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
// 해당 장비카드가 필수 장비인지 알려줌
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
        List<CardData> cardsPicked = new(); // 재료가 될 수 있는 카드 목록
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