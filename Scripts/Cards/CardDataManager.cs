using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class Serialization<T>
{
    public Serialization(List<T> _target) => MyCards = _target;
    public List<T> MyCards;
}

[System.Serializable]
public class CardData
{
    public CardData(string _id, string _Type, string _Grade, string _Name, string _exp)
    {
        ID = _id;
        Type = _Type;
        Grade = _Grade;
        Name = _Name;
        Exp = _exp;
    }
    public string ID, Type, Grade, Name, Exp;
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
            cardList.Add(new CardData(row[0], row[1], row[2], row[3], row[4]));
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

#region Don't Destory
    void Awake()
    {
        cardDictionary = GetComponent<CardsDictionary>();

        var cardDataManagers = FindObjectsOfType<CardDataManager>();
        if(cardDataManagers.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

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
        MyCardsList = JsonUtility.FromJson<Serialization<CardData>>(jdata).MyCards;
    }

    // 특정 카드를 가지고 시작하도록 만들려고. 아무것도 없이 시작할 수도 있다
    void ResetCards()
    {
        MyCardsList.Clear();
        List<CardData> startingCards = new ReadCardData().GetCardsList(startingCardData);
        // MyCardsList.AddRange(startingCards);
        string _weapon = startingCards[0].Type;
        string _grade = startingCards[0].Grade;
        string _name = startingCards[0].Name;
        // GameObject startingCard = cardDictionary.GenCard(_weapon, _grade, _name);
        // GameObject startingCard = cardDictionary.GenCard("Weapon", "Common", "뿌뿌");

        // 생성된 카드를 내 카드 리스트에 저장
        // Card newCard = startingCard.GetComponent<Card>();
        AddCardToMyCardsList(_weapon, _grade, _name);
        // Destroy(startingCard);
        Save();
        Load();
    }

    public List<CardData> GetMyCardList()
    {
        if(MyCardsList == null)Debug.Log("리스트 널");
        return MyCardsList;
    }

    public void RemoveCardFromMyCardList(Card cardToRemove)
    {
        string mID = cardToRemove.GetCardID();
        string mType = cardToRemove.GetCardType().ToString();
        string mGrade = cardToRemove.GetCardGrade().ToString();
        string mName = cardToRemove.GetCardName();

        foreach (var item in MyCardsList)
        {
            if(item.ID == mID && item.Type == mType && item.Grade == mGrade && item.Name == mName)
            {
                MyCardsList.Remove(item);
                return;
            }
            Save();
        }
    }

    public void AddCardToMyCardsList(string _type, string _grade, string _name)
    {
        string _id = MyCardsList.Count.ToString();

        CardData newCard = new CardData(_id, _type, _grade, _name, "1");
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
}
