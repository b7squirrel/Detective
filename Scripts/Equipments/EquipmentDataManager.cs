using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class CardEquipmentData
{
    public CardEquipmentData(string _ori, string _head, string _chest,
            string _legs, string _gloves, string _weapon)
    {
        ori = _ori;
        head = _head;
        chest = _chest;
        legs = _legs;
        gloves = _gloves;
        weapon = _weapon;
    }

    public string ori, head, chest, legs, gloves, weapon;
}

[System.Serializable]
public class ReadEquipmentsData
{
    public List<CardEquipmentData> cardStatsList;
    public List<CardEquipmentData> GetCardEquipmentsList(TextAsset statsDataText)
    {
        cardStatsList = new();

        string[] line = statsDataText.text.Substring(0, statsDataText.text.Length - 1).Split('\n');
        for (int i = 0; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');
            cardStatsList.Add(new CardEquipmentData(row[0], row[1], row[2], row[3], row[4],
                                row[5]));
        }
        return cardStatsList;
    }
}
public class EquipmentDataManager : MonoBehaviour
{
    // public TextAsset startingEquipments;
    public List<CardEquipmentData> MyEquipmentsList;
    string filePath;
    string myEquips = "MyEquipments.txt";

    void Start()
    {
        if (Directory.Exists(Application.persistentDataPath + "/MyEquipmentsData") == false)
            Directory.CreateDirectory(Application.persistentDataPath + "/MyEquipmentsData");

        filePath = Application.persistentDataPath + "/MyEquipmentsData/" + myEquips;
        
        Load();
    }

    void Save()
    {
        string jsonData = JsonUtility.ToJson(new Serialization<CardEquipmentData>(MyEquipmentsList), true);
        File.WriteAllText(filePath, jsonData);
    }

    void Load()
    {
        // 로드할 파일이 있으면 로드
        // 없으면 아무것도 안함
        if (!File.Exists(filePath))
        {
            // InitEquipments();
            return;
        }
        string jdata = File.ReadAllText(filePath);
        MyEquipmentsList = JsonUtility.FromJson<Serialization<CardEquipmentData>>(jdata).Data;
    }

    // void InitEquipments()
    // {
    //     MyEquipmentsList.Clear();
    //     List<CardEquipmentData> startingEquips =
    //                 new ReadEquipmentsData().GetCardEquipmentsList(startingEquipments);

    //     MyEquipmentsList.AddRange(startingEquips);
    //     Save();
    //     Load();
    // }

    public List<CardEquipmentData> GetMyEquipmentsList()
    {
        if (MyEquipmentsList == null) Debug.Log("리스트 널");
        return MyEquipmentsList;
    }
}
