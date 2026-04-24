using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GearTutorialController : MonoBehaviour
{
    public static GearTutorialController instance;

    [Header("오버레이")]
    [SerializeField] TutorialHighlight tutorialHighlight;

    [Header("하이라이트 대상 - 정적")]
    [SerializeField] RectTransform gearTabButton;  // 하단 탭의 Gear(장비) 버튼
    [SerializeField] RectTransform equipButton;    // EquipInfoPanel 안의 장착 버튼

    [Header("슬롯 풀")]
    [SerializeField] Transform presentSlotPool;    // Slot Containers > Present Field > Viewport > Present Slot Pool

    [Header("클릭 차단")]
    [SerializeField] GameObject fg;

    [Header("팝업")]
    [SerializeField] GameObject gearOpenPopup;  // "장비 탭이 열렸습니다!" 팝업

    [Header("런치 화면")]
    [SerializeField] GameObject panelLaunch;
    [Header("상점 스크롤 잠금")]
    // Gear 튜토리얼 중 상점 스크롤이 되지 않도록
    [SerializeField] ScrollRect shopScrollRect;

    // ─────────────────────────────────────────
    // 내부 상태
    // ─────────────────────────────────────────
    enum GearTutorialPhase
    {
        None,
        HighlightGearTab,      // Gear 탭 클릭 유도
        HighlightDuckCard,     // 오리 카드 클릭 유도
        HighlightItemCard,     // 아이템 카드 클릭 유도
        HighlightEquipButton,  // 장착 버튼 클릭 유도
        Done
    }
    GearTutorialPhase phase = GearTutorialPhase.None;

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
        if (step == TutorialStep.Step2_GearUnlocked)
            StartGearTutorial();
        else
            HideAll();
    }

    // ─────────────────────────────────────────
    // 튜토리얼 흐름
    // ─────────────────────────────────────────

    // [1단계] Step2 진입 → Gear 탭 하이라이트
    void StartGearTutorial()
    {
        StartCoroutine(WaitForMainMenuThenStart());
    }

    // Gear 탭 버튼이 실제로 활성화될 때까지 대기
    IEnumerator WaitForMainMenuThenStart()
    {
        yield return new WaitUntil(() => GameInitializer.IsInitialized);
        yield return new WaitForSeconds(0.5f);

        // ✅ 추가: Gear 튜토리얼 시작 시 상점 스크롤 잠금
        if (shopScrollRect != null) shopScrollRect.enabled = false;

        if (fg != null) fg.SetActive(true);
        ShowPopup(gearOpenPopup);
        StartCoroutine(HighlightAfterDelay(gearTabButton, GearTutorialPhase.HighlightGearTab, 1.5f));
    }

    // [2단계] Gear 탭 클릭 → 오리 카드 하이라이트
    // EquipmentPanelManager.OnEnable()에서 호출
    public void OnGearPanelEntered()
    {
        if (phase != GearTutorialPhase.HighlightGearTab) return;

        // ✅ 추가: Gear 패널 진입 시 상점 스크롤 복구 (더 이상 상점이 보이지 않으므로)
        if (shopScrollRect != null) shopScrollRect.enabled = true;

        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true);
        StartCoroutine(HighlightFirstSlotAfterDelay(GearTutorialPhase.HighlightDuckCard, 0.3f));
    }

    // [3단계] 오리 카드 클릭 → 아이템 카드 하이라이트
    // EquipmentPanelManager.InitDisplay()에서 호출
    public void OnDuckSelected()
    {
        if (phase != GearTutorialPhase.HighlightDuckCard) return;

        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true);

        // 필드가 아이템으로 전환되고 슬롯이 생성될 때까지 대기
        StartCoroutine(HighlightFirstSlotAfterDelay(GearTutorialPhase.HighlightItemCard, 0.5f));
    }

    // [4단계] 아이템 카드 클릭 → 장착 버튼 하이라이트
    // EquipmentPanelManager.ActivateEquipInfoPanel()에서 호출
    public void OnItemSelected()
    {
        if (phase != GearTutorialPhase.HighlightItemCard) return;

        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true);

        StartCoroutine(HighlightAfterDelay(equipButton, GearTutorialPhase.HighlightEquipButton, 0.3f));
    }

    // [5단계] 장착 버튼 클릭
    // → EquipmentPanelManager.OnEquipButton()에 이미 AdvanceStep() 구현됨
    // → AdvanceStep() → OnStepChanged(Step3) → HideAll() 자동 호출

    // ─────────────────────────────────────────
    // 공통 유틸리티
    // ─────────────────────────────────────────

    // 지연 후 고정 타겟 하이라이트 (Gear 탭, 장착 버튼 등)
    IEnumerator HighlightAfterDelay(RectTransform target, GearTutorialPhase nextPhase, float delay)
    {
        yield return new WaitForSeconds(delay);
        phase = nextPhase;
        // ✅ 수정
        tutorialHighlight.HighlightUI(target, fg);
    }

    // 지연 후 Present Slot Pool의 첫 번째 활성 슬롯 하이라이트 (오리 카드, 아이템 카드)
    IEnumerator HighlightFirstSlotAfterDelay(GearTutorialPhase nextPhase, float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return null;

        RectTransform firstSlot = GetFirstActiveSlot();
        if (firstSlot == null)
        {
            Debug.LogWarning("[GearTutorial] 첫 번째 슬롯을 찾을 수 없습니다.");
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

        // ✅ 추가: 튜토리얼 종료 시 상점 스크롤 복구
        if (shopScrollRect != null) shopScrollRect.enabled = true;

        phase = GearTutorialPhase.None;
    }

    void ShowPopup(GameObject popup)
    {
        if (popup == null) return;
        popup.SetActive(true);
        PanelTween tween = popup.GetComponent<PanelTween>();
        if (tween != null) tween.ShowWithScale();
    }
}