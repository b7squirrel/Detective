using System;
using System.Collections.Generic;
using System.Diagnostics;

// Slot Action
public enum SlotType { Field, Up, Mat, None };
public enum EquipSlotType { FieldOri, FieldEquipment, UpEquipment, None }
public enum LaunchSlotType { Field, Up, None }

// Card Slot
public enum TargetSlot { UpField, MatField, UpSlot, MatSlot } // Ŭ���Ǿ��� �� �̵��� ����

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
// �ش� ���ī�尡 �ʼ� ������� �˷���
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
    // ���� ���� ī��� �� �����Կ� �ö�� �ִ� ī��� �̸��� ����� ������ �߷����� �޼���
    public List<CardData> GetCardsAvailableForMat(List<CardData> myCardsExceptUpCard, CardData upCard)
    {
        List<CardData> cardsPicked = new(); // ��ᰡ �� �� �ִ� ī�� ���
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