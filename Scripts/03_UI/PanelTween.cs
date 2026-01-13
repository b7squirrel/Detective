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

        // Sequence로 여러 애니메이션 조합
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
        gameObject.SetActive(true);
        
        // 시작 스케일을 0으로 설정
        panelTrns.localScale = Vector3.zero;
        
        // OutBack 이징으로 튕기는 효과
        panelTrns.DOScale(Vector3.one, animationDuration)
            .SetEase(popupEase)
            .SetUpdate(true); // TimeScale 영향 안받게 (일시정지 시에도 작동)
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
