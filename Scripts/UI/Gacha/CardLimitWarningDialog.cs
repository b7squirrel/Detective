using System.Collections;
using UnityEngine;

/// <summary>
/// isManuallyClose 플래그로 수동/자동 닫기 구분
/// CloseWarning() 메서드로 UI 닫기 로직 통합
/// 코루틴 완료 시 co = null 설정으로 참조 정리
/// 수동으로 닫힌 경우 자동 닫기 방지
/// </summary>
public class CardLimitWarningDialog : MonoBehaviour
{
    [SerializeField] GameObject warningPanel;
    [SerializeField] float warningDuration;
    [SerializeField] TMPro.TextMeshProUGUI warningText;
    [SerializeField] GameObject BG;

    Coroutine co;
    bool isManuallyClose = false; // 수동으로 닫혔는지 플래그

    public void SetWarningText(string cardType, int cardCount)
    {
        string warning = $"{cardType} 카드가 100장을 넘겼어요! ({cardCount}/100)\n\n" +
                        "카드를 판매하거나 합성해서\n" +
                        "공간을 정리해 주세요.";
        warningText.text = warning;

        // 기존 코루틴 정리
        if (co != null)
        {
            StopCoroutine(co);
            co = null;
        }

        isManuallyClose = false; // 플래그 리셋
        co = StartCoroutine(WarningCo());
    }

    public void StopCardLimitCoroutine()
    {
        if (co != null)
        {
            StopCoroutine(co);
            co = null;
        }

        isManuallyClose = true; // 수동으로 닫혔음을 표시
        CloseWarning();
    }

    void CloseWarning()
    {
        warningPanel.SetActive(false);
        BG.SetActive(false);
    }

    IEnumerator WarningCo()
    {
        // 패널 열기
        warningPanel.SetActive(true);
        BG.SetActive(true);

        yield return new WaitForSecondsRealtime(warningDuration);

        // 수동으로 닫히지 않았을 때만 자동으로 닫기
        if (!isManuallyClose)
        {
            CloseWarning();
        }

        co = null; // 코루틴 완료 후 참조 정리
    }
}