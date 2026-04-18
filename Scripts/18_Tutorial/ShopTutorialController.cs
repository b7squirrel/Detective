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

    // ─────────────────────────────────────────
    // 튜토리얼 흐름
    // ─────────────────────────────────────────

    // [1단계] 상점 팝업 → Shop 탭 하이라이트
    void StartShopTutorial()
    {
        ShowPopup(shopOpenPopup);
        StartCoroutine(HighlightAfterDelay(shopTabButton, ShopTutorialPhase.HighlightShopTab, 1.5f));
    }

    // [2단계] Shop 탭 클릭 → 보석 지급 → Duck Card 하이라이트
    public void OnShopTabEntered()
    {
        if (phase != ShopTutorialPhase.HighlightShopTab) return;

        tutorialHighlight.Hide();

        // 보석 자동 지급
        PlayerDataManager.Instance.AddCristal(crystalAmount);
        if (crystalGivenText != null)
            crystalGivenText.text = $"보석 {crystalAmount}개를 지급했습니다!";
        ShowPopup(crystalGivenPopup);

        // ✅ 스크롤 후 하이라이트
        StartCoroutine(ScrollThenHighlight());
    }

    IEnumerator ScrollThenHighlight()
    {
        // 팝업 보여주는 동안 대기
        yield return new WaitForSeconds(1.5f);

        // 스크롤 애니메이션
        yield return StartCoroutine(ScrollToPosition(scrollToDuckCardPosY));

        // 스크롤 완료 후 Duck Card 하이라이트
        phase = ShopTutorialPhase.HighlightDuckCard;
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

    // [3단계] Duck Card 뽑기 → Item Card 하이라이트
    public void OnDuckCardPurchased()
    {
        if (phase != ShopTutorialPhase.HighlightDuckCard) return;
        StartCoroutine(HighlightAfterDelay(itemCardButton, ShopTutorialPhase.HighlightItemCard, 0.5f));
    }

    // [4단계] Item Card 뽑기 → 완료 → Step2 진행
    public void OnItemCardPurchased()
    {
        if (phase != ShopTutorialPhase.HighlightItemCard) return;

        phase = ShopTutorialPhase.Done;
        HideAll();

        // Step2_GearUnlocked 로 진행
        TutorialManager.instance.AdvanceStep();
    }

    // ─────────────────────────────────────────
    // 유틸리티
    // ─────────────────────────────────────────
    IEnumerator HighlightAfterDelay(RectTransform target, ShopTutorialPhase nextPhase, float delay)
    {
        yield return new WaitForSeconds(delay);
        phase = nextPhase;
        tutorialHighlight.HighlightUI(target);
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
        StopAllCoroutines();
        tutorialHighlight?.Hide();
        phase = ShopTutorialPhase.None;
    }
}