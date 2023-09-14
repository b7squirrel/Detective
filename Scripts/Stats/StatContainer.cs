using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardStats
{
    public CardStats(int _cardHP, int _cardAtk)
    {
        cardHP = _cardHP;
        cardAtk = _cardAtk;
    }
    public int cardHP, cardAtk; 
    public void SumStats(CardStats _statsToAdd)
    {
        cardHP += _statsToAdd.cardHP;
        cardAtk += _statsToAdd.cardAtk;
    }
}
public class StatContainer : MonoBehaviour
{
    [SerializeField] CardStats cardStats;
}
