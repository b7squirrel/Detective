using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementPanel : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject achievementItemPrefab;

    Dictionary<string, AchievementItemUI> itemDict = new();
    CardSlotManager cardSlotManager;

    // 🔥 삭제 대기 리스트 (코루틴 중단 대비)
    private List<RuntimeAchievement> pendingRemoveList = new();

    private void OnEnable()
    {
        if (AchievementManager.Instance == null) return;

        AchievementManager.Instance.OnAnyProgressChanged += UpdateItem;
        AchievementManager.Instance.OnAnyCompleted += UpdateItem;
        AchievementManager.Instance.OnAnyRewarded += RemoveItem;
        
        // ★ 언어 변경 이벤트 구독 추가
        LocalizationManager.OnLanguageChanged += RefreshAllText;

        if (cardSlotManager == null) cardSlotManager = FindObjectOfType<CardSlotManager>();
        cardSlotManager.SettrigerAnim("Off");

        // 🔥 패널이 다시 켜질 때, 삭제 대기 중이었던 항목들 마무리
        foreach (var ra in pendingRemoveList.ToList())
        {
            FinishRemove(ra);
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAnyProgressChanged -= UpdateItem;
            AchievementManager.Instance.OnAnyCompleted -= UpdateItem;
            AchievementManager.Instance.OnAnyRewarded -= RemoveItem;
        }
        
        // ★ 언어 변경 이벤트 구독 해제 추가
        LocalizationManager.OnLanguageChanged -= RefreshAllText;
    }

    private void Start()
    {
        if (AchievementManager.Instance == null) return;
        
        foreach (var ra in AchievementManager.Instance.GetAll())
        {
            if (ra.isRewarded) continue;

            var go = Instantiate(achievementItemPrefab, content);
            var ui = go.GetComponent<AchievementItemUI>();

            ui.Bind(ra);
            itemDict.Add(ra.original.id, ui);
        }

        RefreshUI();
    }

    private void UpdateItem(RuntimeAchievement ra)
    {
        if (ra.isRewarded) return;

        if (itemDict.TryGetValue(ra.original.id, out var ui))
            ui.Refresh();

        RefreshUI();
    }

    // ★ 모든 업적 텍스트 새로고침 (언어 변경 시)
    private void RefreshAllText()
    {
        foreach (var kvp in itemDict)
        {
            AchievementItemUI ui = kvp.Value;
            if (ui != null && ui.ra != null)
            {
                // AchievementItemUI의 UpdateText는 이벤트로 자동 호출되지만
                // 혹시 모를 경우를 대비해 명시적으로 Refresh 호출
                ui.Refresh();
            }
        }
    }

    // =======================================================
    //                   🔥 삭제 처리 시스템
    // =======================================================

    private void RemoveItem(RuntimeAchievement ra)
    {
        // 삭제 대기 리스트에 먼저 등록
        if (!pendingRemoveList.Contains(ra))
            pendingRemoveList.Add(ra);

        StartCoroutine(RemoveItemCo(ra));
    }

    IEnumerator RemoveItemCo(RuntimeAchievement ra)
    {
        if (itemDict.TryGetValue(ra.original.id, out var ui))
        {
            ui.GetComponent<Animator>().SetTrigger("Swipe");
            yield return new WaitForSeconds(0.5f);
        }

        // 🔥 코루틴 중단되어도 OnEnable에서 마무리됨
        FinishRemove(ra);
    }

    // 🔥 실제 삭제 처리 (코루틴 성공/중단 상관없이 여기서 최종 처리)
    private void FinishRemove(RuntimeAchievement ra)
    {
        if (itemDict.TryGetValue(ra.original.id, out var ui))
        {
            Destroy(ui.gameObject);
            itemDict.Remove(ra.original.id);
        }

        pendingRemoveList.Remove(ra);

        RefreshUI();
    }

    // =======================================================

    /// <summary>
    /// UI 정렬: 완료된 항목 위, 그 안에서는 SO 리스트 순서대로
    /// </summary>
    public void RefreshUI()
    {
        var items = content.GetComponentsInChildren<AchievementItemUI>();

        var sortedItems = items
            .OrderByDescending(i => i.ra.isCompleted)
            .ThenBy(i => AchievementManager.Instance.achievementSOList.IndexOf(i.ra.original))
            .ToList();

        for (int i = 0; i < sortedItems.Count; i++)
        {
            sortedItems[i].transform.SetSiblingIndex(i);
        }
    }

    // Debug 용: 모든 업적 완료 표시
    public void ForceCompleteAllAchievements()
    {
        foreach (var kvp in itemDict)
        {
            AchievementItemUI ui = kvp.Value;
            ui.ForceComplete();
        }

        RefreshUI();
    }
}