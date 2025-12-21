using UnityEngine;

public class ShopDebugPanelToggle : MonoBehaviour
{
    bool isActivated = false;

    public void ToggleShopDebugPanel()
    {
        isActivated = !isActivated;
        gameObject.SetActive(isActivated);
    }
}
