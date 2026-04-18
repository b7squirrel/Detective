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
    [SerializeField] RectTransform shopTabButton;   // Tab → 첫번째 Btn
    [SerializeField] RectTransform duckCardButton;  // Ducks Panel → Button
    [SerializeField] RectTransform itemCardButton;  // Items Panel → Button

    [Header("팝업")]
    [SerializeField] GameObject shopOpenPopup;      // "상점이 열렸어요!" 팝업
    [SerializeField] GameObject crystalGivenPopup;  // "보석 지급!" 팝업
    [SerializeField] TextMeshProUGUI crystalGivenText;

    [Header("보석 지급량")]
    [SerializeField] int crystalAmount = 1650; // Duck 1000 + Item 650

    [Header("스크롤")]
    [SerializeField] ScrollRect shopScrollRect;        // Panel Store → Scroll View
    [SerializeField] float scrollToDuckCardPosY = 2413f; // Content Y 목표값
    [SerializeField] float scrollDuration = 0.5f;

    [Header("클릭 차단")]
    [SerializeField] GameObject fg; // Canvas → FG 오브젝트 연결

    ChestType pendingChestType = ChestType.Other; // 어떤 가챠였는지 기억
    const string CRYSTAL_GIVEN_KEY = "TutorialCrystalGiven"; // 크리스탈 지급 여부 저장
    const string SHOP_PHASE_KEY = "TutorialShopPhase"; // 샵 튜토리얼 진행도 저장
    // ─────────────────────────────────────────
    // 내부 상태
    // ─────────────────────────────────────────
    enum ShopTutorialPhase
    {
        None,
        HighlightShopTab,   // Shop 탭 클릭 유도
        HighlightDuckCard,  // Duck Card 뽑기 유도
        HighlightItemCard,  // Item Card 뽑기 유도
        Done
    }
    ShopTutorialPhase phase = ShopTutorialPhase.None;

    // ─────────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────────
    void Awake()
    {
        instance = this;
    }

    // Start()에서 저장된 phase 복원
    void Start()
    {
        if (TutorialManager.instance?.CurrentStep == TutorialStep.Step1_ShopUnlocked)
        {
            phase = (ShopTutorialPhase)PlayerPrefs.GetInt(SHOP_PHASE_KEY, 0);
        }
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
        if (step == TutorialStep.Step1_ShopUnlocked)
            StartShopTutorial();
        else
            HideAll(); // 다른 단계로 넘어가면 오버레이 숨기기
    }

    // phase 변경 시마다 저장
    private void SetPhase(ShopTutorialPhase newPhase)
    {
        phase = newPhase;
        PlayerPrefs.SetInt(SHOP_PHASE_KEY, (int)phase);
        PlayerPrefs.Save();
    }

    // ─────────────────────────────────────────
    // 튜토리얼 흐름
    // ─────────────────────────────────────────

    // [1단계] 상점 팝업 → Shop 탭 하이라이트
    void StartShopTutorial()
    {
        if (fg != null) fg.SetActive(true); // ✅ 가장 먼저 차단
        ShowPopup(shopOpenPopup);
        StartCoroutine(HighlightAfterDelay(shopTabButton, ShopTutorialPhase.HighlightShopTab, 1.5f));
    }
    // [2단계] 팝업 대기 코루틴 — FG는 하이라이트 직전에 해제
    IEnumerator HighlightAfterDelay(RectTransform target, ShopTutorialPhase nextPhase, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetPhase(nextPhase);
        if (fg != null) fg.SetActive(false); // ✅ 하이라이트 직전에 해제
        tutorialHighlight.HighlightUI(target);
    }

    // [3단계] Shop 탭 클릭 → 보석 지급 → Duck Card 하이라이트
    // FG는 이미 켜져 있으므로 그대로 유지
    public void OnShopTabEntered()
    {
        if (phase != ShopTutorialPhase.HighlightShopTab) return;

        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true); // ✅ 혹시 꺼져 있을 경우 대비

        // ✅ 한 번만 지급
        if (PlayerPrefs.GetInt(CRYSTAL_GIVEN_KEY, 0) == 0)
        {
            PlayerDataManager.Instance.AddCristal(crystalAmount);
            if (crystalGivenText != null)
                crystalGivenText.text = $"보석 {crystalAmount}개를 지급했습니다!";
            ShowPopup(crystalGivenPopup);
            PlayerPrefs.SetInt(CRYSTAL_GIVEN_KEY, 1);
            PlayerPrefs.Save();
        }

        StartCoroutine(ScrollThenHighlight());
    }

    IEnumerator ScrollThenHighlight()
    {
        // 팝업 보여주는 동안 대기
        yield return new WaitForSeconds(1.5f);

        // 스크롤 애니메이션
        yield return StartCoroutine(ScrollToPosition(scrollToDuckCardPosY));

        // ✅ 하이라이트 준비 완료 후 FG 해제
        if (fg != null) fg.SetActive(false);

        // 스크롤 완료 후 Duck Card 하이라이트
        SetPhase(ShopTutorialPhase.HighlightDuckCard);
        tutorialHighlight.HighlightUI(duckCardButton);
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
            // EaseInOut 느낌
            t = t * t * (3f - 2f * t);

            float newY = Mathf.Lerp(startPosY, targetPosY, t);
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);

            yield return null;
        }

        // 정확한 최종값 설정
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetPosY);
    }

    // [4단계] Duck Card 뽑기 → Item Card 하이라이트
    // 가챠 화면 열릴 때 호출
    public void OnGachaOpened(ChestType chestType)
    {
        // 오버레이 즉시 숨기기
        tutorialHighlight.Hide();
        if (fg != null) fg.SetActive(true); // ✅ 가챠 열리는 동안 차단
        pendingChestType = chestType;
    }

    // 가챠 화면 닫힐 때 호출 (탭해서 계속하기 버튼에서 호출)
    public void OnGachaClosed()
    {
        if (fg != null) fg.SetActive(true); // ✅ 즉시 차단

        if (TutorialManager.instance?.CurrentStep != TutorialStep.Step1_ShopUnlocked) return;

        if (pendingChestType == ChestType.Duck && phase == ShopTutorialPhase.HighlightDuckCard)
        {
            StartCoroutine(ScrollThenHighlightItem());
        }
        else if (pendingChestType == ChestType.Item && phase == ShopTutorialPhase.HighlightItemCard)
        {
            SetPhase(ShopTutorialPhase.Done);
            HideAll();
            TutorialManager.instance.AdvanceStep();
        }

        pendingChestType = ChestType.Other;
    }
    IEnumerator ScrollThenHighlightItem()
    {
        // FG는 이미 켜져 있음

        yield return new WaitForSeconds(0.5f);
        if (fg != null) fg.SetActive(false);

        SetPhase(ShopTutorialPhase.HighlightItemCard);
        tutorialHighlight.HighlightUI(itemCardButton);
    }

    // ─────────────────────────────────────────
    // 유틸리티
    // ─────────────────────────────────────────


    void ShowPopup(GameObject popup)
    {
        if (popup == null) return;
        popup.SetActive(true);
        PanelTween tween = popup.GetComponent<PanelTween>();
        if (tween != null) tween.ShowWithScale();
    }

    void HideAll()
    {
        StopAllCoroutines();
        tutorialHighlight?.Hide();
        if (fg != null) fg.SetActive(false); // ✅ 추가
        phase = ShopTutorialPhase.None;
    }
}