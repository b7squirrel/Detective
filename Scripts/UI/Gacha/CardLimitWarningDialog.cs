using System.Collections;
using UnityEngine;

public class CardLimitWarningDialog : MonoBehaviour
{
    [SerializeField] GameObject warningPanel;
    [SerializeField] float warningDuration; // 경고 지속시간
    [SerializeField] TMPro.TextMeshProUGUI warningText;
    [SerializeField] GameObject BG;
    Coroutine co;

    public void SetWarningText(string cardType, int cardCount)
    {
        string warning = $"{cardType} 카드가 100장을 넘겼어요! ({cardCount}/100)\n\n" +
                        "카드를 판매하거나 합성해서\n" +
                        "공간을 정리해 주세요.";

        warningText.text = warning;

        if (co != null) StopCoroutine(co);
        StartCoroutine(WarningCo());
    }
    IEnumerator WarningCo()
    {
        warningPanel.SetActive(true);
        BG.SetActive(true);
        yield return new WaitForSecondsRealtime(warningDuration);
        warningPanel.SetActive(false);
        BG.SetActive(false);
    }
}
