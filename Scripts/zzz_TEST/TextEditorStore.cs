using UnityEngine;
using TMPro;

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

        // HeroSingle.text = HeroSingleText;
        // HeroMulti.text = HeroMultiText;

        for (int i = 0; i < cristalNums.Length; i++)
        {
            cristalNums[i].text = cristalNumsText[i];
            cristalCost[i].text = cristalCostText[i];
        }
    }
}