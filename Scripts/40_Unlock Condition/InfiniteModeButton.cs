using UnityEngine;
using UnityEngine.UI;

public class InfiniteModeButton : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] Button button;
    [SerializeField] Image buttonImage;
    [SerializeField] Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] Color unlockedColor = new Color(1f, 0.4f, 0.6f, 1f); // 기존 핑크색

    [Header("배지 (빨간 점)")]
    [SerializeField] GameObject badge;

    [Header("잠금 안내 팝업")]
    [SerializeField] GameObject lockedPopup;

    [Header("무한모드 패널")]
    [SerializeField] InfiniteStagePanel infiniteStagePanel;

    const string KEY_BADGE_SEEN = "InfiniteMode_BadgeSeen";

    // ───────────────────────────────────────────

    void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        bool unlocked = UnlockConditionManager.Instance != null &&
                        UnlockConditionManager.Instance.IsInfiniteModeUnlocked();

        // 버튼 색상
        if (buttonImage != null)
            buttonImage.color = unlocked ? unlockedColor : lockedColor;

        // 배지: 해금됐고 아직 안 봤을 때만 표시
        if (badge != null)
        {
            bool badgeSeen = PlayerPrefs.GetInt(KEY_BADGE_SEEN, 0) == 1;
            badge.SetActive(unlocked && !badgeSeen);
        }
    }

    // ───────────────────────────────────────────
    // 버튼 클릭 (인스펙터에서 OnClick에 연결)
    // ───────────────────────────────────────────

    public void OnClick()
    {
        bool unlocked = UnlockConditionManager.Instance != null &&
                        UnlockConditionManager.Instance.IsInfiniteModeUnlocked();

        if (!unlocked)
        {
            ShowPopup(lockedPopup);
            return;
        }

        // 배지 제거
        HideBadge();

        // 무한모드 패널 열기
        infiniteStagePanel?.ActivateInfinitePanel();
    }

    // ───────────────────────────────────────────

    void HideBadge()
    {
        if (PlayerPrefs.GetInt(KEY_BADGE_SEEN, 0) == 1) return;

        PlayerPrefs.SetInt(KEY_BADGE_SEEN, 1);
        PlayerPrefs.Save();

        if (badge != null) badge.SetActive(false);
    }

    void ShowPopup(GameObject popup)
    {
        if (popup == null) return;
        popup.SetActive(true);
        PanelTween tween = popup.GetComponent<PanelTween>();
        if (tween != null) tween.ShowWithScale();
    }

    // ───────────────────────────────────────────
    // WinStage에서 해금 직후 호출 — 배지 초기화
    // ───────────────────────────────────────────

    public void OnInfiniteModeJustUnlocked()
    {
        PlayerPrefs.SetInt(KEY_BADGE_SEEN, 0);
        PlayerPrefs.Save();
        Refresh();
    }

    // ───────────────────────────────────────────
    //  디버그
    // ───────────────────────────────────────────

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