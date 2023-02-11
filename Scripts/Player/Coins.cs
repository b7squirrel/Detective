using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coins : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] TMPro.TextMeshProUGUI coinsCountText;

    void Start()
    {
        Add(0);
    }

    public void Add(int coinAmount)
    {
        dataContainer.coins += coinAmount;
        coinsCountText.text = dataContainer.coins.ToString();
    }
}
