using UnityEngine;

public class TopBarAddButton : MonoBehaviour
{
    [SerializeField] MainMenuManager mainMenuManager;
    [SerializeField] ShopSectionType sectionType;

    public void OnAddButtonClicked()
    {
        if (mainMenuManager == null)
            mainMenuManager = FindObjectOfType<MainMenuManager>();

        if (mainMenuManager == null)
        {
            Logger.LogError("[TopBarAddButton] MainMenuManager를 찾을 수 없습니다.");
            return;
        }

        // ⭐ 이미 상점 탭(0번)에 있으면 OnEnable이 재발생하지 않으니 직접 스크롤 호출
        if (mainMenuManager.GetTabIndex() == 0)
        {
            StorePanelManager storePanelManager = FindObjectOfType<StorePanelManager>();
            if (storePanelManager != null)
                storePanelManager.ScrollToSectionImmediate(sectionType);
            else
                Logger.LogError("[TopBarAddButton] StorePanelManager를 찾을 수 없습니다.");

            return;
        }

        StorePanelManager.RequestScrollTo(sectionType);
        mainMenuManager.SetTabPos(0);
    }
}