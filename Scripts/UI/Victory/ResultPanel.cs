using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
    ResultPanelUI resultPanelUI;

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
    [SerializeField] GameObject bestRecordCircle;
    [SerializeField] RectTransform shineEffect;
    bool isNewRecord; // 웨이브 기록이 갱신이 되었는지

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

    [Header("무한모드 사운드")]
    [SerializeField] AudioClip[] newRecordSound;


    public void InitAwards(int killNum, int coinNum, int stageNum, bool isWinningStage)
    {
        SetBG();
        PlayRegularAwardsSequence(killNum, coinNum, stageNum, isWinningStage);
    }
    public void InitInfiniteAwards(int killNum, int coinNum, int currentWave, string survivalTime, string bestRecord, bool isBestRecord)
    {
        SetBG();
        isNewRecord = isBestRecord;
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
    
    #region 레귤러 애니메이션
    void PlayRegularAwardsSequence(int killNum, int coinNum, int stageNum, bool isWinningStage)
    {
        if (isWinningStage)
        {
            bouncerManager.JumpHappy(confettiNums); // 150마리 폭죽
        }
        else
        {
            bouncerManager.JumpSad(30); // 50마리 슬픈 폭죽
        }
        

        ResetRecs();
        string title = isWinningStage ? "축하해요!" : "실패...";
        string stamp = isWinningStage ? "참\n잘했어요!" : "아쉬워요..";
        titleText.text = title;
        killText.text = killNum.ToString();
        coinText.text = coinNum.ToString();
        stageNumberText.text = stageNum.ToString();
        stampText.text = stamp;

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
        string animTrigger = isWinningStage ? "Idle" : "Hit";
        GenWeaponCards(animTrigger);

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

            panelRec.DOShakePosition(.5f, strength: 20f, vibrato: 30, randomness: 30)
            .SetUpdate(true);
        });

        // Result Sound
        seq.AppendInterval(0.2f);
        seq.AppendCallback(() =>
        {
            if (isWinningStage)
            {
                PlayUISound(resultSoundSuccess);
            }
            else
            {
                PlayUISound(resultSoundFail);
            }
        });

        // 탭해서 계속하기
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() =>
        {
            GameManager.instance.ActivateConfirmationButtonWithoutDelay();
        });
    }
    #endregion

    #region 무한 스테이지 애니메이션
    void PlayInfiniteAwardsSequence(int killNum, int coinNum, int wave, string survivalTime, string bestRecord)
    {
        bouncerManager.JumpHappy(confettiNums); // 150마리 폭죽

        ResetRecs();
        titleText.text = "도전 결과";
        killText.text = killNum.ToString();
        coinText.text = coinNum.ToString();
        survivalTimeText.text = survivalTime;
        bestRecordText.text = bestRecord;

        bestRecordCircle.SetActive(false);

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
        // charAnim.SetTrigger("Idle");
        // oriRec.localRotation = Quaternion.Euler(0f, 0f, 0f);
        GenWeaponCards("Idle");

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


        /// ⭐ 신기록 연출 (핵심)
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() =>
        {
            if (isNewRecord)
            {
                foreach (var clip in newRecordSound)
                    SoundManager.instance.Play(clip);

                bestRecordCircle.SetActive(true);

                // ⭐ Shine Effect 추가
                shineEffect.gameObject.SetActive(true);
                // shineEffect.position = bestRecordRec.position;
                shineEffect.localScale = Vector3.one;

                // CanvasGroup이 있다면 alpha 조절용
                CanvasGroup shineCanvas = shineEffect.GetComponent<CanvasGroup>();
                if (shineCanvas != null)
                {
                    shineCanvas.alpha = 1f;
                    shineCanvas.DOFade(0f, .6f).SetEase(Ease.OutQuad).SetUpdate(true);
                }
                // 또는 Image 컴포넌트가 있다면
                else
                {
                    Image shineImage = shineEffect.GetComponent<Image>();
                    if (shineImage != null)
                    {
                        Color c = shineImage.color;
                        c.a = 1f;
                        shineImage.color = c;
                        shineImage.DOFade(0f, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true);
                    }
                }

                // x축만 1.5배 스케일업 애니메이션
                shineEffect.DOScale(new Vector3(1.5f, 2f, 1f), 0.4f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true)
                    .OnComplete(() => shineEffect.gameObject.SetActive(false));

                // 텍스트 색상 반짝임 효과
                Color originalColor = bestRecordTitleText.color;
                Color flashColor = Color.white; // 원하는 색상으로 변경 가능 (예: Color.white, new Color(1f, 0.8f, 0f))

                bestRecordTitleText.DOColor(flashColor, 0.1f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true)
                    .OnComplete(() =>
                        bestRecordTitleText.DOColor(originalColor, 0.1f).SetEase(Ease.InQuad).SetUpdate(true)
                    );

                bestRecordText.DOColor(flashColor, 0.1f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true)
                    .OnComplete(() =>
                        bestRecordText.DOColor(originalColor, 0.1f).SetEase(Ease.InQuad).SetUpdate(true)
                    );

                bestRecordRec
                    .DOScale(1.25f, 0.1f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .OnComplete(() =>
                        bestRecordRec.DOScale(1f, 0.1f)
                            .SetEase(Ease.OutQuad)
                            .SetUpdate(true)
                    );
            }
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

            panelRec.DOShakePosition(.5f, strength: 20f, vibrato: 30, randomness: 30)
            .SetUpdate(true);

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
    void GenWeaponCards(string animTrigger)
    {
        GetComponent<ResultPanelUI>().ShowResults(animTrigger);
    }
}