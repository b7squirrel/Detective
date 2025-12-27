using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ResultPanel : MonoBehaviour
{
    [Header("공통 요소")]
    [SerializeField] RectTransform panelRec;
    [SerializeField] RectTransform titleRec;
    [SerializeField] RectTransform killRec;
    [SerializeField] RectTransform coinRec;
    [SerializeField] RectTransform oriRec;
    [SerializeField] RectTransform stampRec;
    [SerializeField] TMPro.TextMeshProUGUI titleText;
    [SerializeField] TMPro.TextMeshProUGUI killText;
    [SerializeField] TMPro.TextMeshProUGUI coinText;
    [SerializeField] TMPro.TextMeshProUGUI stampText;

    [Header("일반 모드 요소")]
    [SerializeField] RectTransform stageNumRec;
    [SerializeField] TMPro.TextMeshProUGUI stageNumberText;

    [Header("무한 모드 요소")]
    [SerializeField] RectTransform survivalTimeRec;
    [SerializeField] RectTransform bestRecordRec;
    [SerializeField] TMPro.TextMeshProUGUI survivalTimeText;
    [SerializeField] TMPro.TextMeshProUGUI survivalTimeTitleText;
    [SerializeField] TMPro.TextMeshProUGUI bestRecordText;
    [SerializeField] TMPro.TextMeshProUGUI bestRecordTitleText;

    [SerializeField] bool isDarkBG;
    [SerializeField] CardDisp cardDisp; // 플레이어를 보여줄 cardDisp
    [SerializeField] Animator charAnim; // 오리의 애니메이터

    [Header("오리폭죽")]
    [SerializeField] ImageBouncerManager bouncerManager; // 오리 폭죽
    [SerializeField] int confettiNums; // 오리 수

    [Header("사운드")]
    [SerializeField] AudioClip resultSoundSuccess;
    [SerializeField] AudioClip resultSoundFail;
    [SerializeField] AudioClip panelSound;
    [SerializeField] AudioClip failTitleSound;
    [SerializeField] AudioClip clearTitleSound;
    [SerializeField] AudioClip TitleSound;
    [SerializeField] AudioClip ContentsSound;
    [SerializeField] AudioClip scribbleSound;
    [SerializeField] AudioClip popupSound;
    [SerializeField] AudioClip stampSound;
    
    public void InitAwards(int killNum, int coinNum, int stageNum, bool isWinningStage)
    {
        SetBG();
        PlayRegularAwardsSequence(killNum, coinNum, stageNum, isWinningStage);
    }
    public void InitInfiniteAwards(int killNum, int coinNum, int currentWave, string survivalTime, string bestRecord, bool isBestRecord)
    {
        SetBG();
        PlayInfiniteAwardsSequence(killNum, coinNum, currentWave, survivalTime, bestRecord);
    }

    void SetBG()
    {
        if (isDarkBG)
        {
            GameManager.instance.darkBG.SetActive(true);
            GameManager.instance.lightBG.SetActive(false);
        }
        else
        {
            GameManager.instance.lightBG.SetActive(true);
            GameManager.instance.darkBG.SetActive(false);
        }
    }
    IEnumerator InitAwardsCo(int killNum, int coinNum, int stageNum, bool isWinningStage)
    {
        if (isWinningStage) bouncerManager.Jump(confettiNums); // 150마리 폭죽


        yield return new WaitForSecondsRealtime(.5f);

        SetEquipSpriteRow();

        AudioClip resultSound = resultSoundSuccess;
        if (isWinningStage == false) 
        {
            charAnim.SetTrigger("Hit"); // 패배 화면이라면 오리도 패배 모션으로
            resultSound = resultSoundFail;
        }
        SoundManager.instance.Play(resultSound);

        killText.text = killNum.ToString();
        coinText.text = coinNum.ToString();
        stageNumberText.text = stageNum.ToString();

        
        GameManager.instance.ActivateConfirmationButton(2.7f);
    }
    #region 레귤러 애니메이션
    void PlayRegularAwardsSequence(int killNum, int coinNum, int stageNum, bool isWinningStage)
    {
        ResetRecs();
        string title = isWinningStage? "축하해요!" : "이런...";
        titleText.text = title;
        killText.text = killNum.ToString();
        coinText.text = coinNum.ToString();
        stageNumberText.text = stageNum.ToString();

        Sequence seq = DOTween.Sequence();
        // UI는 타임스케일 무시
        seq.SetUpdate(true);

        // panel
        seq.Append(panelRec.DOScale(.8f, .2f).SetEase(Ease.OutBack));

        // title
        // title
        seq.AppendInterval(.05f);
        seq.Append(titleRec.DOScale(1f, .18f).SetEase(Ease.OutBack, 1.7f));
        seq.AppendCallback(() => PlayUISound(popupSound));

        // ori
        charAnim.SetTrigger("Hit");
        oriRec.localRotation = Quaternion.Euler(0f, 0f, 0f);
        seq.AppendInterval(.05f);
        seq.Append(oriRec.DOScale(1f, .18f).SetEase(Ease.OutBack, 1.7f));
        seq.AppendCallback(() => PlayUISound(popupSound));

        // Coin
        seq.AppendInterval(0.01f);
        seq.Append(
            coinRec.DOScale(1f, 0.15f)
                    .SetEase(Ease.OutBack)
        );
        seq.AppendCallback(() => PlayUISound(popupSound));

        // Kill
        seq.AppendInterval(0.01f);
        seq.Append(
            killRec.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
        );
        seq.AppendCallback(() => PlayUISound(popupSound));

        // Stamp
        seq.AppendInterval(0.1f);
        seq.Append(
            stampRec.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
        );
        seq.AppendCallback(() => PlayUISound(stampSound));
    }
    #endregion

    #region 무한 스테이지 애니메이션
    void PlayInfiniteAwardsSequence(int killNum, int coinNum, int wave, string survivalTime, string bestRecord)
    {
        bouncerManager.Jump(confettiNums); // 150마리 폭죽

        ResetRecs();
        titleText.text = "도전 결과";
        killText.text = killNum.ToString();
        coinText.text = coinNum.ToString();
        survivalTimeText.text = survivalTime;
        bestRecordText.text = bestRecord;

        TMP_Typewriter survivalTimeTitle = survivalTimeTitleText.GetComponent<TMP_Typewriter>();
        TMP_Typewriter survivalTimeContents = survivalTimeText.GetComponent<TMP_Typewriter>();
        TMP_Typewriter bestRecordTimeTitle = bestRecordTitleText.GetComponent<TMP_Typewriter>();
        TMP_Typewriter bestRecordTimeContents = bestRecordText.GetComponent<TMP_Typewriter>();
        survivalTimeTitle.ResetMaxVisibleChar();
        survivalTimeContents.ResetMaxVisibleChar();
        bestRecordTimeTitle.ResetMaxVisibleChar();
        bestRecordTimeContents.ResetMaxVisibleChar();

        stageNumRec.gameObject.SetActive(false);

        Sequence seq = DOTween.Sequence();
        // UI는 타임스케일 무시
        seq.SetUpdate(true);

        // panel
        seq.AppendInterval(1f);
        seq.AppendCallback(() =>
        {
            PlayUISound(panelSound);
            PlayUISound(panelSound);
        });
        seq.Append(panelRec.DOScale(.8f, .2f).SetEase(Ease.OutBack));

        // ori
        charAnim.SetTrigger("Idle");
        oriRec.localRotation = Quaternion.Euler(0f, 0f, 0f);
        seq.AppendInterval(.01f);
        seq.AppendCallback(() => PlayUISound(popupSound));
        seq.Append(oriRec.DOScale(1f, .18f).SetEase(Ease.OutBack, 1.7f));

        // title
        seq.AppendInterval(.05f);
        titleRec.localScale = Vector2.one;
        seq.AppendCallback(() =>
        {
            PlayUISound(popupSound);
        });
        
        // 현재 시간
        seq.AppendInterval(.5f);
        survivalTimeRec.localScale = Vector2.one;
        seq.AppendCallback(() => 
        {
            PlayUISound(scribbleSound);
            survivalTimeTitle.Play();
        });

        seq.AppendInterval(.15f);
        seq.AppendCallback(() => 
        {
            survivalTimeContents.Play();
        });

        // 최고 기록
        seq.AppendInterval(.3f);
        bestRecordRec.localScale = Vector2.one;
        seq.AppendCallback(() =>
        {
            PlayUISound(scribbleSound);
            bestRecordTimeTitle.Play();
        });
        
        seq.AppendInterval(.15f);
        seq.AppendCallback(() =>
        {
            bestRecordTimeContents.Play();
        });

        // Coin
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() => PlayUISound(TitleSound));
        seq.Append(coinRec.DOScale(1f, 0.15f).SetEase(Ease.OutBack));

        // Kill
        seq.AppendInterval(0.01f);
        seq.AppendCallback(() => PlayUISound(TitleSound));
        seq.Append(killRec.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

        // Stamp
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() =>
        {
            PlayUISound(stampSound);
            stampRec.localScale = Vector2.one;
        });

        // Result Sound
        seq.AppendInterval(0.2f);
        seq.AppendCallback(() =>
        {
            PlayUISound(resultSoundSuccess);
        });

        // 탭해서 계속하기
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() =>
        {
            GameManager.instance.ActivateConfirmationButtonWithoutDelay();
        });
    }
    #endregion

    void ResetRecs()
    {
        // 안전 처리
        DOTween.Kill(titleRec);
        DOTween.Kill(coinRec);
        DOTween.Kill(killRec);
        DOTween.Kill(survivalTimeRec);
        DOTween.Kill(bestRecordRec);
        DOTween.Kill(oriRec);

        // 초기 상태
        panelRec.localScale = Vector2.zero;
        titleRec.localScale = Vector2.zero;
        coinRec.localScale = Vector2.zero;
        killRec.localScale = Vector2.zero;
        survivalTimeRec.localScale = Vector2.zero;
        bestRecordRec.localScale = Vector2.zero;
        oriRec.localScale = Vector2.zero;
        stampRec.localScale = Vector2.zero;
    }

    void SetEquipSpriteRow()
    {
        WeaponData wd = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        cardDisp.InitWeaponCardDisplay(wd, null);
        cardDisp.InitSpriteRow(); // card sprite row의 이미지 참조들이 남지 않게 초기화

        for (int i = 0; i < 4; i++)
        {
            Item item = GameManager.instance.startingDataContainer.GetItemDatas()[i];

            if (item == null)
            {
                cardDisp.SetEquipCardDisplay(i, null, false, Vector2.zero); // 이미지 오브젝트를 비활성화
                continue;
            }
            SpriteRow equipmentSpriteRow = item.spriteRow;
            Vector2 offset = item.needToOffset ? item.posHead : Vector2.zero;

            cardDisp.SetEquipCardDisplay(i, equipmentSpriteRow, item.needToOffset, offset);
        }
    }
    void PlayUISound(AudioClip audioClip)
    {
        SoundManager.instance.Play(audioClip);
    }
}