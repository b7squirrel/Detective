using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayCurrency : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] TMPro.TextMeshProUGUI coinText;
    [SerializeField] TMPro.TextMeshProUGUI goldText;

    void Update()
    {
        coinText.text = dataContainer.coins.ToString();
        goldText.text = dataContainer.gold.ToString();
    }
}
