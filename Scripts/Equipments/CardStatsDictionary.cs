using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CardStats
{
    public CardStats(string _name, string _grade, string _armor, 
            string _magnetSize, string _moveSpeed, string _maxHP, string _damageBonus,
            string _projAmount, string _projSpeed, string _knockbackChance,
            string _criticalChance, string _coolTIme, string _hpRegenRate)
            {
                Name = _name;
                Grade = _grade;
                Armor = _armor;
                MagnetSize = _magnetSize;
                MoveSpeed = _moveSpeed;
                MaxHP = _maxHP;
                DamageBonus = _damageBonus;
                ProjAmount = _projAmount;
                ProjSpeed = _projSpeed;
                KnockbackChance = _knockbackChance;
                CriticalChance = _criticalChance;
                CoolTime = _coolTIme;
                HpRegenRate = _hpRegenRate;
            }

    public string Name, Grade, Armor, MagnetSize, MoveSpeed, MaxHP, 
                    DamageBonus, ProjAmount, ProjSpeed, KnockbackChance,
                    CriticalChance, CoolTime, HpRegenRate;
}

[System.Serializable]
public class ReadStatsData
{
    public List<CardStats> cardStatsList;
    public List<CardStats> GetCardStatsList(TextAsset statsDataText)
    {
        string[] line = statsDataText.text.Substring(0, statsDataText.text.Length - 1).Split('\n');
        for (int i = 0; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');
            cardStatsList.Add(new CardStats(row[0], row[1], row[2], row[3], row[4], 
                                row[5], row[6], row[7], row[8], row[9], row[10], row[11], row[12]));
        }
        return cardStatsList;
    }
}

public class CardStatsDictionary : MonoBehaviour
{
    public TextAsset oriStatData;
    public List<CardStats> AllCardStats;
    string filePath;
    string allStats = "AllStats.txt";

    void Start()
    {
        // 전체 카드 스탯 불러오기
        AllCardStats = new ReadStatsData(AllCardStats);

        if (Directory.Exists(Application.persistentDataPath + "/StatsData") == false)
            Directory.CreateDirectory(Application.persistentDataPath + "/StatsData");

        filePath = Application.persistentDataPath + "/StatsData/" + allStats;

        Load();
    }

    void Load()
    {
        if (!File.Exists(filePath))
        {
            ResetCards();
            return;
        }
        string jdata = File.ReadAllText(filePath);
        AllCardStats = JsonUtility.FromJson<Serialization<CardData>>(jdata).Data;
    }

    void ResetCardStats()
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
        AddCardToMyCardsList(GenNewCardData(_weapon, _grade, _name));
        // Destroy(startingCard);
        Save();
        Load();
    }
}
