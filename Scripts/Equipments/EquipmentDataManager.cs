using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 내가 구글시트로 작성할 필요 없는 데이터
/// </summary>
[System.Serializable]
public class CardEquipmentData
{
    public CardEquipmentData(string _ori, string _head, string _chest,
            string _legs, string _weapon)
    {
        charID = _ori;
        IDs[0] = _head;
        IDs[1] = _chest;
        IDs[2] = _legs;
        IDs[3] = _weapon;
    }
    public string charID;
    public string[] IDs = new string[4];
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
            cardStatsList.Add(new CardEquipmentData(row[0], row[1], row[2], row[3], row[4]));
        }
        return cardStatsList;
    }
}
public class EquipmentDataManager : MonoBehaviour
{
    public List<CardEquipmentData> MyEquipmentsList;
    string filePath;
    string myEquips = "MyEquipments.txt";

    void Start()
    {
        if (Directory.Exists(Application.persistentDataPath + "/MyEquipmentsData") == false)
            Directory.CreateDirectory(Application.persistentDataPath + "/MyEquipmentsData");

        filePath = Application.persistentDataPath + "/MyEquipmentsData/" + myEquips;

        Load();
        FindAnyObjectByType<CardList>().InitCardList();
    }

    void Save()
    {
        string jsonData = JsonUtility.ToJson(new Serialization<CardEquipmentData>(MyEquipmentsList), true);
        File.WriteAllText(filePath, jsonData);
    }

    void Load()
    {
        // 로드할 파일이 있으면 로드, 없으면 아무것도 안함
        if (!File.Exists(filePath))
        {
            return;
        }
        string jdata = File.ReadAllText(filePath);
        MyEquipmentsList = JsonUtility.FromJson<Serialization<CardEquipmentData>>(jdata).Data;
    }

    public List<CardEquipmentData> GetMyEquipmentsList()
    {
        if (MyEquipmentsList == null) Debug.Log("리스트 널");
        return MyEquipmentsList;
    }

    // 아이디로 charCard를 찾아서 장비 데이터(My Equipment List) 업데이트
    public void UpdateEquipment(CharCard charCard)
    {
        // 오리ID 비교
        CardEquipmentData charEquipData = MyEquipmentsList.Find(x => x.charID == charCard.CardData.ID);
        CardData charCardData = charCard.CardData;

        string[] equipmentCardID = new string[4];


        for (int i = 0; i < 4; i++)
        {
            if (charCard.equipmentCards[i] != null)
                equipmentCardID[i] = charCard.equipmentCards[i].CardData.ID;
        }

        if (charEquipData == null)
        {
            CardEquipmentData newEquipData =
                new CardEquipmentData(charCardData.ID, equipmentCardID[0], equipmentCardID[1], equipmentCardID[2], equipmentCardID[3]);

            MyEquipmentsList.Add(newEquipData);
        }
        else
        {
            charEquipData.charID = charCardData.ID;

            for (int i = 0; i < equipmentCardID.Length; i++)
            {
                charEquipData.IDs[i] = equipmentCardID[i];
            }
        }

        Save();
    }
}
