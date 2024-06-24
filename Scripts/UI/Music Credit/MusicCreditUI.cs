using UnityEngine;
using UnityEngine.UI;

public class MusicCreditUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI credit;
    [SerializeField] TMPro.TextMeshProUGUI musicText;
    [SerializeField] Image icon;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] RectTransform rectransform;

    [SerializeField] float fadeTime;

    Animator anim;

    public void CreditFadeIn(string _credit)
    {
        credit.text = _credit;
        anim = GetComponent<Animator>();
        anim.SetTrigger("Start");
    }
    public void CreditFadeOut()
    {
        anim.SetTrigger("End");
    }
}