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

    private static ShopSectionType pendingSection = ShopSectionType.None;

    void OnEnable()
    {
        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Off");
        if (scrollRect == null) scrollRect = GetComponentInChildren<ScrollRect>();

        if (pendingSection != ShopSectionType.None)
        {
            ConsumePendingScroll();
            return;
        }

        scrollRect.verticalNormalizedPosition = 1f;
    }

    public static void RequestScrollTo(ShopSectionType section)
    {
        pendingSection = section;
    }

    // ⭐ 추가: 이미 상점 탭이 열려있을 때 직접 호출하기 위한 public 진입점
    public void ScrollToSectionImmediate(ShopSectionType section)
    {
        pendingSection = section;
        if (scrollRect == null) scrollRect = GetComponentInChildren<ScrollRect>();
        ConsumePendingScroll();
    }

    void ConsumePendingScroll()
    {
        RectTransform target = GetTargetBySection(pendingSection);
        pendingSection = ShopSectionType.None;

        if (target != null)
            StartCoroutine(ScrollToTargetCo(target));
        else
            scrollRect.verticalNormalizedPosition = 1f;
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

        float desiredOffset = targetDistanceFromTop - (viewportHeight / 2f);
        desiredOffset = Mathf.Clamp(desiredOffset, 0f, maxScroll);

        float normalizedPos = 1f - (desiredOffset / maxScroll);
        scrollRect.verticalNormalizedPosition = normalizedPos;
    }
}