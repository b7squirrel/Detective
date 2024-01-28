using UnityEngine;

public class Coins : MonoBehaviour
{
    [SerializeField] DataContainer dataContainer;
    [SerializeField] TMPro.TextMeshProUGUI candyCountText;
    [SerializeField] Animator candyIconAnim;
    PlayerDataManager playerDataManager;
    int currentCandyNum;

    void Start()
    {
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        currentCandyNum = playerDataManager.GetCurrentCandyNumber();
        Add(0); // UI √ ±‚»≠
    }

    public void Add(int candyAmount)
    {
        //dataContainer.coins += coinAmount;
        currentCandyNum += candyAmount;
        playerDataManager.SetCurrentCandyNumber(currentCandyNum);
        candyCountText.text = currentCandyNum.ToString();
        candyIconAnim.SetTrigger("Pop");
    }
}
