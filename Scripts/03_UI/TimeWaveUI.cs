using DG.Tweening;
using TMPro;
using UnityEngine;

public class TimeWaveUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] TextMeshProUGUI stageText;
    [SerializeField] AudioClip wavePopupSound;
    [SerializeField] AudioClip waveStartSound;
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
        if (enemyCounterUI == null) enemyCounterUI = GetComponent<EnemyCountUI>();
        enemyCounterUI.InitProgressText(false);
    }

    public void PunchWaveText()
    {
        waveText.transform.DOKill();
        DOTween.Kill(waveText);
        waveText.transform.localScale = Vector3.one;

        // 1. 스케일 펀치
        waveText.transform.DOPunchScale(Vector3.one * 4f, 0.4f, 1, 0f);

        // 2. 색상 변화: 노란색으로 번쩍였다가 원래 색으로
        waveText.DOColor(Color.yellow, 0.1f)
            .OnComplete(() => waveText.DOColor(Color.white, 0.15f));

        // 3. 사운드
        if (wavePopupSound != null)
            SoundManager.instance.Play(wavePopupSound);
        if (waveStartSound != null)
            SoundManager.instance.Play(waveStartSound);
    }
}
