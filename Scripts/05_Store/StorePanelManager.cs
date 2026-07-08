using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum ShopSectionType { None, Energy, Gold, Cristal }

public class StorePanelManager : MonoBehaviour
{
    [SerializeField] CardSlotManager cardSlotManager;
    [SerializeField] ScrollRect scrollRect;

    [Header("스크롤 대상 섹션")]
    [SerializeField] RectTransform energySectionTarget;
    [SerializeField] RectTransform goldSectionTarget;
    [SerializeField] RectTransform cristalSectionTarget;

    // ⭐ 다음 활성화 시 스크롤할 대상 (외부에서 예약)
    private static ShopSectionType pendingSection = ShopSectionType.None;

    void OnEnable()
    {
        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Off");
        if (scrollRect == null) scrollRect = GetComponentInChildren<ScrollRect>();

        Logger.Log($"[StorePanelManager] OnEnable, pendingSection = {pendingSection}"); // ← 추가

        if (pendingSection != ShopSectionType.None)
        {
            RectTransform target = GetTargetBySection(pendingSection);
            Logger.Log($"[StorePanelManager] target = {(target != null ? target.name : "NULL")}"); // ← 추가

            pendingSection = ShopSectionType.None;

            if (target != null)
            {
                StartCoroutine(ScrollToTargetCo(target));
                return;
            }
        }

        scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 외부(상단 UI + 버튼 등)에서 호출 — 다음에 상점이 열릴 때 해당 섹션으로 스크롤
    /// </summary>
    public static void RequestScrollTo(ShopSectionType section)
    {
        pendingSection = section;
    }

    RectTransform GetTargetBySection(ShopSectionType section)
    {
        switch (section)
        {
            case ShopSectionType.Energy: return energySectionTarget;
            case ShopSectionType.Gold: return goldSectionTarget;
            case ShopSectionType.Cristal: return cristalSectionTarget;
            default: return null;
        }
    }

    IEnumerator ScrollToTargetCo(RectTransform target)
{
    yield return null;
    Canvas.ForceUpdateCanvases();

    RectTransform content = scrollRect.content;
    RectTransform viewport = scrollRect.viewport;

    float contentHeight = content.rect.height;
    float viewportHeight = viewport.rect.height;
    float maxScroll = Mathf.Max(0f, contentHeight - viewportHeight);

    if (maxScroll <= 0f)
    {
        scrollRect.verticalNormalizedPosition = 1f;
        yield break;
    }

    Vector3 targetLocalPosInContent = content.InverseTransformPoint(target.position);
    float targetDistanceFromTop = Mathf.Abs(targetLocalPosInContent.y);

    // ⭐ 타겟을 뷰포트 "맨 위"가 아니라 "중앙"에 오도록 오프셋 계산
    float desiredOffset = targetDistanceFromTop - (viewportHeight / 2f);
    desiredOffset = Mathf.Clamp(desiredOffset, 0f, maxScroll);

    Logger.Log($"[StorePanelManager] targetDistanceFromTop={targetDistanceFromTop}, desiredOffset={desiredOffset}, maxScroll={maxScroll}");

    float normalizedPos = 1f - (desiredOffset / maxScroll);
    Logger.Log($"[StorePanelManager] normalizedPos = {normalizedPos}");
    scrollRect.verticalNormalizedPosition = normalizedPos;
}
}