using UnityEngine;

public class DisplayCoinNumbers : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI coinNumbers;
    [SerializeField] Animator coinIconAnim;
    CoinManager coinManager;

    void Start()
    {
        coinManager = GetComponent<CoinManager>();
        coinManager.OnCoinAcquired += UpdateKillNumberDisp;
    }

    void UpdateKillNumberDisp()
    {
        coinNumbers.text = coinManager.GetCurrentCoins().ToString();
        coinIconAnim.SetTrigger("Pop");
    }
}