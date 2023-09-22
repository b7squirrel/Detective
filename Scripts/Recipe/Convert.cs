using System;

public enum EquipmentType { Head, Chest, Legs, Weapon }
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