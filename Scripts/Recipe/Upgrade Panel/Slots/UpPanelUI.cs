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

    #region Anim 업그레이드 슬롯 - with Card
    public void UpSlotToWithCardAnimation()
    {
        upSlot.DOAnchorPos(new Vector2(-140,26), .1f).SetEase(Ease.OutElastic);
        matSlot.DOAnchorPos(new Vector2(200,26), .1f).SetEase(Ease.OutElastic);
    }
    #endregion

    public void UpCardAcquiredUI()
    {
        UpSlotToWithCardAnimation();
        haloUpCard.SetActive(true);

        matSlot.gameObject.SetActive(true);
        plus.gameObject.SetActive(true);
    }

    public void UpgradeConfirmationUI()
    {
        panelUpgradeDark.SetActive(true);

        fieldSlotPanel.SetActive(false);
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
        haloUpCard.SetActive(false);
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