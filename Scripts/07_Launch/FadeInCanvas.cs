using UnityEngine;
using DG.Tweening;

public class FadeInCanvas : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;

    public void FadeIn(float endValue, float duration)
    {
        canvasGroup.DOFade(endValue, duration);
    }
}