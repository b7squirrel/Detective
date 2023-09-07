using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GachaSystem : MonoBehaviour
{
    CardsDictionary cardDictionary;
    CardDataManager cardDataManager;

    [SerializeField] TextAsset gachaPoolDataBase;
    List<CardData> gachaPools;

    void Start()
    {
        cardDictionary = FindObjectOfType<CardsDictionary>();
        cardDataManager = FindObjectOfType<CardDataManager>();
    }

    // shop 패널의 뽑기 버튼에서 호출
    public void Draw()
    {
        if(gachaPools == null) gachaPools = new List<CardData>();

        gachaPools = new ReadCardData().GetCardsList(gachaPoolDataBase);

        // 임시로 3개씩 카드가 뽑힘
        for (int i = 0; i < 3; i++)
        {
            int pickIndex = UnityEngine.Random.Range(0, gachaPools.Count);
            string mType = gachaPools[pickIndex].Type;
            string mGrade = gachaPools[pickIndex].Grade;
            string mName = gachaPools[pickIndex].Name;
            
            cardDataManager.AddNewCardToMyCardsList(gachaPools[pickIndex]);
        }
    }
    
    public void GenCards(List<CardData> drawnItems)
    {
        foreach (var item in drawnItems)
        {
            if(item.Type == CardType.Weapon.ToString())
            {
                cardDictionary.GetWeaponData(item);
            }
            else
            {
                cardDictionary.GetItemData(item);
            }
        }
    }
}
