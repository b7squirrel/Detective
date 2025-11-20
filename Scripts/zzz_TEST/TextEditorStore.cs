using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TextEditorStore : MonoBehaviour
{
    [Header("입력")]
    [SerializeField] string starterSingleText;
    [SerializeField] string startermultiText;
    [SerializeField] string oriSngleText;
    [SerializeField] string oriMultiText;
    [SerializeField] string equipmentSingleText;
    [SerializeField] string equipmentMultiText;
    [SerializeField] string HeroSingleText;
    [SerializeField] string HeroMultiText;
    [SerializeField] string[] coinNumsText;
    [SerializeField] string[] coinCostText;
    [SerializeField] string[] cristalNumsText;
    [SerializeField] string[] cristalCostText;

    [Header("스타터")]
    [SerializeField] TextMeshProUGUI starterSingle;
    [SerializeField] TextMeshProUGUI starterMulti;

    [Header("오리")]
    [SerializeField] TextMeshProUGUI oriSngle;
    [SerializeField] TextMeshProUGUI oriMulti;

    [Header("장비")]
    [SerializeField] TextMeshProUGUI equipmentSingle;
    [SerializeField] TextMeshProUGUI equipmentMulti;

    [Header("전문가")]
    [SerializeField] TextMeshProUGUI HeroSingle;
    [SerializeField] TextMeshProUGUI HeroMulti;

    [Header("코인")]
    [SerializeField] TextMeshProUGUI[] coinNums;
    [SerializeField] TextMeshProUGUI[] coinCost;

    [Header("보석")]
    [SerializeField] TextMeshProUGUI[] cristalNums;
    [SerializeField] TextMeshProUGUI[] cristalCost;

    void Start()
    {
        starterSingle.text = starterSingleText;
        starterMulti.text = startermultiText;

        oriSngle.text = oriSngleText;
        oriMulti.text = oriMultiText;

        equipmentSingle.text = equipmentSingleText;
        equipmentMulti.text = equipmentMultiText;

        HeroSingle.text = HeroSingleText;
        HeroMulti.text = HeroMultiText;

        for (int i = 0; i < coinNums.Length; i++)
        {
            coinNums[i].text = coinNumsText[i];
            coinCost[i].text = coinCostText[i];
        }
        
        for (int i = 0; i < cristalNums.Length; i++)
        {
            cristalNums[i].text = cristalNumsText[i];
            cristalCost[i].text = "$" + cristalCostText[i];
        }
    }

    public int GetCristalNums(int index)
    {
        return int.Parse(cristalNums[index].text);
    }
    public float GetCristalCost(int index)
    {
        return float.Parse(cristalCost[index].text);
    }

    public int GetCoinNum(int index)
    {
        return int.Parse(coinNums[index].text);
    }
}