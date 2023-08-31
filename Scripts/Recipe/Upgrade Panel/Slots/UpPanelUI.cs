using System.Collections;
using UnityEngine;
using DG.Tweening;

public class UpPanelUI : MonoBehaviour
{
    [SerializeField] GameObject haloUpCard;
    [SerializeField] GameObject haloMatCard;
    [SerializeField] GameObject confirmationButtonContainer;
    [SerializeField] GameObject fieldSlotPanel; 
    [SerializeField] GameObject panelUpgradeDark;

    [SerializeField] RectTransform upSlot, matSlot, plus;

    void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        // panelUpgradePurple.SetActive(true);
        // panelUpgradeDark.SetActive(false);

        upSlot.gameObject.SetActive(true);
        matSlot.gameObject.SetActive(false);
        plus.gameObject.SetActive(false);
        haloUpCard.SetActive(false);
        confirmationButtonContainer.SetActive(false);
    }

    #region Animation
    // upSlot with a card
    void UpCardAcquiredAnimation()
    {
        upSlot.DOAnchorPos(new Vector2(-140,26), .15f).SetEase(Ease.OutBack);
        matSlot.DOAnchorPos(new Vector2(200,26), .15f).SetEase(Ease.OutBack);
    }
    // upslot canceled
    void UpSlotCanceledAnimation()
    {
        upSlot.DOAnchorPos(new Vector2(0,26), .15f).SetEase(Ease.OutBack);
        matSlot.DOAnchorPos(new Vector2(0,26), .15f).SetEase(Ease.OutBack);
    }

    void UpgradeConfirmationAnimation()
    {
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
        UpgradeConfirmationAnimation();

        confirmationButtonContainer.SetActive(true);

        // 합성 혹은 취소 버튼이 아니라 카드를 클릭해서 취소하는 것을 방지
        // animUpSlot.GetComponent<CanvasGroup>().interactable = false;
        // animMatSlot.GetComponent<CanvasGroup>().interactable = false;
    }

    // 강화를 승인하면 강화 연출을 위해 확인 창을 없애기
    public void OffUpgradeConfirmationUI()
    {
        confirmationButtonContainer.SetActive(false);
    }

    public void MergingCardsUI()
    {
        upSlot.DOAnchorPos(new Vector2(0,26), .15f).SetEase(Ease.OutBack);
        matSlot.DOAnchorPos(new Vector2(0,26), .15f).SetEase(Ease.OutBack);
        haloUpCard.SetActive(false);
    }
    public void DeactivateSpecialSlots()
    {
        upSlot.gameObject.SetActive(false);
        matSlot.gameObject.SetActive(false);
        plus.gameObject.SetActive(false);
    }
    
    // 업그레이드 슬롯들을 다 비우는 리프레시
    public void RefreshUpSlotUI()
    {
        Debug.Log("Refresh");
        haloUpCard.SetActive(false);
        confirmationButtonContainer.SetActive(false);
        fieldSlotPanel.SetActive(true);

        panelUpgradeDark.SetActive(false);

        StartCoroutine(WaitToInitCo());
    }
    IEnumerator WaitToInitCo()
    {
        yield return new WaitForSeconds(.5f);
        Init();
    }
}