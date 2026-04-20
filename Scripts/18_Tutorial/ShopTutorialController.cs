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
            HideAll();
            return;
        }

        // ✅ Done 케이스 제거 - TutorialManager에서 이미 교정됨
        if (phase == ShopTutorialPhase.None)
        {
            StartShopTutorial();
            return;
        }

        if (fg != null) fg.SetActive(false);
        tutorialHighlight.HighlightUI(shopTabButton);
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
        if (fg != null) fg.SetActive(false);
        tutorialHighlight.HighlightUI(shopTabButton);
    }

    // ─────────────────────────────────────────
    // [2단계] Shop 탭 진입 → phase에 따라 라우팅
    // ShopPanel의 OnEnable 또는 탭 버튼에서 호출
    // ─────────────────────────────────────────
    public void OnShopTabEntered()
    {
        Debug.Log($"[ShopTutorial] OnShopTabEntered - phase: {phase}");

        tutorialHighlight.Hide();
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
                // 오리카드 이미 뽑음 → 아이템카드 하이라이트
                StartCoroutine(ScrollThenHighlightItem());
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
        if (fg != null) fg.SetActive(false);
        tutorialHighlight.HighlightUI(duckCardButton);
    }

    IEnumerator ScrollThenHighlightItem()
    {
        yield return new WaitForSeconds(1.0f);
        yield return StartCoroutine(ScrollToPosition(scrollToDuckCardPosY));
        // phase는 이미 HighlightItemCard로 저장되어 있음 (OnGachaOpened에서)
        if (fg != null) fg.SetActive(false);
        tutorialHighlight.HighlightUI(itemCardButton);
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
        if (fg != null) fg.SetActive(true);
        pendingChestType = chestType;

        // 가챠가 열리는 순간 = 뽑기 완료 → 즉시 다음 phase 저장
        if (chestType == ChestType.Duck && phase == ShopTutorialPhase.HighlightDuckCard)
        {
            SetPhase(ShopTutorialPhase.HighlightItemCard);
        }
        else if (chestType == ChestType.Item && phase == ShopTutorialPhase.HighlightItemCard)
        {
            SetPhase(ShopTutorialPhase.Done);
        }

        Debug.Log($"[ShopTutorial] OnGachaOpened - chestType: {chestType}, phase: {phase}");
    }

    // ─────────────────────────────────────────
    // [4단계] 가챠 화면 닫힐 때
    // ─────────────────────────────────────────
    public void OnGachaClosed()
    {
        if (fg != null) fg.SetActive(true);
        if (TutorialManager.instance?.CurrentStep != TutorialStep.Step1_ShopUnlocked) return;

        Debug.Log($"[ShopTutorial] OnGachaClosed - pendingChestType: {pendingChestType}, phase: {phase}");

        // OnGachaOpened에서 이미 phase가 변경됨
        if (pendingChestType == ChestType.Duck && phase == ShopTutorialPhase.HighlightItemCard)
        {
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
    // 유틸리티
    // ─────────────────────────────────────────
    void GiveCrystalIfNeeded()
    {
        if (PlayerPrefs.GetInt(CRYSTAL_GIVEN_KEY, 0) == 1) return;

        PlayerDataManager.Instance.AddCristal(crystalAmount);
        if (crystalGivenText != null)
            crystalGivenText.text = $"보석 {crystalAmount}개를 지급했습니다!";
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
        StopAllCoroutines();
        tutorialHighlight?.Hide();
        if (fg != null) fg.SetActive(false);
        phase = ShopTutorialPhase.None;
    }
}