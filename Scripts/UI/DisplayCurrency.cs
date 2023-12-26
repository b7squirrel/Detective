using UnityEngine;

public class DisplayCurrency : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] TMPro.TextMeshProUGUI coinText;
    [SerializeField] TMPro.TextMeshProUGUI goldText;
    [SerializeField] TMPro.TextMeshProUGUI LightningText;
    [SerializeField] TMPro.TextMeshProUGUI dateTimeText;

    PlayerDataManager playerDataManager;

    void Start()
    {
        playerDataManager = FindObjectOfType<PlayerDataManager>();
    }

    void Update()
    {
        //coinText.text = dataContainer.coins.ToString();
        coinText.text = playerDataManager.GetCurrentCandyNumber().ToString();
        goldText.text = dataContainer.gold.ToString();
        LightningText.text = dataContainer.lightning.ToString();
        dateTimeText.text = System.DateTime.Now.ToString();
    }
}
