using System;

public class GradeConverter
{
    public int ConvertStringToInt(string _grade)
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
}

public enum EquipmentType { Head, Chest, Legs, Weapon }

public class EquipmentTypeConverter
{
    public int ConvertStringToInt(string _equipType)
    {
        EquipmentType[] allEquipType = (EquipmentType[])Enum.GetValues(typeof(EquipmentType));

        for (int i = 0; i < allEquipType.Length; i++)
        {
            if(allEquipType[i].ToString() == _equipType)
            {
                return i;
            }
        }
        return -1;
    }
}
