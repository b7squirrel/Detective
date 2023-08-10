using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Draw()
    {
        if(gachaPools == null) gachaPools = new List<CardData>();

        gachaPools = new ReadCardData().GetCardsList(gachaPoolDataBase);

        

        for (int i = 0; i < 3; i++)
        {
            int pickIndex = UnityEngine.Random.Range(0, gachaPools.Count);
            string mType = gachaPools[pickIndex].Type;
            string mGrade = gachaPools[pickIndex].Grade;
            string mName = gachaPools[pickIndex].Name;
            GameObject newCard = cardDictionary.GenCard(mType, mGrade, mName);
            newCard.transform.SetParent(transform);
            newCard.transform.position = transform.position;
            newCard.transform.localScale = Vector3.one;

            cardDataManager.AddCardToMyCardsList(newCard.GetComponent<Card>());
            Destroy(newCard);
        }
    }
    public void GenCards(List<CardData> drawnItems)
    {
        foreach (var item in drawnItems)
        {
            if(item.Type == CardType.Weapon.ToString())
            {
                cardDictionary.GetWeaponData(item.Grade, item.Name);
            }
            else
            {
                cardDictionary.GetItemData(item.Grade, item.Name);
            }
        }
    }
}
