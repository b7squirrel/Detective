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
    }

    void OnStepChanged(TutorialStep step)
    {
        if (step == TutorialStep.Step4_AchievementUnlocked)
        {
            // 합성 성공 패널이 열려있으면 닫힐 때까지 대기
            // 재시작 시에는 패널이 없으므로 바로 시작
            if (upgradeSuccessPanel != null && upgradeSuccessPanel.activeSelf)
            {
                // ✅ 탭해서 계속하기 버튼을 기다리는 상태로 전환
                phase = AchievementTutorialPhase.WaitingForTapToContinue;
            }
            else
            {
                // 재시작 시: 바로 튜토리얼 시작
                StartAchievementTutorial();
            }
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

        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAnyRewarded += OnAnyRewarded;

        yield return new WaitForSeconds(1.5f);

        // ✅ 추가: 딜레이 동안 이미 패널에 진입했다면 하이라이트 건너뜀
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
        achievementPanel.SwitchTabPublic(TabTypePublic.Permanent);

        // 탭 전환 + UI 갱신 대기
        yield return new WaitForSeconds(0.3f);
        yield return null; // 1프레임 추가 대기

        RectTransform rewardBtn = achievementPanel.GetRewardButtonRect(tutorialAchievementId);
        Debug.Log($"[AchievementTutorial] rewardBtn: {(rewardBtn == null ? "NULL" : rewardBtn.name)}");
        
        if (rewardBtn == null)
        {
            Debug.LogWarning("[AchievementTutorial] 보상 버튼을 찾을 수 없습니다.");
            if (fg != null) fg.SetActive(false);
            yield break;
        }

        phase = AchievementTutorialPhase.HighlightRewardButton;
        if (fg != null) fg.SetActive(false);
        tutorialHighlight.HighlightUI(rewardBtn);
    }

    // [3단계] 보상 버튼 클릭 → AdvanceStep()
    // AchievementManager.OnAnyRewarded 이벤트로 감지
    void OnAnyRewarded(RuntimeAchievement ra)
    {
        if (phase != AchievementTutorialPhase.HighlightRewardButton) return;
        if (ra.original.id != tutorialAchievementId) return;

        HideAll();
        TutorialManager.instance?.AdvanceStep(); // → Completed
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