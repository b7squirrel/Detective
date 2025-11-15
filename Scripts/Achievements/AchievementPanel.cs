using System.Collections.Generic;
using UnityEngine;

public class AchievementPanel : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject achievementItemPrefab;

    private Dictionary<string, AchievementItemUI> itemDict = new();

    private void OnEnable()
    {
        AchievementManager.Instance.OnAnyProgressChanged += UpdateItem;
        AchievementManager.Instance.OnAnyCompleted += UpdateItem;
        AchievementManager.Instance.OnAnyRewarded += RemoveItem;
    }

    private void OnDisable()
    {
        AchievementManager.Instance.OnAnyProgressChanged -= UpdateItem;
        AchievementManager.Instance.OnAnyCompleted -= UpdateItem;
        AchievementManager.Instance.OnAnyRewarded -= RemoveItem;
    }

    private void Start()
    {
        foreach (var ra in AchievementManager.Instance.GetAll())
        {
            if (ra.isRewarded) continue;

            var go = Instantiate(achievementItemPrefab, content);
            var ui = go.GetComponent<AchievementItemUI>();

            ui.Bind(ra);

            itemDict.Add(ra.original.id, ui);
        }
    }

    private void UpdateItem(RuntimeAchievement ra)
    {
        if (ra.isRewarded) return;

        if (itemDict.TryGetValue(ra.original.id, out var ui))
            ui.Refresh();
    }

    private void RemoveItem(RuntimeAchievement ra)
    {
        if (itemDict.TryGetValue(ra.original.id, out var ui))
        {
            Destroy(ui.gameObject);
            itemDict.Remove(ra.original.id);
        }
    }
}