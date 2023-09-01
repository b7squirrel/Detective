using System.Collections;
using UnityEngine;
using DG.Tweening;

public class UpPanelUI : MonoBehaviour
{
    [SerializeField] GameObject haloUpCard;
    [SerializeField] GameObject haloMatCard;
    [SerializeField] GameObject confirmationButtonContainer;
    [SerializeField] Transform upgradeConfirmationButton;
    [SerializeField] GameObject fieldSlotPanel;
    [SerializeField] GameObject panelUpgradeDark;
    [SerializeField] GameObject upgradeSuccessPanel;
    [SerializeField] RectTransform scrollContent;
    [SerializeField] RectTransform UpPanelBG;

    [SerializeField] RectTransform upSlot, matSlot, plus, upSuccess;
    Coroutine InitCoroutine;

    public void Init()
    {
        upSlot.gameObject.SetActive(true);
        matSlot.gameObject.SetActive(false);
        plus.gameObject.SetActive(false);
        haloUpCard.SetActive(false);
        confirmationButtonContainer.SetActive(false);
        upgradeSuccessPanel.SetActive(false);

        fieldSlotPanel.transform.localScale = new Vector2(.8f, .8f);
        fieldSlotPanel.transform.DOScale(1, .15f).SetEase(Ease.OutBack);
        UpSlotInitAnimtion();
        BGInitAnimation();
    }
    
    #region Animation
    void BGInitAnimation()
    {
        UpPanelBG.transform.rotation = Quaternion.Euler(0, 0, 10f);
        UpPanelBG.anchoredPosition = new Vector2(0, -70);
        UpPanelBG.DOAnchorPosY(0, .2f).SetEase(Ease.OutBack);
        UpPanelBG.DORotate(new Vector3(0, 0, -30f), .2f).SetEase(Ease.OutBack);
        UpPanelBG.transform.localScale = .4f * Vector2.one;
        UpPanelBG.DOScale(1f, .15f).SetEase(Ease.OutBack);
    }
    void UpSlotInitAnimtion()
    {
        StartCoroutine(UpSlotInitAnimationCo());
    }
    IEnumerator UpSlotInitAnimationCo()
    {
        upSlot.transform.localScale = Vector2.zero;
        yield return new WaitForSeconds(.05f);
        upSlot.DOScale(1f, .15f).SetEase(Ease.OutBack);
    }
    void UpCardAcquiredAnimation()
    {
        upSlot.DOAnchorPos(new Vector2(-140, 26), .15f).SetEase(Ease.OutBack);
        matSlot.DOAnchorPos(new Vector2(200, 26), .15f).SetEase(Ease.OutBack);
    }
    // upslot canceled
    void UpSlotCanceledAnimation()
    {
        upSlot.anchoredPosition = Vector2.zero;
        matSlot.DOAnchorPos(new Vector2(0, 26), .15f).SetEase(Ease.OutBack);
        UpSlotInitAnimtion();
    }

    void UpgradeConfirmationAnimation()
    {
        upgradeConfirmationButton.localScale = Vector2.zero;
        upgradeConfirmationButton.DOScale(1f, .15f).SetEase(Ease.OutBack);
    }
    #endregion

    public void UpCardAcquiredUI()
    {
        UpCardAcquiredAnimation();

        haloUpCard.SetActive(true);
        matSlot.gameObject.SetActive(true);
        plus.gameObject.SetActive(true);
    }
    public void UpSlotCanceled()
    {
        UpSlotCanceledAnimation();

        haloUpCard.SetActive(false);
        matSlot.gameObject.SetActive(false);
        plus.gameObject.SetActive(false);
    }

    public void UpgradeConfirmationUI()
    {
        confirmationButtonContainer.SetActive(true);
        UpgradeConfirmationAnimation();
    }

    // 강화를 승인하면 강화 연출을 위해 확인 창을 없애기
    public void OffUpgradeConfirmationUI()
    {
        confirmationButtonContainer.SetActive(false);
    }

    public void MergingCardsUI()
    {
        upSlot.DOAnchorPos(new Vector2(0, 26), .15f).SetEase(Ease.OutBack);
        matSlot.DOAnchorPos(new Vector2(0, 26), .15f).SetEase(Ease.OutBack);
    }
    public void DeactivateSpecialSlots()
    {
        haloUpCard.SetActive(false);
        upSlot.gameObject.SetActive(false);
        matSlot.gameObject.SetActive(false);
        plus.gameObject.SetActive(false);
    }

    public void OpenUpgradeSuccessPanel(CardData cardData, DisplayCardOnSlot displayCardOnSlot)
    {
        upgradeSuccessPanel.SetActive(true);
        CardSlot successCardSlot = upgradeSuccessPanel.GetComponentInChildren<CardSlot>();
        displayCardOnSlot.DispCardOnSlot(cardData, successCardSlot);

        upSuccess.localScale = .8f * Vector2.one;
        upSuccess.DOScale(1f, .5f).SetEase(Ease.OutBack);

        fieldSlotPanel.SetActive(false);
    }

    // upgrade success 버튼에서 호출
    public void CloseUpgradeSuccessUI()
    {
        upgradeSuccessPanel.gameObject.SetActive(false);
        upSlot.gameObject.SetActive(true);
        matSlot.gameObject.SetActive(true);
        fieldSlotPanel.SetActive(true);
    }
    public void ResetScrollContent() // 스크롤뷰를 원래 위치로 되돌려 준다
    {
        // 통 튀기는 효과를 위해 아래로 당겼다가 원래 위치로
        scrollContent.anchoredPosition = new Vector2(scrollContent.anchoredPosition.x, 150f);
        scrollContent.DOAnchorPosY(0, .15f).SetEase(Ease.OutBack);
    }
}