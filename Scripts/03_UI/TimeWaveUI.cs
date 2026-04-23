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

        // 2. 색상 반짝임: 노랑 → 흰색 → 노랑 → 흰색 → 원래색 (여러 번 깜빡)
        Sequence colorSeq = DOTween.Sequence();
        colorSeq.Append(waveText.DOColor(Color.yellow, 0.07f));
        colorSeq.Append(waveText.DOColor(Color.white, 0.07f));
        colorSeq.Append(waveText.DOColor(Color.yellow, 0.07f));
        colorSeq.Append(waveText.DOColor(Color.white, 0.07f));
        colorSeq.Append(waveText.DOColor(new Color(1f, 0.8f, 0f), 0.08f)); // 황금색
        colorSeq.Append(waveText.DOColor(Color.white, 0.15f));

        // 3. 사운드
        if (wavePopupSound != null)
            SoundManager.instance.Play(wavePopupSound);
        if (waveStartSound != null)
            SoundManager.instance.Play(waveStartSound);
    }
}
