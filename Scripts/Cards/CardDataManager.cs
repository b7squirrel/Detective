using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class Serialization<T>
{
    public Serialization(List<T> _target) => target = _target;
    public List<T> target;
}

[System.Serializable]
public class CardData
{
    public CardData(string _Type, string _Grade, string _Name)
    {
        Type = _Type;
        Grade = _Grade;
        Name = _Name;
    }
    public string Type, Grade, Name;
}

public class CardDataManager : MonoBehaviour
{
    public TextAsset CardDatabase;
    public List<CardData> AllCardsList, MyCardsList;
    string filePath;
    string myCards = "MyCards.txt";

#region Don't Destory
    void Awake()
    {
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
        string[] line = CardDatabase.text.Substring(0, CardDatabase.text.Length - 1).Split('\n');
        for (int i = 0; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');
            AllCardsList.Add(new CardData(row[0], row[1], row[2]));
        }

#if UNITY_EDITOR
        if (Directory.Exists(Application.dataPath + "/PlayerData") == false)
            Directory.CreateDirectory(Application.dataPath + "/PlayerData");

        filePath = Application.dataPath + "/PlayerData" + myCards;
#endif

#if UNITY_ANDROID
        if (Directory.Exists(Application.persistentDataPath + "/PlayerData") == false)
            Directory.CreateDirectory(Application.persistentDataPath + "/PlayerData");

        filePath = Application.persistentDataPath + "/PlayerData/" + myCards;
#endif

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
        MyCardsList = JsonUtility.FromJson<Serialization<CardData>>(jdata).target;
    }

    // 특정 카드를 가지고 시작하도록 만들려고. 아무것도 없이 시작할 수도 있다
    void ResetCards()
    {
        List<CardData> basicCard = AllCardsList;
        MyCardsList.Clear();
        MyCardsList.AddRange(basicCard);
        Save();
        Load();
    }

    public List<CardData> GetMyCardList()
    {
        return MyCardsList;
    }
}
