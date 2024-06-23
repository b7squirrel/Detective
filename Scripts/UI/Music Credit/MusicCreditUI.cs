using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MusicCreditUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI credit;
    [SerializeField] Image icon;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] RectTransform rectransform;

    [SerializeField] float fadeTime;

    public void CreditFadeIn(string _credit)
    {
        credit.text = _credit;
        //canvasGroup.alpha = 0f;
        rectransform.transform.localPosition = new Vector3(0f, -1000f, 0f);
        rectransform.DOAnchorPos(new Vector2(0f, 210f), fadeTime, false).SetEase(Ease.OutQuad)
            .SetDelay(1f)
            .OnComplete(() => CreditFadeOut()); // 동작이 끝나면
        //canvasGroup.DOFade(1, fadeTime);
    }
    
    void CreditFadeOut()
    {
        //canvasGroup.alpha = 1f;
        //rectransform.transform.localPosition = new Vector3(0f, 210f, 0f);
        rectransform.DOAnchorPos(new Vector2(0f, -200f), fadeTime, false).SetEase(Ease.InOutQuint)
            .SetDelay(3f);
        //canvasGroup.DOFade(0, fadeTime);
    }
}