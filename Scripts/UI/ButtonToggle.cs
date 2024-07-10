using UnityEngine;
using UnityEngine.UI;

public class ButtonToggle : MonoBehaviour
{
    [SerializeField] Sprite soundOnSprite;
    [SerializeField] Sprite soundOffSprite;
    [SerializeField] Image buttonImage;
    [SerializeField] TMPro.TextMeshProUGUI text;

    public void SetImage(bool soundState)
    {
        buttonImage.sprite = soundState ? soundOnSprite : soundOffSprite;
        text.text = soundState ? "Sound On" : "Sound Off";
    }
}