using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfiniteModeButton : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] Button button;
    [SerializeField] Image buttonImage;
    [SerializeField] Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] Color unlockedColor = new Color(1f, 0.4f, 0.6f, 1f);

    [Header("라벨 텍스트 (Duck Challenge!!)")]
    [SerializeField] TMP_Text labelText;
    [SerializeField] Color labelLockedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] Color labelUnlockedColor = Color.white;

    [Header("배지 (빨간 점)")]
    [SerializeField] GameObject badge;

    [Header("잠금 안내 팝업")]
    [SerializeField] GameObject lockedPopup;

    [Header("무한모드 패널")]
    [SerializeField] InfiniteStagePanel infiniteStagePanel;

    [Header("튜토리얼 하이라이트")]
    [SerializeField] TutorialHighlight tutorialHighlight;
    [SerializeField] RectTransform highlightTarget; // 비워두면 이 버튼 자신의 RectTransform 사용

    // ⭐ 추가: 말풍선/설명 팝업 (Overlay Popup > Infinite Mode Popup)
    [Header("말풍선 안내")]
    [SerializeField] GameObject infiniteModePopup;
    [SerializeField] Vector2 infiniteModePopupOffset = Vector2.zero; // ⭐ 추가: 필요시 인스펙터에서 미세 조정

    const string KEY_BADGE_SEEN = "InfiniteMode_BadgeSeen";

    // ⭐ 추가: 팝업 위치 계산용 캐시
    RectTransform _popupRect;
    RectTransform _popupParent;
    Canvas _popupCanvas;

    void Awake()
    {
        // ⭐ 추가: infiniteModePopup이 다른 부모 아래에 있으므로 위치 계산용 캐시를 미리 준비
        if (infiniteModePopup != null)
        {
            _popupRect = infiniteModePopup.GetComponent<RectTransform>();
            _popupParent = _popupRect.parent as RectTransform;
            _popupCanvas = infiniteModePopup.GetComponentInParent<Canvas>(true); // includeInactive
        }
    }

    void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        bool unlocked = UnlockConditionManager.Instance != null &&
                        UnlockConditionManager.Instance.IsInfiniteModeUnlocked();

        if (buttonImage != null)
            buttonImage.color = unlocked ? unlockedColor : lockedColor;

        if (labelText != null)
            labelText.color = unlocked ? labelUnlockedColor : labelLockedColor;

        bool shouldShowHint = false;
        if (badge != null)
        {
            bool badgeSeen = PlayerPrefs.GetInt(KEY_BADGE_SEEN, 0) == 1;
            shouldShowHint = unlocked && !badgeSeen;
            badge.SetActive(shouldShowHint);
        }

        if (tutorialHighlight != null)
        {
            if (shouldShowHint)
            {
                RectTransform target = highlightTarget != null ? highlightTarget : (RectTransform)transform;
                tutorialHighlight.HighlightUI(target);
            }
            else
            {
                tutorialHighlight.Hide();
            }
        }

        // ⭐ 변경: 말풍선도 같은 조건으로 동기화 + buttonImage 위치에 맞춤
        if (infiniteModePopup != null)
        {
            if (shouldShowHint)
            {
                PositionPopupAt(buttonImage != null ? buttonImage.rectTransform : null);
                infiniteModePopup.SetActive(true);
            }
            else
            {
                infiniteModePopup.SetActive(false);
            }
        }
    }

    // ⭐ 추가: infiniteModePopup을 target(buttonImage) 위치에 맞춤
    void PositionPopupAt(RectTransform target)
    {
        if (target == null || _popupRect == null || _popupParent == null || _popupCanvas == null) return;

        Camera cam = (_popupCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : _popupCanvas.worldCamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, target.position);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_popupParent, screenPoint, cam, out Vector2 localPoint))
        {
            _popupRect.anchoredPosition = localPoint + infiniteModePopupOffset;
        }
    }

    public void OnClick()
    {
        bool unlocked = UnlockConditionManager.Instance != null &&
                        UnlockConditionManager.Instance.IsInfiniteModeUnlocked();

        if (!unlocked)
        {
            ShowPopup(lockedPopup);
            return;
        }

        HideBadge();
        infiniteStagePanel?.ActivateInfinitePanel();
    }

    void HideBadge()
    {
        if (PlayerPrefs.GetInt(KEY_BADGE_SEEN, 0) == 1) return;

        PlayerPrefs.SetInt(KEY_BADGE_SEEN, 1);
        PlayerPrefs.Save();

        if (badge != null) badge.SetActive(false);
        if (tutorialHighlight != null) tutorialHighlight.Hide();
        // ⭐ 추가: 클릭 시 말풍선도 같이 해제
        if (infiniteModePopup != null) infiniteModePopup.SetActive(false);
    }

    void ShowPopup(GameObject popup)
    {
        if (popup == null) return;
        popup.SetActive(true);
        PanelTween tween = popup.GetComponent<PanelTween>();
        if (tween != null) tween.ShowWithScale();
    }

    public void OnInfiniteModeJustUnlocked()
    {
        PlayerPrefs.SetInt(KEY_BADGE_SEEN, 0);
        PlayerPrefs.Save();
        Refresh();
    }

#if UNITY_EDITOR
    [ContextMenu("Debug/배지 초기화 (미확인 상태로)")]
    void DebugResetBadge()
    {
        PlayerPrefs.SetInt(KEY_BADGE_SEEN, 0);
        PlayerPrefs.Save();
        Refresh();
    }
#endif
}