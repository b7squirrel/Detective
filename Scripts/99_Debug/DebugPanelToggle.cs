using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPanelToggle : MonoBehaviour
{
    bool isActive;
    [SerializeField] CanvasGroup canvasGroup;

    void OnEnable()
    {
        var config = Resources.Load<GameConfig>("GameConfig");
        isActive = config != null && config.isDebugMode;

        ToggleDebugPanel(isActive);
    }

    public void ToggleDebugPanel()
    {
        isActive = !isActive;
        ToggleDebugPanel(isActive);
    }
    void ToggleDebugPanel(bool active)
    {
        if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();
        canvasGroup.alpha = active ? 1 : 0;
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
        canvasGroup.gameObject.SetActive(active);   // 자식(Debug Panel)을 토글
    }
}
