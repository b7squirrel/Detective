using UnityEngine;
using TMPro;

public class DisplayCurrency : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI coinText;
    [SerializeField] TextMeshProUGUI cristalText;
    [SerializeField] TextMeshProUGUI lightningText;

    PlayerDataManager playerDataManager;

    void Start()
    {
        playerDataManager = FindObjectOfType<PlayerDataManager>();

        // 이벤트 구독
        playerDataManager.OnCurrencyChanged += UpdateUI;

        // 처음 UI 세팅
        UpdateUI();
    }

    void UpdateUI()
    {
        coinText.text = playerDataManager.GetCurrentCandyNumber().ToString();
        cristalText.text = playerDataManager.GetCurrentHighCoinNumber().ToString();
        lightningText.text = playerDataManager.GetCurrentLightningNumber().ToString() + "/ 60";
    }
}