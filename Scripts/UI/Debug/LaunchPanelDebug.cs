using System.Collections.Generic;
using UnityEngine;

public class LaunchPanelDebug : MonoBehaviour
{
    [SerializeField] CardDataManager cardDataManager;
    [SerializeField] TMPro.TextMeshProUGUI oriNum;
    [SerializeField] TMPro.TextMeshProUGUI itemNum;
    

    void Update()
    {
        List<CardData> mCardData = new();
        mCardData.AddRange(cardDataManager.GetMyCardList());

        int ori = 0;
        int itemN = 0;

        foreach (var item in mCardData)
        {
            if(item.Type == "Weapon") ori++;
            if(item.Type == "Item") itemN++;
        }

        oriNum.text = ori.ToString();
        itemNum.text = itemN.ToString();
    }
}