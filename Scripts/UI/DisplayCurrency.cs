using UnityEngine;
using TMPro;

public class DisplayCurrency : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI coinText;
    [SerializeField] TextMeshProUGUI cristalText;
    [SerializeField] TextMeshProUGUI lightningText;

    PlayerDataManager playerDataManager;

    void OnEnable()
    {
        if (playerDataManager == null)
            playerDataManager = FindObjectOfType<PlayerDataManager>();

        playerDataManager.OnCurrencyChanged += UpdateUI;
        UpdateUI();
    }

    void OnDisable()
    {
        if (playerDataManager != null)
            playerDataManager.OnCurrencyChanged -= UpdateUI;
    }

    void Start()
    {
        UpdateUI();       
    }

    public void UpdateUI()
    {
        coinText.text = playerDataManager.GetCurrentCoinNumber().ToString();
        cristalText.text = playerDataManager.GetCurrentCristalNumber().ToString();
        lightningText.text = playerDataManager.GetCurrentLightningNumber().ToString() + "/ 60";
    }
}