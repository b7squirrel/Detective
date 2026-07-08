using UnityEngine;

/// <summary>
/// 상단 리소스 바의 + 버튼 (에너지/골드/크리스탈 공용)
/// 상점 탭으로 이동 + 해당 재화 섹션으로 자동 스크롤
/// </summary>
public class TopBarAddButton : MonoBehaviour
{
    [SerializeField] MainMenuManager mainMenuManager;
    [SerializeField] ShopSectionType sectionType;

    public void OnAddButtonClicked()
    {
        Logger.Log($"[TopBarAddButton] 클릭됨: {sectionType}");
        
        if (mainMenuManager == null)
            mainMenuManager = FindObjectOfType<MainMenuManager>();

        if (mainMenuManager == null)
        {
            Logger.LogError("[TopBarAddButton] MainMenuManager를 찾을 수 없습니다.");
            return;
        }

        // ⭐ 순서 중요: 탭 전환보다 먼저 예약해야 StorePanelManager.OnEnable 시점에 반영됨
        StorePanelManager.RequestScrollTo(sectionType);
        mainMenuManager.SetTabPos(0); // 0 = Shop 탭
    }
}