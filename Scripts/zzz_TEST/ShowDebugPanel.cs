using UnityEngine;

public class ShowDebugPanel : MonoBehaviour
{
    bool isActive;
    CanvasGroup canvasGroup;

    void OnEnable()
    {
        isActive = false;
        ToggleDebugPanel(isActive);
    }
    public void ToggleDebugPanel()
    {
        isActive = !isActive;
        ToggleDebugPanel(isActive);
    }
    void ToggleDebugPanel(bool active)
    {
        // transform.localScale = active ? Vector2.one : Vector2.zero;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = active ? 1 : 0;
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }
}
