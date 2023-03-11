using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayCurrency : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] TMPro.TextMeshProUGUI coinText;
    [SerializeField] TMPro.TextMeshProUGUI goldText;
    [SerializeField] TMPro.TextMeshProUGUI LightningText;
    [SerializeField] TMPro.TextMeshProUGUI dateTimeText;

    void Update()
    {
        coinText.text = dataContainer.coins.ToString();
        goldText.text = dataContainer.gold.ToString();
        LightningText.text = dataContainer.lightning.ToString();
        dateTimeText.text = System.DateTime.Now.ToString();
    }
}
