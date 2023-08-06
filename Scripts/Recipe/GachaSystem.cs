using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    CardsDictionary cardDictionary;

    void Start()
    {
        cardDictionary = FindObjectOfType<CardsDictionary>();
    }

    public void Draw(List<CardData> cardPool)
    {

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
