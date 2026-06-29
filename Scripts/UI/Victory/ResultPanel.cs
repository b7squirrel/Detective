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
    [SerializeField] ResultRewardCard killRewardCard;
    [SerializeField] ResultRewardCard clearRewardCard;
    [SerializeField] Sprite goldSprite;
    ResultPanelUI resultPanelUI;

    [Header("일반 모드 요소")]
    [SerializeField] RectTransform stageNumRec;
    [SerializeField] TMPro.TextMeshProUGUI stageNumberText;
    [SerializeField] GameObject tearEmitters;
    [SerializeField] Image faceExpressetion;
    [SerializeField] Sprite cryingFaceSprite;

    [Header("무한 모드 요소")]
    [SerializeField] RectTransform survivalTimeRec;
    [SerializeField] RectTransform bestRecordRec;
    [SerializeField] TMPro.TextMeshProUGUI survivalTimeText;
    [SerializeField] TMPro.TextMeshProUGUI survivalTimeTitleText;
    [SerializeField] TMPro.TextMeshProUGUI bestRecordText;
    [SerializeField] TMPro.TextMeshProUGUI bestRecordTitleText;
    [SerializeField] GameObject bestRecordCircle;
    [SerializeField] RectTransform shineEffect;
    bool isNewRecord;

    [SerializeField] bool isDarkBG;
    [SerializeField] CardDisp cardDisp;
    [SerializeField] Animator charAnim;

    [Header("오리폭죽")]
    [SerializeField] ImageBouncerManager bouncerManager;
    [SerializeField] int confettiNums;

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

    [Header("디버그")]
    [SerializeField] bool debugOriOnly; // 촬영용: 오리만 표시
    [SerializeField] GameObject panelFrame;


    public void InitAwards(int killNum, int coinNum, int stageNum, bool isWinningStage, int killGold, int clearBonus)
    {
        SetBG();
        if (debugOriOnly) { StartCoroutine(PlayDebugOriOnly("Idle")); return; }
        PlayRegularAwardsSequence(killNum, killGold, stageNum, isWinningStage, clearBonus);
    }

    public void InitInfiniteAwards(int killNum, int coinNum, int currentWave,
    string survivalTime, string bestRecord, bool isBestRecord,
    int infiniteGold, int killGold, int waveBonus)
    {
        SetBG();
        isNewRecord = isBestRecord;
        if (debugOriOnly) { StartCoroutine(PlayDebugOriOnly("Idle")); return; }
        PlayInfiniteAwardsSequence(killNum, infiniteGold, killGold, waveBonus, currentWave, survivalTime, bestRecord);
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

    #region 디버그: 오리만 표시
    IEnumerator PlayDebugOriOnly(string animTrigger)
    {
        ResetRecs();

        // 패널 배경만 표시
        panelRec.localScale = Vector3.one * 0.8f;
        stageNumRec.gameObject.SetActive(false);
        if (panelFrame != null) panelFrame.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(0.5f);

        // 오리만 등장
        GenWeaponCards(animTrigger);
        PlayUISound(popupSound);
        oriRec.DOScale(1f, .18f).SetEase(Ease.OutBack, 1.7f).SetUpdate(true);
    }
    #endregion

    #region 레귤러 애니메이션
    void PlayRegularAwardsSequence(int killNum, int killGold, int stageNum, bool isWinningStage, int clearBonus)
    {
        if (isWinningStage)
            bouncerManager.JumpHappy(confettiNums);
        else
            bouncerManager.JumpSad(30);

        ResetRecs();

        var g = LocalizationManager.Game;
        string title = isWinningStage ? g.congratulations : g.failed;
        string stamp = isWinningStage ? g.greatJob : g.soClose;
        titleText.text = title;
        killText.text = killNum.ToString();
        coinText.text = killGold.ToString();
        stageNumberText.text = LocalizationManager.Game.stage + " " + stageNum.ToString();
        stampText.text = stamp;

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        // panel
        seq.AppendInterval(1f);
        seq.AppendCallback(() => { PlayUISound(panelSound); PlayUISound(panelSound); });
        seq.Append(panelRec.DOScale(.8f, .2f).SetEase(Ease.OutBack));

        // ori
        string animTrigger = isWinningStage ? "Idle" : "Hit";
        GenWeaponCards(animTrigger);
        if (!isWinningStage) faceExpressetion.sprite = cryingFaceSprite;

        seq.AppendInterval(.01f);
        seq.AppendCallback(() => PlayUISound(popupSound));
        seq.Append(oriRec.DOScale(1f, .18f).SetEase(Ease.OutBack, 1.7f));
        seq.AppendCallback(() => { if (!isWinningStage) tearEmitters.SetActive(true); });

        // title
        seq.AppendInterval(.05f);
        titleRec.localScale = Vector2.one;
        seq.AppendCallback(() => PlayUISound(popupSound));

        // ★ 보상 카드
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() =>
        {
            Logger.Log($"[RewardCard] killGold={killGold}, clearBonus={clearBonus}, isWinning={isWinningStage}");

            killRewardCard.Initialize(g.killBonus, goldSprite, killGold, delay: 0f);
            clearRewardCard.Initialize(g.stageReward, goldSprite, clearBonus, delay: 0.2f);
        });

        // Stamp
        seq.AppendInterval(.6f);
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
            PlayUISound(isWinningStage ? resultSoundSuccess : resultSoundFail);
        });

        // 탭해서 계속하기
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() => GameManager.instance.ActivateConfirmationButtonWithoutDelay());
    }
    #endregion

    #region 무한 스테이지 애니메이션
    void PlayInfiniteAwardsSequence(int killNum, int infiniteGold, int killGold, int waveBonus,
    int wave, string survivalTime, string bestRecord)
    {
        bouncerManager.JumpHappy(confettiNums);

        ResetRecs();
        var g = LocalizationManager.Game;
        titleText.text = g.challengeResult;
        killText.text = killNum.ToString();
        coinText.text = infiniteGold.ToString();
        survivalTimeTitleText.text = g.currentRecord;
        bestRecordTitleText.text = g.bestWave;
        stampText.text = g.greatJob;

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

        // ⭐ 신기록 연출
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() =>
        {
            if (isNewRecord)
            {
                foreach (var clip in newRecordSound)
                    SoundManager.instance.Play(clip);

                bestRecordCircle.SetActive(true);

                shineEffect.gameObject.SetActive(true);
                shineEffect.localScale = Vector3.one;

                CanvasGroup shineCanvas = shineEffect.GetComponent<CanvasGroup>();
                if (shineCanvas != null)
                {
                    shineCanvas.alpha = 1f;
                    shineCanvas.DOFade(0f, .6f).SetEase(Ease.OutQuad).SetUpdate(true);
                }
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

                shineEffect.DOScale(new Vector3(1.5f, 2f, 1f), 0.4f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true)
                    .OnComplete(() => shineEffect.gameObject.SetActive(false));

                Color originalColor = bestRecordTitleText.color;
                Color flashColor = Color.white;

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

        // ★ 보상 카드
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() =>
        {
            var g = LocalizationManager.Game;
            killRewardCard.Initialize(g.killBonus, goldSprite, killGold, delay: 0f);
            clearRewardCard.Initialize(g.waveBonus, goldSprite, waveBonus, delay: 0.4f);
        });

        // Stamp
        seq.AppendInterval(.6f);
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
        DOTween.Kill(titleRec);
        DOTween.Kill(coinRec);
        DOTween.Kill(killRec);
        DOTween.Kill(survivalTimeRec);
        DOTween.Kill(bestRecordRec);
        DOTween.Kill(oriRec);

        panelRec.localScale = Vector2.zero;
        titleRec.localScale = Vector2.zero;
        coinRec.localScale = Vector2.zero;
        killRec.localScale = Vector2.zero;
        survivalTimeRec.localScale = Vector2.zero;
        bestRecordRec.localScale = Vector2.zero;
        oriRec.localScale = Vector2.zero;
        stampRec.localScale = Vector2.zero;

        killRewardCard.Hide();
        clearRewardCard.Hide();
    }

    void SetEquipSpriteRow()
    {
        WeaponData wd = GameManager.instance.startingDataContainer.GetLeadWeaponData();
        cardDisp.InitWeaponCardDisplay(wd, null);
        cardDisp.InitSpriteRow();

        for (int i = 0; i < 4; i++)
        {
            Item item = GameManager.instance.startingDataContainer.GetItemDatas()[i];

            if (item == null)
            {
                cardDisp.SetEquipCardDisplay(i, null, false, Vector2.zero);
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