using TMPro;
using UnityEngine;

public class TimeWaveUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI waveText;

    public void InitTimeWaveUI(string _time, string _wave)
    {
        timeText.text = _time;
        waveText.text = "웨이브 " + _wave;
    }
    public void InitTimeUI(string _time)
    {
        timeText.text = _time;
    }
    public void InitStageUI(string _stage)
    {
        waveText.text = "스테이지 " + _stage;
    }
}
