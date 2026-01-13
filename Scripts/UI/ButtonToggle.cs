using UnityEngine;
using UnityEngine.UI;

public class ButtonToggle : MonoBehaviour
{   
    [SerializeField] Image buttonIcon;
    [SerializeField] Sprite onSprite;
    [SerializeField] Sprite offSprite;
    [SerializeField] GameObject onObject;
    [SerializeField] GameObject offObject;

    public void SetImage(bool state)
    {
        if(state)
        {
            buttonIcon.sprite = onSprite;

            onObject.SetActive(true);
            offObject.SetActive(false);
        }
        else
        {
            buttonIcon.sprite = offSprite;
            onObject.SetActive(false);
            offObject.SetActive(true);
        }
    }
}