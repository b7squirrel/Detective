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
    // 합성 성공 화면이 열려있는지 확인하기 위해
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
        WaitingForTapToContinue,  // 합성 성공 화면이 닫히길 대기 중
        HighlightAchievementTab,  // Achievement 탭 클릭 유도
        HighlightRewardButton,    // 보상 버튼 클릭 유도
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

        // ✅ 추가
        GemCollectFX.OnAllGemsCollected -= OnGemsCollectedThenComplete;
    }

    void OnStepChanged(TutorialStep step)
    {
        if (step == TutorialStep.Step4_AchievementUnlocked)
        {
            // ✅ 수정: upgradeSuccessPanel이 null이거나 비활성이면 바로 시작
            if (upgradeSuccessPanel != null && upgradeSuccessPanel.activeSelf)
                phase = AchievementTutorialPhase.WaitingForTapToContinue;
            else
                StartAchievementTutorial();
        }
        else
        {
            HideAll();
        }
    }

    // ─────────────────────────────────────────
    // 튜토리얼 흐름
    // ─────────────────────────────────────────

    // UpPanelUI.CloseUpgradeSuccessUI()에서 호출
    // "탭해서 계속하기" 버튼을 눌러 합성 성공 화면이 닫힐 때 실행
    public void OnMergeSuccessClosed()
    {
        if (phase != AchievementTutorialPhase.WaitingForTapToContinue) return;
        StartAchievementTutorial();
    }

    // [1단계] Achievement 탭 하이라이트
    void StartAchievementTutorial()
    {
        StartCoroutine(WaitThenStart());
    }

    // 게임 초기화가 완전히 끝날 때까지 대기
    // 이 시점이면 메인 메뉴가 실제로 보이는 상태
    IEnumerator WaitThenStart()
    {
        yield return new WaitUntil(() => GameInitializer.IsInitialized);
        yield return new WaitForSeconds(0.5f);

        phase = AchievementTutorialPhase.HighlightAchievementTab;

        if (fg != null) fg.SetActive(true);
        ShowPopup(achievementOpenPopup);

        // ✅ 수정: 구독 전 기존 구독 해제로 이중 구독 방지
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAnyRewarded -= OnAnyRewarded;
            AchievementManager.Instance.OnAnyRewarded += OnAnyRewarded;
        }

        yield return new WaitForSeconds(1.5f);

        if (phase != AchievementTutorialPhase.HighlightAchievementTab) yield break;

        if (fg != null) fg.SetActive(false);
        tutorialHighlight.HighlightUI(achievementTabButton);
    }

    // [2단계] Achievement 탭 클릭 → 영구 업적 탭 전환 후 보상 버튼 하이라이트
    // MainMenuManager.SetTabPos()에서 호출
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
        // 영구 업적 탭으로 전환 (tutorial_merge는 영구 업적)
        // achievementPanel.SwitchTabPublic(TabTypePublic.Permanent);

        // 탭 전환 + UI 갱신 대기
        yield return new WaitForSeconds(0.3f);

        // ✅ 레이아웃 강제 재계산 (ContentSizeFitter 업데이트)
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
            achievementPanel.GetContentRect());

        // ✅ 재계산 후 2프레임 대기 (렌더링 반영 완료 보장)
        yield return null;
        yield return null;

        RectTransform rewardBtn = achievementPanel.GetRewardButtonRect(tutorialAchievementId);
        if (rewardBtn == null)
        {
            Logger.LogWarning("[AchievementTutorial] 보상 버튼을 찾을 수 없습니다.");
            if (fg != null) fg.SetActive(false);
            yield break;
        }

        phase = AchievementTutorialPhase.HighlightRewardButton;
        if (fg != null) fg.SetActive(false);
        tutorialHighlight.HighlightUI(rewardBtn);
        Logger.Log("[AchievementTutorial] 보상 버튼 하이라이트 완료");
    }

    // [3단계] 보상 버튼 클릭 → AdvanceStep()
    // AchievementManager.OnAnyRewarded 이벤트로 감지
    void OnAnyRewarded(RuntimeAchievement ra)
    {
        if (phase != AchievementTutorialPhase.HighlightRewardButton) return;
        if (ra.original.id != tutorialAchievementId) return;

        HideAll();

        // ✅ 보석 수집 완료 후 팝업 표시
        GemCollectFX.OnAllGemsCollected += OnGemsCollectedThenComplete;
    }

    // ✅ 추가: 보석 수집 완료 → 팝업 → AdvanceStep
    void OnGemsCollectedThenComplete()
    {
        GemCollectFX.OnAllGemsCollected -= OnGemsCollectedThenComplete;

        // ✅ confetti 파티클 활성화 + 재생
        if (confettiEffect != null)
        {
            confettiEffect.gameObject.SetActive(true);
            confettiEffect.Play();

            // ✅ 파티클 재생 시간 후 자동 비활성화
            StartCoroutine(DeactivateAfterPlay(confettiEffect));
        }

        // ✅ 완료 사운드 재생
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

    // ─────────────────────────────────────────
    // 공통 유틸리티
    // ─────────────────────────────────────────

    // 지연 후 고정 타겟 하이라이트 (Achievement 탭 버튼 등)
    IEnumerator HighlightAfterDelay(RectTransform target,
        AchievementTutorialPhase nextPhase, float delay)
    {
        yield return new WaitForSeconds(delay);
        phase = nextPhase;
        if (fg != null) fg.SetActive(false);
        tutorialHighlight.HighlightUI(target);
    }

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