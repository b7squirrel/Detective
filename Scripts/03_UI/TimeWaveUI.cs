using TMPro;
using UnityEngine;

public class TimeWaveUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] TextMeshProUGUI stageText;

    public void InitTimeWaveUI(string _time, string _wave)
    {
        timeText.text = _time;
        waveText.text = _wave;
        stageText.gameObject.SetActive(false);
    }
    public void InitTimeUI(string _time)
    {
        timeText.text = _time;
    }
    public void InitStageUI(string _stage)
    {
        stageText.text = "스테이지 " + _stage;
        waveText.gameObject.SetActive(false);
    }
}
