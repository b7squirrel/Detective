using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

// Slot Action
public enum SlotType { Field, Up, Mat, None };
public enum EquipSlotType { FieldOri, FieldEquipment, UpEquipment, None }
public enum LaunchSlotType { Field, Up, None }

// Card Slot
public enum TargetSlot { UpField, MatField, UpSlot, MatSlot } // Target Slot to move to when a slot is clicked on.

// Card Data
public enum CardType { Weapon, Item, none }
public enum Grade { Common, Rare, Epic, Unique, Legendary }
public enum EquipmentType { Head, Chest, Face, Hand, Ori }
public enum EssentialEquip { Head, Chest, Face, Hand, Essential }
public enum StartingMember { Zero, First, Second, Third, Forth, Fifth }
public enum DefaultItem { Default }

public class MyGrade
{
    public static int Common = 0;
    public static int Rare = 1;
    public static int Epic = 2;
    public static int Unique = 3;
    public static int Legendary = 4;

}