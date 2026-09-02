using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopTutorialController : MonoBehaviour
{
    public static ShopTutorialController instance;

    [Header("오버레이")]
    [SerializeField] TutorialHighlight tutorialHighlight;

    [Header("하이라이트 대상")]
    [SerializeField] RectTransform shopTabButton;
    [SerializeField] RectTransform duckCardButton;
    [SerializeField] RectTransform itemCardButton;

    [Header("팝업")]
    [SerializeField] GameObject shopOpenPopup;
    [SerializeField] GameObject crystalGivenPopup;
    [SerializeField] TextMeshProUGUI crystalGivenText;
    [SerializeField] TextMeshProUGUI crystalGivenTextShd;

    [Header("최고레벨 카드 안내 말풍선")]
    [SerializeField] GameObject topCardHintBubble;
    [SerializeField] Vector2 hintOffset = Vector2.zero; // 필요시 인스펙터에서 미세 조정

    Canvas _canvas;
    RectTransform _hintRect;
    RectTransform _hintParent;

    [Header("보석 지급량")]
    [SerializeField] int crystalAmount = 1650;

    [Header("스크롤")]
    [SerializeField] ScrollRect shopScrollRect;
    [SerializeField] float scrollToDuckCardPosY = 2413f;
    [SerializeField] float scrollDuration = 0.5f;

    [Header("클릭 차단")]
    [SerializeField] GameObject fg;

    // ─────────────────────────────────────────
    // 내부 상태
    // ─────────────────────────────────────────
    enum ShopTutorialPhase
    {
        None,
        HighlightShopTab,   // 상점 탭 클릭 유도
        HighlightDuckCard,  // 오리카드 뽑기 유도
        HighlightItemCard,  // 아이템카드 뽑기 유도
        Done                // 상점 튜토리얼 완료
    }

    ShopTutorialPhase phase = ShopTutorialPhase.None;
    ChestType pendingChestType = ChestType.Other;

    const string CRYSTAL_GIVEN_KEY = "TutorialCrystalGiven";
    const string SHOP_PHASE_KEY = "TutorialShopPhase";

    Coroutine _activeScrollCoroutine = null;

    // ─────────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────────
    void Awake()
    {
        instance = this;
        int savedStep = PlayerPrefs.GetInt("TutorialStep", 0);
        if (savedStep == (int)TutorialStep.Step1_ShopUnlocked)
        {
            phase = (ShopTutorialPhase)PlayerPrefs.GetInt(SHOP_PHASE_KEY, 0);
        }
        Debug.Log($"[ShopTutorial] Awake - phase 복원: {phase}");

        _hintRect = topCardHintBubble.GetComponent<RectTransform>();
        _hintParent = _hintRect.parent as RectTransform;
        _canvas = topCardHintBubble.GetComponentInParent<Canvas>(true); // includeInactive: true
    }

    void OnEnable()
    {
        TutorialManager.OnStepChanged += OnStepChanged;
    }

    void OnDisable()
    {
        TutorialManager.OnStepChanged -= OnStepChanged;
    }

    // ─────────────────────────────────────────
    // Step 변경 수신
    // ─────────────────────────────────────────
    void OnStepChanged(TutorialStep step)
    {
        if (step != TutorialStep.Step1_ShopUnlocked)
        {
            // ✅ None이거나 이미 Done(정상 종료)이면 다른 컨트롤러의 fg를 건드리지 않음
            if (phase != ShopTutorialPhase.None && phase != ShopTutorialPhase.Done)
                HideAll();
            return;
        }

        // Done 케이스 제거 - TutorialManager에서 이미 교정됨
        if (phase == ShopTutorialPhase.None)
        {
            StartShopTutorial();
            return;
        }

        // ✅ fg 넘겨서 동시 처리
        tutorialHighlight.HighlightUI(shopTabButton, fg);
    }

    // ─────────────────────────────────────────
    // [1단계] 최초 시작: 팝업 → Shop 탭 하이라이트
    // ─────────────────────────────────────────
    void StartShopTutorial()
    {
        if (fg != null) fg.SetActive(true);
        ShowPopup(shopOpenPopup);
        StartCoroutine(HighlightShopTabAfterDelay(1.5f));
    }

    IEnumerator HighlightShopTabAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetPhase(ShopTutorialPhase.HighlightShopTab);
        // ✅ fg 넘겨서 동시 처리
        tutorialHighlight.HighlightUI(shopTabButton, fg);
    }

    // ─────────────────────────────────────────
    // [2단계] Shop 탭 진입 → phase에 따라 라우팅
    // ShopPanel의 OnEnable 또는 탭 버튼에서 호출
    // ─────────────────────────────────────────
    public void OnShopTabEntered()
    {
        Debug.Log($"[ShopTutorial] OnShopTabEntered - phase: {phase}");
        tutorialHighlight.Hide();
        HideTopCardHint();
        if (fg != null) fg.SetActive(true);

        switch (phase)
        {
            case ShopTutorialPhase.HighlightShopTab:
                // 처음 진입: 보석 지급 후 오리카드 하이라이트
                GiveCrystalIfNeeded();
                StartCoroutine(ScrollThenHighlightDuck());
                break;

            case ShopTutorialPhase.HighlightDuckCard:
                // 오리카드 아직 안 뽑음 → 오리카드 하이라이트
                StartCoroutine(ScrollThenHighlightDuck());
                break;

            case ShopTutorialPhase.HighlightItemCard:
                if (_activeScrollCoroutine == null) // 이미 진행 중이면 중복 시작 안 함
                    StartCoroutine(ScrollThenHighlightItem());
                else
                    Debug.Log("[ShopTutorial] OnShopTabEntered: 이미 하이라이트 진행 중");
                break;

            case ShopTutorialPhase.Done:
                // 안전장치: Done이면 바로 다음 단계
                HideAll();
                TutorialManager.instance.AdvanceStep();
                break;

            default:
                if (fg != null) fg.SetActive(false);
                break;
        }
    }

    // ─────────────────────────────────────────
    // 스크롤 + 하이라이트 코루틴
    // ─────────────────────────────────────────
    IEnumerator ScrollThenHighlightDuck()
    {
        yield return new WaitForSeconds(1.0f);
        yield return StartCoroutine(ScrollToPosition(scrollToDuckCardPosY));
        SetPhase(ShopTutorialPhase.HighlightDuckCard);
        // ✅ 하이라이트 표시 후 스크롤 잠금
        LockScroll();
        tutorialHighlight.HighlightUI(duckCardButton, fg);
        ShowTopCardHint(); // ⭐ 오리카드 하이라이트와 함께 표시
    }

    IEnumerator ScrollThenHighlightItem()
    {
        // 중복 실행 방지: 이미 진행 중이면 중단
        if (_activeScrollCoroutine != null)
        {
            Debug.Log("[ShopTutorial] ScrollThenHighlightItem 이미 진행 중 - 스킵");
            yield break;
        }
        _activeScrollCoroutine = StartCoroutine(ScrollThenHighlightItemInternal());
        yield return _activeScrollCoroutine;
        _activeScrollCoroutine = null;
    }

    IEnumerator ScrollThenHighlightItemInternal()
    {
        if (_activeScrollCoroutine != null)
        {
            Debug.Log("[ShopTutorial] ScrollThenHighlightItem 이미 진행 중 - 스킵");
            yield break;
        }

        // ✅ 대기 중에도 fg가 켜져 있는지 매 프레임 보장
        float elapsed = 0f;
        while (elapsed < 1.0f)
        {
            if (fg != null && !fg.activeSelf) fg.SetActive(true); // 혹시 꺼졌으면 다시 켬
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (TutorialManager.instance?.CurrentStep != TutorialStep.Step1_ShopUnlocked)
        {
            Debug.LogWarning("[ShopTutorial] 대기 중 Step이 변경됨! 아이템카드 하이라이트 취소");
            yield break;
        }

        yield return StartCoroutine(ScrollToPosition(scrollToDuckCardPosY));
        LockScroll();
        tutorialHighlight.HighlightUI(itemCardButton, fg);
        Debug.Log("[ShopTutorial] ✅ 아이템카드 하이라이트 표시 완료");
    }

    IEnumerator ScrollToPosition(float targetPosY)
    {
        RectTransform content = shopScrollRect.content;
        float startPosY = content.anchoredPosition.y;
        float elapsed = 0f;

        while (elapsed < scrollDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / scrollDuration);
            t = t * t * (3f - 2f * t);

            content.anchoredPosition = new Vector2(
                content.anchoredPosition.x,
                Mathf.Lerp(startPosY, targetPosY, t)
            );
            yield return null;
        }

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetPosY);
    }

    // ─────────────────────────────────────────
    // [3단계] 가챠 화면 열릴 때 → 즉시 phase 저장
    // ─────────────────────────────────────────
    public void OnGachaOpened(ChestType chestType)
    {
        tutorialHighlight.Hide();
        HideTopCardHint();
        if (fg != null) fg.SetActive(true);

        // ✅ 가챠 화면 열릴 때 스크롤 복구
        UnlockScroll();

        pendingChestType = chestType;

        // 가챠가 열리는 순간 = 뽑기 완료 → 즉시 다음 phase 저장
        if (chestType == ChestType.Duck && phase == ShopTutorialPhase.HighlightDuckCard)
            SetPhase(ShopTutorialPhase.HighlightItemCard);
        else if (chestType == ChestType.Item && phase == ShopTutorialPhase.HighlightItemCard)
            SetPhase(ShopTutorialPhase.Done);

        Logger.Log($"[ShopTutorial] OnGachaOpened - chestType: {chestType}, phase: {phase}");
    }

    // ─────────────────────────────────────────
    // [4단계] 가챠 화면 닫힐 때
    // ─────────────────────────────────────────
    public void OnGachaClosed()
    {
        // ✅ 가장 먼저 fg 활성화 (ResetState보다 늦게 호출돼도 즉시 막음)
        if (fg != null) fg.SetActive(true);

        if (TutorialManager.instance?.CurrentStep != TutorialStep.Step1_ShopUnlocked) return;

        Logger.Log($"[ShopTutorial] OnGachaClosed - pendingChestType: {pendingChestType}, phase: {phase}");

        if (pendingChestType == ChestType.Duck && phase == ShopTutorialPhase.HighlightItemCard)
        {
            LockScroll();
            StartCoroutine(ScrollThenHighlightItem());
        }
        else if (pendingChestType == ChestType.Item && phase == ShopTutorialPhase.Done)
        {
            HideAll();
            TutorialManager.instance.AdvanceStep();
        }

        pendingChestType = ChestType.Other;
    }

    // ─────────────────────────────────────────
    // 스크롤 제어
    // ─────────────────────────────────────────
    void LockScroll()
    {
        if (shopScrollRect != null) shopScrollRect.enabled = false;
    }

    void UnlockScroll()
    {
        if (shopScrollRect != null) shopScrollRect.enabled = true;
    }

    // ─────────────────────────────────────────
    // 유틸리티
    // ─────────────────────────────────────────
    void GiveCrystalIfNeeded()
    {
        if (PlayerPrefs.GetInt(CRYSTAL_GIVEN_KEY, 0) == 1) return;

        PlayerDataManager.Instance.AddCristal(crystalAmount);

        if (crystalGivenText != null)
        {
            string coloredAmount = $"<color=#FFE600>{crystalAmount}</color>";
            crystalGivenText.text = string.Format(LocalizationManager.Game.crystalGiven, coloredAmount);
        }
        if (crystalGivenTextShd != null)
        {
            string coloredAmount = $"<color=#000000>{crystalAmount}</color>";
            crystalGivenTextShd.text = string.Format(LocalizationManager.Game.crystalGiven, coloredAmount);
        }

        ShowPopup(crystalGivenPopup);

        PlayerPrefs.SetInt(CRYSTAL_GIVEN_KEY, 1);
        PlayerPrefs.Save();

        Debug.Log($"[ShopTutorial] 보석 {crystalAmount}개 지급");
    }

    void SetPhase(ShopTutorialPhase newPhase)
    {
        phase = newPhase;
        PlayerPrefs.SetInt(SHOP_PHASE_KEY, (int)phase);
        PlayerPrefs.Save();
        Debug.Log($"[ShopTutorial] SetPhase: {phase}");
    }

    void ShowPopup(GameObject popup)
    {
        if (popup == null) return;
        popup.SetActive(true);
        PanelTween tween = popup.GetComponent<PanelTween>();
        if (tween != null) tween.ShowWithScale();
    }

    void HideAll()
    {
        _activeScrollCoroutine = null; // 레퍼런스 초기화
        StopAllCoroutines();
        tutorialHighlight?.Hide();
        HideTopCardHint();
        if (fg != null) fg.SetActive(false);
        UnlockScroll();
        phase = ShopTutorialPhase.None;
        Debug.Log("[ShopTutorial] HideAll 호출됨 - 호출 스택 확인 필요");
    }

    // ─────────────────────────────────────────
    // 최고레벨 카드 안내 말풍선
    // ─────────────────────────────────────────
    void ShowTopCardHint()
    {
        if (topCardHintBubble == null) return;
        StartCoroutine(ShowTopCardHintCo());
    }

    IEnumerator ShowTopCardHintCo()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        PositionHintBubbleAt(duckCardButton);
        topCardHintBubble.SetActive(true);
    }

    // shop open popup의 close button에서 호출
    public void TriggerTopCardHintBubbleAnim()
    {
        topCardHintBubble.GetComponentInChildren<Animator>().SetTrigger("Init");
        
    }

    void PositionHintBubbleAt(RectTransform target)
    {
        if (target == null || _hintRect == null || _hintParent == null || _canvas == null) return;

        // Screen Space - Overlay면 카메라는 null, Camera 모드면 canvas.worldCamera 사용
        Camera cam = (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : _canvas.worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, target.position);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_hintParent, screenPoint, cam, out Vector2 localPoint))
        {
            _hintRect.anchoredPosition = localPoint + hintOffset;
        }
    }

    void HideTopCardHint()
    {
        if (topCardHintBubble == null || !topCardHintBubble.activeSelf) return;
        topCardHintBubble.SetActive(false);
    }
}