using UnityEngine;

public class VictoryPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI killText;
    [SerializeField] TMPro.TextMeshProUGUI coinText;
    [SerializeField] TMPro.TextMeshProUGUI stageNumber;

    public void InitAwards(int _killNum, int _coinNum, int _stageNum)
    {
        killText.text = _killNum.ToString();
        coinText.text = _coinNum.ToString();
        stageNumber.text = _stageNum.ToString();
    }
}