using System.Collections;
using DG.Tweening;
using UnityEngine;
public class PanelTween : MonoBehaviour
{
     [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease popupEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;
    [SerializeField] Transform panelTrns;
    
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    public void ShowWithBounce()
    {
        gameObject.SetActive(true);
        panelTrns.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(panelTrns.DOScale(Vector3.one * 1.2f, animationDuration * 0.6f)
            .SetEase(Ease.OutCubic))
            .Append(panelTrns.DOScale(Vector3.one, animationDuration * 0.4f)
            .SetEase(Ease.InOutCubic))
            .SetUpdate(true);
    }
    /// <summary>
    /// 스케일 0에서 팍! 하고 나타나는 애니메이션
    /// </summary>
    public void ShowWithScale()
    {
        panelTrns.DOKill();
        gameObject.SetActive(true);
        
        panelTrns.localScale = Vector3.zero;
        
        panelTrns.DOScale(Vector3.one, animationDuration)
            .SetEase(popupEase)
            .SetUpdate(true);
    }
    public void ShowWithScaleDelayed(float delayedTime)
    {
        gameObject.SetActive(true);        // 먼저 켜서 코루틴을 이 오브젝트 위에서 돌릴 수 있게 함
        panelTrns.localScale = Vector3.zero; // 즉시 투명(스케일 0) 상태로 만들어 시각적으로는 안 보이게
        StartCoroutine(ShowWithScaleDelayedCo(delayedTime));
    }

    IEnumerator ShowWithScaleDelayedCo(float delayedTime)
    {
        yield return new WaitForSeconds(delayedTime);
        ShowWithScale(); // 내부에서 다시 SetActive(true) + DOScale 처리 (SetActive는 중복 호출이라 무해함)
    }
    public void HidePanel()
    {
        HideWithScale();
    }
    /// <summary>
    /// 스케일 0으로 사라지는 애니메이션
    /// </summary>
    public void HideWithScale(System.Action onComplete = null)
    {
        panelTrns.DOScale(Vector3.zero, animationDuration)
            .SetEase(closeEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }
}