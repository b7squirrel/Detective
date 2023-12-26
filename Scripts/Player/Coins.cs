using UnityEngine;

public class Coins : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] TMPro.TextMeshProUGUI candyCountText;
    PlayerDataManager playerDataManager;
    int currentCandyNum;

    void Start()
    {
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        currentCandyNum = playerDataManager.GetCurrentCandyNumber();
        Add(0); // UI �ʱ�ȭ
    }

    public void Add(int candyAmount)
    {
        //dataContainer.coins += coinAmount;
        currentCandyNum += candyAmount;
        playerDataManager.SetCurrentCandyNumber(currentCandyNum);
        candyCountText.text = currentCandyNum.ToString();
    }
}
