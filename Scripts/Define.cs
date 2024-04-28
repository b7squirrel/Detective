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

// Slot Pooling
public enum Slots { CardSlot, EquipSlot, LaunchSlot }
public class MyGrade
{
    public static int Common = 0;
    public static int Rare = 1;
    public static int Epic = 2;
    public static int Unique = 3;
    public static int Legendary = 4;
}

public class StaticValues
{
    public static int MaxLevel = 30;
    public static int MaxEvoStage = 3; // evo stage 0, 1, 2
    public static int MaxGrade = 5; // grade 0, 1, 2, 3, 4
    public static int MaxItemGrade = 3; // grade 0, 1, 2
    public static int MaxSkillNumbers = 5; // skill 1, 2, 3, 4, 5 : 3자리수의 백자리가 0이 될 수 없음
    public static int MaxEnemyNumbers;
    public static int MaxGemNumbers;
}

public class Equation
{
    public int GetDamage(int _originalDamage, int _damageBonus)
    {
        int damage = (int)(_originalDamage + (_originalDamage * _damageBonus / 100));
        return damage;
    }

    public int GetCriticalDamage(int _damage)
    {
        int criticalCoefficient = UnityEngine.Random.Range(5, 9);
        int criticalConstant = UnityEngine.Random.Range(1, 100);
        int _cDamage = (_damage * criticalCoefficient) + criticalConstant;
        return _cDamage;
    }

    public float GetCoolDownTime(float _rate, int _grade, int _evoStage, float _defaultCoolDownTime)
    {
        return _defaultCoolDownTime - (_rate * ((_grade + 4) + _evoStage));
    }

    public int GetSkillDamage(float _rate, int _grade, int _evoStage, float _defaultDamage)
    {
        return (int)(_defaultDamage * (int)(_rate * ((_grade + 4) + _evoStage)));
    }

    public float GetSkillDuration(float _rate, int _grade, int _evoStage, float _defaultDuration)
    {
        return _defaultDuration + (_rate * ((_grade + 4) + _evoStage));
    }

    public int GetSkillDamageBonus(float _rate, int _grade, int _evoStage, float _defaultDamageBonus)
    {
        return (int)(_defaultDamageBonus * (int)(8 * (((_grade + 1) * 1.5f) + _evoStage)));
    }

    public float GetSlowSpeedFactor(int _grade, int _evoStage)
    {
        int slownessFactor = 5;
        return .01f * (slownessFactor * ((_grade * 5) + (_evoStage * 2)) + 90f);
    }
}
