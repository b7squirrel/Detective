using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentDataManager : MonoBehaviour
{
    [System.Serializable]
    public class EquipmentData
    {
        public EquipmentData()
        {

        }
        public string Type, Grade, Name, Exp;
    }
}
