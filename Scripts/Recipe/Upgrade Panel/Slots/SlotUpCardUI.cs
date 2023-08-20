using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotUpCardUI : MonoBehaviour
{
    [SerializeField] UpgradeSuccessUI upgradeSuccessUI;

    [SerializeField] GameObject halo;
    [SerializeField] GameObject confirmationButtonContainer;
    [SerializeField] GameObject slotPanel;

    [SerializeField] Animator animUpSlot;
    [SerializeField] Animator animMatSlot;
    [SerializeField] Animator animPlus;
    RectTransform upSlot, matSlot;
    RectTransform upCard, matCard;

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
    }

    void Init()
    {
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
        animMatSlot.SetTrigger("Init");
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
        slotPanel.SetActive(false);
        confirmationButtonContainer.SetActive(true);
    }

    public void DeactivationUpgradeConfirmationUI()
    {
        confirmationButtonContainer.SetActive(false);
    }

    void MergingCardsUI()
    {
        animUpSlot.SetTrigger("Merging");
        animMatSlot.SetTrigger("Merging");
    }

    void refreshUpSlotUI()
    {
        halo.SetActive(false);
        confirmationButtonContainer.SetActive(false);
        slotPanel.SetActive(true);
        animUpSlot.SetTrigger("Canceled");
        animMatSlot.SetTrigger("Canceled");
        animPlus.SetTrigger("PlusDown");
        upSlot = null;
        upCard = null;
        matSlot = null;
        matCard = null;
        StartCoroutine(WaitToInitCo());
    }
    IEnumerator WaitToInitCo()
    {
        yield return new WaitForSeconds(.5f);
        Init();
    }
}
