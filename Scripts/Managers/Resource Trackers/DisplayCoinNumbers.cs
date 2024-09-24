using UnityEngine;

public class DisplayCoinNumbers : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI coinNumbers;
    [SerializeField] Animator coinIconAnim;
    CoinManager coinManager;

    void Start()
    {
        coinManager = GetComponent<CoinManager>();
        coinManager.OnCoinAcquired += UpdateCoinNumberDisp;
    }

    /// <summary>
    /// 스테이지에서 습득한 코인만 표시
    /// </summary>
    void UpdateCoinNumberDisp()
    {
        coinNumbers.text = coinManager.GetCoinNumPickedup().ToString();
        if (IsPopAnimPlaying()) 
            return;
        coinIconAnim.SetTrigger("Pop");
    }
    bool IsPopAnimPlaying()
    {
        if (coinIconAnim.GetCurrentAnimatorStateInfo(0).IsName("Pop"))
        {
            return true;
        }
        return false;
    }
}