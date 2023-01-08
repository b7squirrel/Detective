using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickUpObject : MonoBehaviour, IPickUpObject
{
    [SerializeField] int coinAmount;
    public void OnPickUp(Character character)
    {
        character.coin.Add(coinAmount);
    }
}
