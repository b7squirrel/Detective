using UnityEngine;
using UnityEngine.Events;

public class UpgradePanelSlotsManager : MonoBehaviour
{
    public UnityEvent OnRefresh;

    void OnEnable()
    {
        RefreshUpgradePanel();
    }

    // 업그레이드 연출이 끝나면, 업그레이드 패널을 떠났다가 되돌아오면(Disabled, Enabled)
    // 업그레이드 패널을 갱신한다
    public void RefreshUpgradePanel()
    {
        // Upgrade Slot, Mat Slots의 Clear함수들을 가지고 있음
        OnRefresh?.Invoke();
    }
}
