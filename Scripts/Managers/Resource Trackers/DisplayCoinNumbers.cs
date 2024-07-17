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
    /// ������������ ������ ���θ� ǥ��
    /// </summary>
    void UpdateCoinNumberDisp()
    {
        coinNumbers.text = coinManager.GetCoinNumPickedup().ToString();
        coinIconAnim.SetTrigger("Pop");
    }
}