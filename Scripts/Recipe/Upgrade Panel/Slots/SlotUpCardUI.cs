using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlotUpCardUI : MonoBehaviour
{
    [SerializeField] UpgradeSuccessUI upgradeSuccessUI;

    [SerializeField] GameObject halo;
    [SerializeField] GameObject confirmationButtonContainer;
    [SerializeField] GameObject slotPanel;
    [SerializeField] GameObject panelUpgradePurple;
    [SerializeField] GameObject panelUpgradeDark;

    [SerializeField] Animator animUpSlot;
    [SerializeField] Animator animMatSlot;
    [SerializeField] Animator animPlus;
    RectTransform upSlot, matSlot;
    RectTransform upCard, matCard;

    public bool UpSlotState {get; set;} 

    void Start()
    {
        GetComponent<SlotUpCard>().OnCardAcquiredOnUpSlotUI += upCardAcquiredUI;
        GetComponent<SlotUpCard>().OnCardAcquiredOnMatSlotUI += matCardAcquiredUI;
        GetComponent<SlotUpCard>().OnUpgradeConfirmation += ActivationUpgradeConfirmationUI;
        GetComponent<SlotUpCard>().OnCloseUpgradeConfirmation += DeactivationUpgradeConfirmationUI;
        GetComponent<SlotUpCard>().OnMerging += MergingCardsUI;
        GetComponent<SlotUpCard>().OnRefreshUI += refreshUpSlotUI;
        GetComponent<SlotUpCard>().OnUpdateUI += UpdateUI;
    }

    void UpdateUI()
    {
        if (upCard != null && upSlot != null)
        {
            upCard.position = upSlot.position;
        }

        if(matCard != null & matSlot != null)
        {
            matCard.position = matSlot.position;
        }
    }

    void OnEnable()
    {
        Init();
        UpSlotState = false;
    }

    void Init()
    {
        // panelUpgradePurple.SetActive(true);
        panelUpgradeDark.SetActive(false);
        
        upgradeSuccessUI.gameObject.SetActive(false);
        animUpSlot.gameObject.SetActive(true);
        animMatSlot.gameObject.SetActive(false);
        animPlus.gameObject.SetActive(false);
        halo.SetActive(false);
        confirmationButtonContainer.SetActive(false);
        

    }

    void upCardAcquiredUI(Card card)
    {
        upCard = card.GetComponent<RectTransform>();
        upSlot = animUpSlot.GetComponent<RectTransform>();

        halo.SetActive(true);
        animUpSlot.SetTrigger("HavingCard");
        animMatSlot.gameObject.SetActive(true);
        animMatSlot.SetTrigger("Up");
        animPlus.gameObject.SetActive(true);
        animPlus.SetTrigger("PlusUp");
    }

    void matCardAcquiredUI(Card card)
    {
        matCard = card.GetComponent<RectTransform>();
        matSlot = animMatSlot.GetComponent<RectTransform>();
    }

    public void ActivationUpgradeConfirmationUI()
    {
        // panelUpgradePurple.SetActive(false);
        panelUpgradeDark.SetActive(true);

        slotPanel.SetActive(false);
        confirmationButtonContainer.SetActive(true);

        // 합성 혹은 취소 버튼이 아니라 카드를 클릭해서 취소하는 것을 방지
        upCard.GetComponent<CanvasGroup>().interactable = false;
        matCard.GetComponent<CanvasGroup>().interactable = false;
    }

    // 강화를 승인하면 강화 연출을 위해 확인 창을 없애기
    public void DeactivationUpgradeConfirmationUI()
    {
        confirmationButtonContainer.SetActive(false);

        upCard.GetComponent<CanvasGroup>().interactable = true;
        matCard.GetComponent<CanvasGroup>().interactable = true;
    }

    void MergingCardsUI()
    {
        animUpSlot.SetTrigger("Merging");
        animMatSlot.SetTrigger("Merging");
        animPlus.SetTrigger("PlusDown");
    }

    public void ActivateSlotPanel(bool open)
    {
        slotPanel.SetActive(open);
    }

    public void ActivateDarkPanel(bool open)
    {
        panelUpgradeDark.SetActive(open);
    }

    // 업그레이드 슬롯들을 다 비우는 리프레시
    void refreshUpSlotUI()
    {
        halo.SetActive(false);
        confirmationButtonContainer.SetActive(false);
        slotPanel.SetActive(true);

        if (UpSlotState == false) // 업그레이드 과정 중에 캔슬되었다는 의미
        {
            animUpSlot.SetTrigger("Canceled");
            animMatSlot.SetTrigger("Canceled");
            animPlus.SetTrigger("PlusDown");
        }

        upSlot = null;
        upCard = null;
        matSlot = null;
        matCard = null;

        // panelUpgradePurple.SetActive(true);
        panelUpgradeDark.SetActive(false);

        StartCoroutine(WaitToInitCo());
    }
    IEnumerator WaitToInitCo()
    {
        yield return new WaitForSeconds(.5f);
        Init();
    }
}
