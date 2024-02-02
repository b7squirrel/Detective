using UnityEngine;

public class VictoryPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI killText;
    [SerializeField] TMPro.TextMeshProUGUI coinText;

    public void InitAwards(int killNum, int coinNum)
    {
        killText.text = killNum.ToString();
        coinText.text = coinNum.ToString();
    }
}