using System.Collections;
using UnityEngine;
public class AchievementTutorialController : MonoBehaviour
{
    public static AchievementTutorialController instance;
    [Header("오버레이")]
    [SerializeField] TutorialHighlight tutorialHighlight;
    [Header("하이라이트 대상 - 정적")]
    [SerializeField] RectTransform achievementTabButton;
    [Header("업적 패널 참조")]
    [SerializeField] AchievementPanel achievementPanel;
    [SerializeField] string tutorialAchievementId = "tutorial_merge";
    [Header("합성 성공 패널 참조")]
    [SerializeField] GameObject upgradeSuccessPanel;
    [Header("클릭 차단")]
    [SerializeField] GameObject fg;
    [Header("팝업")]
    [SerializeField] GameObject achievementOpenPopup;
    [Header("튜토리얼 완료 팝업")]
    [SerializeField] GameObject tutorialCompletePopup;
    [Header("튜토리얼 완료 이펙트")]
    [SerializeField] ParticleSystem confettiEffect;
    [SerializeField] AudioClip completeSound;
    // ─────────────────────────────────────────
    // 내부 상태
    // ─────────────────────────────────────────
    enum AchievementTutorialPhase
    {
        None,
        WaitingForTapToContinue,
        HighlightAchievementTab,
        HighlightRewardButton,
        Done
    }
    AchievementTutorialPhase phase = AchievementTutorialPhase.None;
    // ─────────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────────
    void Awake()
    {
        instance = this;
    }
    void OnEnable()
    {
        TutorialManager.OnStepChanged += OnStepChanged;
    }
    void OnDisable()
    {
        TutorialManager.OnStepChanged -= OnStepChanged;
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAnyRewarded -= OnAnyRewarded;
        GemCollectFX.OnAllGemsCollected -= OnGemsCollectedThenComplete;
    }
    void OnStepChanged(TutorialStep step)
    {
        if (step == TutorialStep.Step4_AchievementUnlocked)
        {
            if (upgradeSuccessPanel != null && upgradeSuccessPanel.activeSelf)
                phase = AchievementTutorialPhase.WaitingForTapToContinue;
            else
                StartAchievementTutorial();
        }
        else
        {
            // ✅ 이미 스스로 정리된 상태(phase==None)라면 fg를 다시 건드리지 않음
            if (phase != AchievementTutorialPhase.None)
                HideAll();
        }
    }
    // ─────────────────────────────────────────
    // 튜토리얼 흐름
    // ─────────────────────────────────────────
    public void OnMergeSuccessClosed()
    {
        if (phase != AchievementTutorialPhase.WaitingForTapToContinue) return;
        StartAchievementTutorial();
    }
    void StartAchievementTutorial()
    {
        StartCoroutine(WaitThenStart());
    }
    IEnumerator WaitThenStart()
    {
        if (fg != null) fg.SetActive(true);
        
        yield return new WaitUntil(() => GameInitializer.IsInitialized);
        yield return new WaitForSeconds(0.5f);
        phase = AchievementTutorialPhase.HighlightAchievementTab;
        ShowPopup(achievementOpenPopup);
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAnyRewarded -= OnAnyRewarded;
            AchievementManager.Instance.OnAnyRewarded += OnAnyRewarded;
        }
        yield return new WaitForSeconds(1.5f);
        if (phase != AchievementTutorialPhase.HighlightAchievementTab) yield break;
        tutorialHighlight.HighlightUI(achievementTabButton, fg);
    }
    public void OnAchievementPanelEntered()
    {
        Debug.Log($"[AchievementTutorial] OnAchievementPanelEntered 호출됨 - phase: {phase}");
        if (phase != AchievementTutorialPhase.HighlightAchievementTab) return;
        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true);
        StartCoroutine(SwitchTabThenHighlightReward());
    }
    IEnumerator SwitchTabThenHighlightReward()
    {
        yield return new WaitForSeconds(0.3f);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
            achievementPanel.GetContentRect());
        yield return null;
        yield return null;
        RectTransform rewardBtn = achievementPanel.GetRewardButtonRect(tutorialAchievementId);
        if (rewardBtn == null)
        {
            Logger.LogWarning("[AchievementTutorial] 보상 버튼을 찾을 수 없습니다. 튜토리얼을 강제 완료 처리합니다.");
            ForceCompleteTutorial();
            yield break;
        }
        phase = AchievementTutorialPhase.HighlightRewardButton;
        tutorialHighlight.HighlightUI(rewardBtn, fg);
        Logger.Log("[AchievementTutorial] 보상 버튼 하이라이트 완료");
    }
    void OnAnyRewarded(RuntimeAchievement ra)
    {
        if (phase != AchievementTutorialPhase.HighlightRewardButton) return;
        if (ra.original.id != tutorialAchievementId) return;
        HideAll();
        GemCollectFX.OnAllGemsCollected += OnGemsCollectedThenComplete;
    }
    void OnGemsCollectedThenComplete()
    {
        GemCollectFX.OnAllGemsCollected -= OnGemsCollectedThenComplete;
        if (confettiEffect != null)
        {
            confettiEffect.gameObject.SetActive(true);
            confettiEffect.Play();
            StartCoroutine(DeactivateAfterPlay(confettiEffect));
        }
        if (completeSound != null)
            SoundManager.instance.Play(completeSound);
        ShowPopup(tutorialCompletePopup);
        TutorialManager.instance?.AdvanceStep(); // → Completed
    }
    IEnumerator DeactivateAfterPlay(ParticleSystem ps)
    {
        yield return new WaitForSeconds(ps.main.duration);
        ps.gameObject.SetActive(false);
    }
    // ⭐ 추가: 업적 보상 버튼을 찾지 못해 튜토리얼을 진행시킬 수 없을 때
    // confetti/사운드/보상 지급 없이 튜토리얼 전체를 강제 완료 처리
    void ForceCompleteTutorial()
    {
        HideAll();
        TutorialManager.instance?.SetStep(TutorialStep.Completed);
        FindObjectOfType<GachaSystem>()?.SkipTutorialRewardGift();
    }
    // ─────────────────────────────────────────
    // 공통 유틸리티
    // ─────────────────────────────────────────
    void HideAll()
    {
        StopAllCoroutines();
        tutorialHighlight?.Hide();
        if (fg != null) fg.SetActive(false);
        phase = AchievementTutorialPhase.None;
    }
    void ShowPopup(GameObject popup)
    {
        if (popup == null) return;
        popup.SetActive(true);
        PanelTween tween = popup.GetComponent<PanelTween>();
        if (tween != null) tween.ShowWithScale();
    }
}