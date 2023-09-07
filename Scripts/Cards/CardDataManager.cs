using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class Serialization<T>
{
    public Serialization(List<T> _target) => Data = _target;
    public List<T> Data;
}

[System.Serializable]
public class CardData
{
    public CardData(string _id, string _Type, string _Grade, string _Name, string _exp, string _equipmentType)
    {
        ID = _id;
        Type = _Type;
        Grade = _Grade;
        Name = _Name;
        Exp = _exp;
        EquipmentType = _equipmentType;
    }
    public string ID, Type, Grade, Name, Exp, EquipmentType;
}
public class ReadCardData
{
    public List<CardData> cardList;
    public List<CardData> GetCardsList(TextAsset cardDataText)
    {
        cardList = new List<CardData>();

        string[] line = cardDataText.text.Substring(0, cardDataText.text.Length - 1).Split('\n');
        for (int i = 0; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');
            cardList.Add(new CardData(row[0], row[1], row[2], row[3], row[4], row[5]));
        }
        return cardList;
    }
}

public class CardDataManager : MonoBehaviour
{
    public TextAsset CardDatabase;
    public TextAsset startingCardData;
    public List<CardData> AllCardsList, MyCardsList;
    string filePath;
    string myCards = "MyCards.txt";

    CardsDictionary cardDictionary;

    void Awake()
    {
        cardDictionary = GetComponent<CardsDictionary>();
    }

    void Start()
    {
        // 전체 카드 리스트 불러오기
        // AllCardsList = new ReadCardData().GetCardsList(CardDatabase);
        
        if (Directory.Exists(Application.persistentDataPath + "/PlayerData") == false)
            Directory.CreateDirectory(Application.persistentDataPath + "/PlayerData");

        filePath = Application.persistentDataPath + "/PlayerData/" + myCards;

        Load();
    }

    void Save()
    {
        string jsonData = JsonUtility.ToJson(new Serialization<CardData>(MyCardsList), true);
        File.WriteAllText(filePath, jsonData);
    }

    void Load()
    {
        if (!File.Exists(filePath))
        {
            ResetCards();
            return;
        }
        string jdata = File.ReadAllText(filePath);
        MyCardsList = JsonUtility.FromJson<Serialization<CardData>>(jdata).Data;
    }

    // 특정 카드를 가지고 시작하도록 만들려고. 아무것도 없이 시작할 수도 있다
    void ResetCards()
    {
        MyCardsList.Clear();
        List<CardData> startingCards = new ReadCardData().GetCardsList(startingCardData);
        AddNewCardToMyCardsList(startingCards[0]);
        Save();
        Load();
    }

    public List<CardData> GetMyCardList()
    {
        if(MyCardsList == null)Debug.Log("리스트 널");
        return MyCardsList;
    }

    public void RemoveCardFromMyCardList(CardData cardToRemove)
    {
        string mID = cardToRemove.ID;
        foreach (var item in MyCardsList)
        {
            if(item.ID == mID)
            {
                MyCardsList.Remove(item);
                return;
            }
            Save();
        }
    }

    // 새로운 카드는 아이디를 발급받는다
    public void AddNewCardToMyCardsList(CardData _cardData)
    {
        string _id = Guid.NewGuid().ToString();
        
        CardData newCard = 
        new CardData(_id, _cardData.Type, _cardData.Grade, _cardData.Name, "1", _cardData.EquipmentType);

        MyCardsList.Add(newCard);
        Save();
    }
    // 착용되어 있는 장비는 아이디가 바뀌면 안되므로
    public void AddUpgradedCardToMyCardList(string _id, CardData _cardData)
    {
        Debug.Log("My Id is " + _id);
        CardData newCard = 
        new CardData(_id, _cardData.Type, _cardData.Grade, _cardData.Name, "1", _cardData.EquipmentType);

        MyCardsList.Add(newCard);
        Save();
    }
    public void DeleteData()
    {
        // 해당 폴더에 있는 파일 삭제
        // string path = Application.persistentDataPath + "/PlayerData";
        // if(Directory.Exists(path))
        // {
        //     Directory.Delete(path, true);
        // }
        // else
        // {
        //     Debug.LogWarning("삭제할 데이터가 없군요.");
        // }

        ResetCards();
    }
    public CardData GenNewCardData(string _type, string _grade, string _name, string _equipmentType)
    {
        string _id = GetInstanceID().ToString();
        CardData newCard = new CardData(_id, _type, _grade, _name, "1", _equipmentType);
        return newCard;
    }
}
