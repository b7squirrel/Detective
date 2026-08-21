using UnityEngine;
using UnityEngine.UI;
using TMPro; // 추가
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

    const string KEY_BADGE_SEEN = "InfiniteMode_BadgeSeen";

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

        // ⭐ 추가: 말풍선도 같은 조건으로 동기화
        if (infiniteModePopup != null)
        {
            infiniteModePopup.SetActive(shouldShowHint);
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