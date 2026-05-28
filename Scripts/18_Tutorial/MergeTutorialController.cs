using System.Collections;
using UnityEngine;

public class MergeTutorialController : MonoBehaviour
{
    public static MergeTutorialController instance;

    [Header("오버레이")]
    [SerializeField] TutorialHighlight tutorialHighlight;

    [Header("하이라이트 대상 - 정적")]
    [SerializeField] RectTransform mergeTabButton;   // 하단 탭의 Merge(합성) 버튼
    [SerializeField] RectTransform confirmButton;    // 합성 확인 버튼 (Buttons 컨테이너 안)

    [Header("슬롯 풀 - AllField/MatField 공용")]
    // AllField와 MatField 모두 Present Slot Pool의 슬롯을 보이고 숨기는 방식이므로
    // 하나의 Transform을 공용으로 사용
    [SerializeField] Transform presentSlotPool;      // Slot Containers > Present Field > Viewport > Content > Present Slot Pool

    [Header("클릭 차단")]
    [SerializeField] GameObject fg;

    [Header("팝업")]
    [SerializeField] GameObject mergeOpenPopup;

    // ─────────────────────────────────────────
    // 내부 상태
    // ─────────────────────────────────────────
    enum MergeTutorialPhase
    {
        None,
        HighlightMergeTab,       // Merge 탭 클릭 유도
        HighlightUpCard,         // 업그레이드 카드 클릭 유도
        HighlightMatCard,        // 재료 카드 클릭 유도
        HighlightConfirmButton,  // 합성 확인 버튼 클릭 유도
        Done
    }
    MergeTutorialPhase phase = MergeTutorialPhase.None;

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
    }

    void OnStepChanged(TutorialStep step)
    {
        if (step == TutorialStep.Step3_MergeUnlocked)
            StartMergeTutorial();
        else
            HideAll();
    }

    // ─────────────────────────────────────────
    // 튜토리얼 흐름
    // ─────────────────────────────────────────

    // [1단계] Step3 진입 → Merge 탭 하이라이트
    void StartMergeTutorial()
    {
        StartCoroutine(WaitThenStart());
    }

    // 게임 초기화가 완전히 끝날 때까지 대기
    // 이 시점이면 메인 메뉴가 실제로 보이는 상태
    IEnumerator WaitThenStart()
    {
        if (fg != null) fg.SetActive(true);
        
        yield return new WaitUntil(() => GameInitializer.IsInitialized);
        yield return new WaitForSeconds(0.5f); // UI 렌더링 여유

        ShowPopup(mergeOpenPopup);
        StartCoroutine(HighlightAfterDelay(mergeTabButton, MergeTutorialPhase.HighlightMergeTab, 1.5f));
    }

    // [2단계] Merge 탭 클릭 → AllField 첫 번째 활성 슬롯(업그레이드 카드) 하이라이트
    // UpPanelManager.OnEnable()에서 호출
    public void OnMergePanelEntered()
    {
        if (phase != MergeTutorialPhase.HighlightMergeTab) return;

        // ✅ 추가: 이전 코루틴 정리 후 시작 (패널 재진입 시 중복 방지)
        StopAllCoroutines();

        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true);
        StartCoroutine(HighlightFirstSlotAfterDelay(MergeTutorialPhase.HighlightUpCard, 0.5f));
    }

    // [3단계] 업그레이드 카드 클릭 → MatField 첫 번째 활성 슬롯(재료 카드) 하이라이트
    // UpPanelManager.GetIntoMatField()에서 호출
    // ※ AllField → MatField 전환 시 ClearPresentationField()가 전부 비활성화 후
    //   재료 가능한 카드만 다시 활성화하므로 첫 번째 활성 슬롯 = 재료 카드로 올바르게 작동
    public void OnUpCardSelected()
    {
        if (phase != MergeTutorialPhase.HighlightUpCard) return;

        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true);
        // MatField는 GenerateMatCardsList 후 슬롯이 활성화되므로 충분히 대기
        StartCoroutine(HighlightFirstSlotAfterDelay(MergeTutorialPhase.HighlightMatCard, 0.6f));
    }

    // [4단계] 재료 카드 클릭 → 합성 확인 버튼 하이라이트
    // UpPanelManager.GetIntoConfirmation()에서 호출
    public void OnMatCardSelected()
    {
        if (phase != MergeTutorialPhase.HighlightMatCard) return;

        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true);
        // UpgradeConfirmationAnimation이 끝날 때까지 대기
        StartCoroutine(HighlightAfterDelay(confirmButton, MergeTutorialPhase.HighlightConfirmButton, 0.4f));
    }

    // [5단계] 합성 확인 버튼 클릭
    // → UpPanelManager.UpgradeCard() → UpgradeUICo() → AdvanceStep() 자동 호출 ✅
    // → OnStepChanged(Step4) → HideAll() 자동 호출 ✅

    // ─────────────────────────────────────────
    // 공통 유틸리티
    // ─────────────────────────────────────────

    // 지연 후 고정 타겟 하이라이트 (Merge 탭, 합성 확인 버튼 등)
    IEnumerator HighlightAfterDelay(RectTransform target, MergeTutorialPhase nextPhase, float delay)
    {
        yield return new WaitForSeconds(delay);
        phase = nextPhase;
        // ✅ 수정
        tutorialHighlight.HighlightUI(target, fg);
    }

    // 지연 후 Present Slot Pool의 첫 번째 활성 슬롯 하이라이트
    // AllField와 MatField 모두 Present Slot Pool을 공용으로 사용하므로 단일 메서드로 처리
    IEnumerator HighlightFirstSlotAfterDelay(MergeTutorialPhase nextPhase, float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return null;

        RectTransform firstSlot = GetFirstActiveSlot();
        if (firstSlot == null)
        {
            Debug.LogWarning("[MergeTutorial] 첫 번째 슬롯을 찾을 수 없습니다.");
            if (fg != null) fg.SetActive(false);
            yield break;
        }

        phase = nextPhase;
        // ✅ 수정
        tutorialHighlight.HighlightUI(firstSlot, fg);
    }

    // Present Slot Pool에서 활성화된 첫 번째 자식의 RectTransform 반환
    RectTransform GetFirstActiveSlot()
    {
        if (presentSlotPool == null) return null;

        for (int i = 0; i < presentSlotPool.childCount; i++)
        {
            Transform child = presentSlotPool.GetChild(i);
            if (child.gameObject.activeInHierarchy)
                return child.GetComponent<RectTransform>();
        }
        return null;
    }

    void HideAll()
    {
        StopAllCoroutines();
        tutorialHighlight?.Hide();
        if (fg != null) fg.SetActive(false);
        phase = MergeTutorialPhase.None;
    }

    void ShowPopup(GameObject popup)
    {
        if (popup == null) return;
        popup.SetActive(true);
        PanelTween tween = popup.GetComponent<PanelTween>();
        if (tween != null) tween.ShowWithScale();
    }
}