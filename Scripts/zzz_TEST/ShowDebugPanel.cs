using UnityEngine;

public class ShowDebugPanel : MonoBehaviour
{
    bool isActive;

    void OnEnable()
    {
        isActive = false;
        SetDebugPanelScale(isActive);
    }
    public void ToggleDebugPanel()
    {
        isActive = !isActive;
        SetDebugPanelScale(isActive);
    }
    void SetDebugPanelScale(bool active)
    {
        transform.localScale = active ? Vector2.one : Vector2.zero;
    }
}
