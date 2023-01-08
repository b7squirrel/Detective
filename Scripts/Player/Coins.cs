using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coins : MonoBehaviour
{
    public int coinAcquired;
    [SerializeField] TMPro.TextMeshProUGUI coinsCountText;

    void Start()
    {
        Add(0);
    }

    public void Add(int coinAmount)
    {
        coinAcquired += coinAmount;
        coinsCountText.text = "COINS  " + coinAcquired.ToString();
    }
}
