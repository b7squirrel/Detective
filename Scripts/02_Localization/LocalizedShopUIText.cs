using UnityEngine;
using TMPro;


public class LocalizedShopUIText : MonoBehaviour
{
    [Header("Shop")]
    [SerializeField] TextMeshProUGUI shop;
    [SerializeField] TextMeshProUGUI beginnerPack;
    [SerializeField] TextMeshProUGUI expertPack;
    [SerializeField] TextMeshProUGUI luckyBox;
    [SerializeField] TextMeshProUGUI duckCard;
    [SerializeField] TextMeshProUGUI itemCard;
    [SerializeField] TextMeshProUGUI[] singleDraw;
    [SerializeField] TextMeshProUGUI[] tenxDraw;
    [SerializeField] TextMeshProUGUI watchAdToDraw;

    void Awake()
    {
             LocalizationManager.OnLanguageChanged += UpdateText;
    }
    void Start()
    {
        UpdateText();
    }
    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }
    void UpdateText()
    {
        shop.text = LocalizationManager.Game.Shop;
        beginnerPack.text = LocalizationManager.Game.beginnerPack;
        expertPack.text = LocalizationManager.Game.ExpertPack;
        luckyBox.text = LocalizationManager.Game.luckyBox;
        duckCard.text = LocalizationManager.Game.duckCard;
        itemCard.text = LocalizationManager.Game.itemCard;
        foreach (var item in singleDraw)
        {
            item.text = LocalizationManager.Game.singleDraw;
        }
        foreach (var item in tenxDraw)
        {
            item.text = LocalizationManager.Game.tenXDraw;
        }
        watchAdToDraw.text = LocalizationManager.Game.watchAdToDraw;
    }
}
