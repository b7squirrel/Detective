using TMPro;
using UnityEngine;

public class TimeWaveUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] TextMeshProUGUI stageText;
    EnemyCountUI enemyCounterUI;

    public void InitTimeWaveUI(string _time, string _wave)
    {
        timeText.text = _time;
        waveText.text = $"웨이브 {_wave}";
        stageText.gameObject.SetActive(false);
        if(enemyCounterUI == null) enemyCounterUI = GetComponent<EnemyCountUI>();
        enemyCounterUI.InitProgressText(true);
    }
    public void InitTimeUI(string _time)
    {
        timeText.text = _time;
    }
    public void InitStageUI(string _stage)
    {
        stageText.text = "스테이지 " + _stage;
        waveText.gameObject.SetActive(false);
        if(enemyCounterUI == null) enemyCounterUI = GetComponent<EnemyCountUI>();
        enemyCounterUI.InitProgressText(false);
    }
}
