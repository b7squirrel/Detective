using System.Collections;
using UnityEngine;

public class TextFeedback : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI textToInteract;
    [SerializeField] float desiredFontSizeFactor;
    float initFontSize;
    bool isInit;

    private void OnEnable()
    {
        if(isInit == false)
        {
            initFontSize = textToInteract.fontSize;
            isInit = true;
        }
    }
    public void UpText(float _rate)
    {
        if (textToInteract.gameObject.activeSelf == false) 
            return;
        StartCoroutine(UpTextCo(_rate));
    }
    IEnumerator UpTextCo(float _rate)
    {
        textToInteract.fontSize *= desiredFontSizeFactor;
        textToInteract.text = _rate.ToString() + "%";
        yield return new WaitForSecondsRealtime(.03f);
        textToInteract.fontSize = initFontSize;
    }
}
