using DG.Tweening;
using TMPro;
using UnityEngine;

public class TimeWaveUI : MonoBehaviour
{
    [SerializeField] ShadowedText timeText;
    [SerializeField] ShadowedText waveNumText;
    [SerializeField] ShadowedText stageNumText;
    [SerializeField] GameObject waveText;
    [SerializeField] GameObject stageText;
    [SerializeField] AudioClip wavePopupSound;
    [SerializeField] AudioClip waveStartSound;
EnemyCountUI enemyCounterUI;

public void InitTimeWaveUI(string _time, string _wave)
    {
timeText.text = _time;
waveNumText.text = $"{_wave}";
waveNumText.gameObject.SetActive(true);
waveText.gameObject.SetActive(true);
stageNumText.gameObject.SetActive(false);
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
stageNumText.text = _stage;
stageNumText.gameObject.SetActive(true);
stageText.gameObject.SetActive(true);
waveNumText.gameObject.SetActive(false);
waveText.gameObject.SetActive(false);
if (enemyCounterUI == null) enemyCounterUI = GetComponent<EnemyCountUI>();
enemyCounterUI.InitProgressText(false);
    }

public void PunchWaveText()
    {
waveNumText.transform.DOKill();
DOTween.Kill(waveNumText.Main); // ★ ShadowedText가 아니라 실제 TMP 컴포넌트를 대상으로 킬
waveNumText.transform.localScale = Vector3.one;

// 1. 스케일 펀치
waveNumText.transform.DOPunchScale(Vector3.one * 4f, 0.4f, 1, 0f);

// 2. 색상 반짝임: 노랑 → 흰색 → 노랑 → 흰색 → 원래색 (여러 번 깜빡)
// ★ 메인 텍스트에만 적용 (그림자는 검정색 유지)
Sequence colorSeq = DOTween.Sequence();
colorSeq.Append(waveNumText.Main.DOColor(Color.yellow, 0.07f));
colorSeq.Append(waveNumText.Main.DOColor(Color.white, 0.07f));
colorSeq.Append(waveNumText.Main.DOColor(Color.yellow, 0.07f));
colorSeq.Append(waveNumText.Main.DOColor(Color.white, 0.07f));
colorSeq.Append(waveNumText.Main.DOColor(new Color(1f, 0.8f, 0f), 0.08f)); // 황금색
colorSeq.Append(waveNumText.Main.DOColor(Color.white, 0.15f));

// 3. 사운드
if (wavePopupSound != null)
SoundManager.instance.Play(wavePopupSound);
if (waveStartSound != null)
SoundManager.instance.Play(waveStartSound);
    }
}