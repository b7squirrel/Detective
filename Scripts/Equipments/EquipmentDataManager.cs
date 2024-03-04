using System.Collections.Generic;
using System.Collections;
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
        charID = int.Parse(_ori);
        IDs[0] = int.Parse(_head);
        IDs[1] = int.Parse(_chest);
        IDs[2] = int.Parse(_legs);
        IDs[3] = int.Parse(_weapon);
    }
    public int charID;
    public int[] IDs = new int[4];
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
        GetComponent<CardList>().InitCardList();
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
    // Card List의 Equip과 UnEquip이 호출.
    public void UpdateEquipment(CharCard charCard, int Equipmentindex)
    {
        // 오리ID 비교
        CardEquipmentData charEquipData = MyEquipmentsList.Find(x => x.charID == charCard.CardData.ID);
        CardData charCardData = charCard.CardData;

        int[] equipmentCardID = new int[4];

        // 쓰기 편하게 equipmentCardID 에 아이디를 저장
        for (int i = 0; i < 4; i++)
        {
            if (charCard.equipmentCards[i] != null)
                equipmentCardID[i] = charCard.equipmentCards[i].CardData.ID;
        }

        // 해당 charCard의 장비 데이터가 존재하지 않으면 새로 생성해서 저장
        if (charEquipData == null)
        {
            CardEquipmentData newEquipData =
                new CardEquipmentData(charCardData.ID.ToString(), 
                                        equipmentCardID[0].ToString(), 
                                        equipmentCardID[1].ToString(), 
                                        equipmentCardID[2].ToString(), 
                                        equipmentCardID[3].ToString());

            MyEquipmentsList.Add(newEquipData);
        }
        // 해당 charCard의 장비 데이터가 존재한다면 바뀌는 장비만 교체하고 저장
        else
        {
            charEquipData.IDs[Equipmentindex] = equipmentCardID[Equipmentindex];
        }

        if(filePath != null)
        {
            Save();
            
        }
        else
        {
            StartCoroutine(WaitToStartMethodRunning());
        }
    }

    IEnumerator WaitToStartMethodRunning()
    {
        yield return new WaitForSeconds(.04f);
        Save();
    }
}
