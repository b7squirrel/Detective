using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [SerializeField] bool showTimeUI;
    TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        if (showTimeUI == false)
        {
            text.color = new Color(0, 0, 0, 0);
        }
    }

    public void UpdateTime(float time)
    {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);

        text.text = minutes.ToString() + ":" + seconds.ToString("00");
    }
}
