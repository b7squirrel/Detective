using UnityEngine;

public class DisplayCurrency : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] TMPro.TextMeshProUGUI coinText;
    [SerializeField] TMPro.TextMeshProUGUI cristalText;
    [SerializeField] TMPro.TextMeshProUGUI LightningText;

    PlayerDataManager playerDataManager;

    void Start()
    {
        playerDataManager = FindObjectOfType<PlayerDataManager>();
    }

    void Update()
    {
        //coinText.text = dataContainer.coins.ToString();
        coinText.text = playerDataManager.GetCurrentCandyNumber().ToString();
        cristalText.text = playerDataManager.GetCurrentHighCoinNumber().ToString();
        LightningText.text = playerDataManager.GetCurrentLightningNumber().ToString() + "/ 60";

        //dateTimeText.text = System.DateTime.Now.ToString();
    }
}
