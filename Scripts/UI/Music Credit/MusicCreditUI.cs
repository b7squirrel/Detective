using UnityEngine;
using UnityEngine.UI;

public class MusicCreditUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI musicTitle;
    [SerializeField] TMPro.TextMeshProUGUI musicCredit;
    [SerializeField] TMPro.TextMeshProUGUI musicText;
    [SerializeField] Image icon;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] RectTransform rectransform;

    [SerializeField] float fadeTime;

    Animator anim;

    public void CreditFadeIn(string _title, string _credit)
    {
        musicTitle.text = _title;
        musicCredit.text = _credit;
        anim = GetComponent<Animator>();
        anim.SetTrigger("Start");
    }
    public void CreditFadeOut()
    {
        anim.SetTrigger("End");
    }
}