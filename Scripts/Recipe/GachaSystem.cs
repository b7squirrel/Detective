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
        if (gachaPools == null)
        {
            gachaPools = new();

            gachaPools = new ReadCardData().GetCardsList(gachaPoolDataBase);
        }

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
        for (int i = 0; i < drawnItems.Count; i++)
        {
            if(drawnItems[i].Type == CardType.Weapon.ToString())
            {
                cardDictionary.GetWeaponData(drawnItems[i]);
            }
            else
            {
                cardDictionary.GetItemData(drawnItems[i]);
            }
        }
    }
}
