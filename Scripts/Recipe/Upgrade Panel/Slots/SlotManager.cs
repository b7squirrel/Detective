using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SlotManager : MonoBehaviour
{
    SlotsAllCards slotsAllCards;
    SlotsMatCards slotsMatCards;
    SlotUpCard slotUpCard;

    CardDataManager cardDataManager;
    CardsDictionary cardDictionary;
    UpgradeSuccessUI upgradeSuccessUI;

    [SerializeField] UnityEvent OnRefresh;

    bool isCardOnUpSlot; // 업그레이드 슬롯에 카드가 올려져 있음

    void Awake()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            child.gameObject.SetActive(true);
        }

        slotsAllCards = GetComponentInChildren<SlotsAllCards>();
        slotsMatCards = GetComponentInChildren<SlotsMatCards>();
        slotUpCard = GetComponentInChildren<SlotUpCard>();
        upgradeSuccessUI = GetComponentInChildren<UpgradeSuccessUI>();

        cardDataManager = FindObjectOfType<CardDataManager>();
        cardDictionary = FindObjectOfType<CardsDictionary>();
    }

    #region Refresh
    void OnEnable()
    {
        StartCoroutine(RefreshCo());
    }

    void OnDisable()
    {
        // GetIntoMyCardsmanager();
    }

    // 업그레이드 연출이 끝나면, 업그레이드 패널을 떠났다가 되돌아오면(Disabled, Enabled)
    // 업그레이드 패널을 갱신한다
    IEnumerator RefreshCo()
    {
        yield return new WaitForSeconds(.01f);
        RefreshUpgradePanel();
    }

    public void RefreshUpgradePanel()
    {
        // slotsAllCards, slotsMatCards, slotUpCard의 Clear함수들을 가지고 있음
        OnRefresh?.Invoke();
    }
    // MainMenuManger가 Awake에서 모든 패널을 비활성화 시키기 전에 
    // Refresh를 실행하는 것을 방지하기 위해 (NullReference)
    // 0.01초 후에 리프레시를 하도록 했음
    #endregion

    #region MyCards, MatCards 전환
    public void GetIntoMatCardsManager()
    {
        slotsAllCards.gameObject.SetActive(false);
        slotsMatCards.gameObject.SetActive(true);
    }

    public void GetIntoMyCardsmanager()
    {
        slotsAllCards.gameObject.SetActive(true);
        slotsMatCards.gameObject.SetActive(false);
    }
    #endregion
    
    #region Upgrade Success UI
    public void OpenUpgradeSuccesUI()
    {
        upgradeSuccessUI.gameObject.SetActive(true);
    }
    public void CloseUpgradeSuccessUI()
    {
        upgradeSuccessUI.gameObject.SetActive(false);

    }
    #endregion
}
