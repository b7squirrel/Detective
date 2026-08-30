using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class MergeTutorialController : MonoBehaviour
{
    public static MergeTutorialController instance;
    [Header("오버레이")]
    [SerializeField] TutorialHighlight tutorialHighlight;
    [Header("하이라이트 대상 - 정적")]
    [SerializeField] RectTransform mergeTabButton;   // 하단 탭의 Merge(합성) 버튼
    [SerializeField] RectTransform confirmButton;    // 합성 확인 버튼 (Buttons 컨테이너 안)
    [Header("슬롯 풀 - AllField/MatField 공용")]
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
    void StartMergeTutorial()
    {
        StartCoroutine(WaitThenStart());
    }
    IEnumerator WaitThenStart()
    {
        if (fg != null) fg.SetActive(true);
        
        yield return new WaitUntil(() => GameInitializer.IsInitialized);
        yield return new WaitForSeconds(0.5f); // UI 렌더링 여유
        ShowPopup(mergeOpenPopup);
        StartCoroutine(HighlightAfterDelay(mergeTabButton, MergeTutorialPhase.HighlightMergeTab, 1.5f));
    }
    public void OnMergePanelEntered()
    {
        if (phase != MergeTutorialPhase.HighlightMergeTab) return;
        StopAllCoroutines();
        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true);
        StartCoroutine(HighlightFirstSlotAfterDelay(MergeTutorialPhase.HighlightUpCard, 0.5f));
    }
    public void OnUpCardSelected()
    {
        if (phase != MergeTutorialPhase.HighlightUpCard) return;
        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true);
        StartCoroutine(HighlightFirstSlotAfterDelay(MergeTutorialPhase.HighlightMatCard, 0.6f));
    }
    public void OnMatCardSelected()
    {
        if (phase != MergeTutorialPhase.HighlightMatCard) return;
        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true);
        StartCoroutine(HighlightAfterDelay(confirmButton, MergeTutorialPhase.HighlightConfirmButton, 0.4f));
    }
    // ─────────────────────────────────────────
    // 공통 유틸리티
    // ─────────────────────────────────────────
    IEnumerator HighlightAfterDelay(RectTransform target, MergeTutorialPhase nextPhase, float delay)
    {
        yield return new WaitForSeconds(delay);
        phase = nextPhase;
        tutorialHighlight.HighlightUI(target, fg);
    }
    IEnumerator HighlightFirstSlotAfterDelay(MergeTutorialPhase nextPhase, float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return null;
        RectTransform firstSlot = GetFirstActiveSlot();
        if (firstSlot == null)
        {
            Debug.LogWarning("[MergeTutorial] 첫 번째 슬롯을 찾을 수 없습니다. 튜토리얼을 강제 완료 처리합니다.");
            ForceCompleteTutorial();
            yield break;
        }
        phase = nextPhase;
        tutorialHighlight.HighlightUI(firstSlot, fg);
    }
    RectTransform GetFirstActiveSlot()
    {
        if (presentSlotPool == null) return null;
        for (int i = 0; i < presentSlotPool.childCount; i++)
        {
            Transform child = presentSlotPool.GetChild(i);
            if (!child.gameObject.activeInHierarchy) continue;
             Transform overlayRef = child.Find("Overlay Ref");
            if (overlayRef != null)
                return overlayRef.GetComponent<RectTransform>();
 
            Debug.LogWarning($"[GearTutorial] '{child.name}' 슬롯 안에서 'Overlay Ref'를 찾을 수 없습니다. Button으로 대체합니다.");
            Button slotButton = child.GetComponentInChildren<Button>(true);
            if (slotButton == null)
            {
                Debug.LogWarning($"[GearTutorial] '{child.name}' 슬롯 안에서 Button 컴포넌트도 찾을 수 없습니다. 슬롯 루트로 대체합니다.");
                return child.GetComponent<RectTransform>(); // 최종 폴백: 슬롯 루트
            }
            return slotButton.GetComponent<RectTransform>();
        }
        return null;
    }
    // ⭐ 추가: 합성 가능한 카드를 찾지 못해 튜토리얼을 진행시킬 수 없을 때
    // 보상 지급 없이 튜토리얼 전체를 강제 완료 처리
    void ForceCompleteTutorial()
    {
        HideAll();
        TutorialManager.instance?.SetStep(TutorialStep.Completed);
        FindObjectOfType<GachaSystem>()?.SkipTutorialRewardGift();
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