using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI killText;
    [SerializeField] TMPro.TextMeshProUGUI coinText;
    [SerializeField] TMPro.TextMeshProUGUI stageNumber;
    [SerializeField] GameObject rayBGEffect;
    [SerializeField] bool isDarkBG;

    public void InitAwards(int _killNum, int _coinNum, int _stageNum)
    {
        killText.text = _killNum.ToString();
        coinText.text = _coinNum.ToString();
        stageNumber.text = _stageNum.ToString();

        if (isDarkBG)
        {
            GameManager.instance.darkBG.SetActive(true);
            GameManager.instance.lightBG.SetActive(false);
            rayBGEffect.SetActive(false);
        }
        else
        {
            GameManager.instance.lightBG.SetActive(true);
            GameManager.instance.darkBG.SetActive(false);
            rayBGEffect.SetActive(true);
        }
        GameManager.instance.ActivateConfirmationButton(1.31f);
    }
}