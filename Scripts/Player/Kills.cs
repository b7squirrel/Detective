using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kills : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] TMPro.TextMeshProUGUI killsCountText;

    void Start()
    {
        Add(0);
    }

    public void Add(int killAmount)
    {
        dataContainer.coins += killAmount;
        killsCountText.text = dataContainer.coins.ToString();
    }
}
