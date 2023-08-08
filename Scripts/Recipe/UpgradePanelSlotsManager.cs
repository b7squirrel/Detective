using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// MainMenuManger가 Awake에서 모든 패널을 비활성화 시키기 전에 
// Refresh를 실행하는 것을 방지하기 위해 (NullReference)
// 0.01초 후에 리프레시를 하도록 했음
public class UpgradePanelSlotsManager : MonoBehaviour
{
    [SerializeField] MyCardsSlotManager myCardsSlotManager;
    [SerializeField] MatSlotManager matSlotManager;
    [SerializeField] UpgradeSlot upgradeSlot;

    public UnityEvent OnRefresh;

    void OnEnable()
    {
        StartCoroutine(RefreshCo());
    }

    // 업그레이드 연출이 끝나면, 업그레이드 패널을 떠났다가 되돌아오면(Disabled, Enabled)
    // 업그레이드 패널을 갱신한다
    public void RefreshUpgradePanel()
    {
        // Upgrade Slot, Mat Slots의 Clear함수들을 가지고 있음
        OnRefresh?.Invoke();
    }
    IEnumerator RefreshCo()
    {
        yield return new WaitForSeconds(.01f);
        RefreshUpgradePanel();
    }

    public void GetIntoMatCardsManager()
    {
        myCardsSlotManager.gameObject.SetActive(false);
        matSlotManager.gameObject.SetActive(true);
    }

    public void GetIntoMyCardsmanager()
    {
        myCardsSlotManager.gameObject.SetActive(true);
        matSlotManager.gameObject.SetActive(false);
    }
}
