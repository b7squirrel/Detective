using UnityEngine;

public class ShowDebugPanel : MonoBehaviour
{
    bool isActive;
    CanvasGroup canvasGroup;

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
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = active ? 1 : 0;
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
        gameObject.SetActive(active);
    }
}