using System.Collections;
using UnityEngine;
using DG.Tweening;

public class UpPanelUI : MonoBehaviour
{
    [SerializeField] GameObject haloUpCard;
    [SerializeField] GameObject confirmationButtonContainer;
    [SerializeField] Transform upgradeConfirmationButton;
    [SerializeField] GameObject fieldSlotPanel;
    [SerializeField] GameObject panelUpgradeDark;
    [SerializeField] GameObject upgradeSuccessPanel;
    [SerializeField] RectTransform scrollContent;
    [SerializeField] RectTransform UpPanelBG;

    [SerializeField] RectTransform upSlot, matSlot, plus, upSuccess;

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
    void BGInitAnimation() // 배경 초기화 애님
    {
        // UpPanelBG.GetComponentInParent<Animator>().SetTrigger("Init");
        UpPanelBG.transform.rotation = Quaternion.Euler(0, 0, -10f);
        UpPanelBG.anchoredPosition = new Vector2(0, -70);
        UpPanelBG.DOAnchorPosY(0, .2f).SetEase(Ease.OutElastic);
        UpPanelBG.DORotate(new Vector3(0, 0, -30f), .2f).SetEase(Ease.OutBack);
        UpPanelBG.transform.localScale = Vector2.zero;
        UpPanelBG.DOScale(1.5f, .2f).SetEase(Ease.OutBack);
    }
    void UpSlotInitAnimtion() // 업그레이드 슬롯 초기화 애님
    {
        StartCoroutine(UpSlotInitAnimationCo());
    }
    IEnumerator UpSlotInitAnimationCo() // 업그레이드 슬롯이 조금 늦게 나타나도록
    {
        upSlot.transform.localScale = Vector2.zero;
        yield return new WaitForSeconds(.1f);
        upSlot.DOScale(1f, .15f).SetEase(Ease.OutBack);
    }
    void UpCardAcquiredAnimation() // 업그레이드 슬롯에 카드를 올렸을 때 애님
    {
        upSlot.transform.localScale = .8f * Vector2.one;
        upSlot.DOScale(1f, .6f).SetEase(Ease.OutElastic);
        upSlot.DOAnchorPos(new Vector2(-110, 26), .15f).SetEase(Ease.OutBack);
        matSlot.DOAnchorPos(new Vector2(160, 26), .15f).SetEase(Ease.OutBack);
    }
    void MatCardAcquiredAnimation() // 재료 슬롯에 카드를 올렸을 때 애님
    {
        matSlot.transform.localScale = .5f * Vector2.one;
        matSlot.DOScale(.6f, .6f).SetEase(Ease.OutElastic);
    }
    
    void UpSlotCanceledAnimation() // 업그레이드 슬롯 취소 탭
    {
        upSlot.anchoredPosition = Vector2.zero;
        matSlot.DOAnchorPos(new Vector2(0, 26), .15f).SetEase(Ease.OutBack);
        UpSlotInitAnimtion();
    }

    void UpgradeConfirmationAnimation() // 강화 확인 애님
    {
        fieldSlotPanel.GetComponent<RectTransform>().DOScale(new Vector2(0, 0), .15f).SetEase(Ease.InBack);
        upgradeConfirmationButton.localScale = Vector2.zero;
        upgradeConfirmationButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -560f);
        StartCoroutine(UpgradeConfirmationAnimationCo());
    }
    IEnumerator UpgradeConfirmationAnimationCo() // 강화 확인 버튼이 살짝 늦게 나오도록
    {
        yield return new WaitForSeconds(.15f);
        upgradeConfirmationButton.localScale = .7f * Vector2.one;
        upgradeConfirmationButton.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -520), .15f).SetEase(Ease.OutBack);
        upgradeConfirmationButton.DOScale(1f, .15f).SetEase(Ease.OutBack);
    }
    void DeactivateUpgradeConfimation()
    {
        StartCoroutine(DeactivateUpgradeConfirmationCo());
    }
    IEnumerator DeactivateUpgradeConfirmationCo()
    {
        upgradeConfirmationButton.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -560), .15f).SetEase(Ease.OutBack);
        upgradeConfirmationButton.DOScale(0, .15f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(.15f);

        fieldSlotPanel.gameObject.SetActive(true);
        fieldSlotPanel.GetComponent<RectTransform>().DOScale(new Vector2(1, 1), .15f).SetEase(Ease.InBack);
        fieldSlotPanel.transform.localScale = new Vector2(.8f, .8f);
        fieldSlotPanel.transform.DOScale(1, .15f).SetEase(Ease.OutBack);
    }
    #endregion

    public void UpCardAcquiredUI() // 업그레이드 슬롯 위에 카드 올렸을 때 UI
    {
        UpCardAcquiredAnimation();

        haloUpCard.SetActive(true);
        matSlot.gameObject.SetActive(true);
        plus.gameObject.SetActive(true);
        plus.DOScale(.48f, .05f).SetEase(Ease.OutBack);
    }
    public void MatCardAcquiredUI() // 재료 슬롯 위에 카드 올렸을 때 UI
    {
        MatCardAcquiredAnimation();
    }
    public void UpSlotCanceled() // 업그레이드 슬롯 탭해서 취소
    {
        UpSlotCanceledAnimation();

        haloUpCard.SetActive(false);
        matSlot.gameObject.SetActive(false);
        plus.gameObject.SetActive(false);
    }

    public void UpgradeConfirmationUI() // 강화 확인 UI
    {
        confirmationButtonContainer.SetActive(true);
        UpgradeConfirmationAnimation();
    }

    public void MatSlotCanceled()
    {
        OffUpgradeConfirmationUI();
        

    }
    public void OffUpgradeConfirmationUI() // 강화를 승인하면 강화 연출을 위해 확인 창을 없애기
    {
        DeactivateUpgradeConfimation();
    }

    public void MergingCardsUI() // 카드 합치기 UI
    {
        plus.transform.localScale = new Vector3(.6f, .6f, .6f); // default .48f
        plus.DOScale(0, .2f).SetEase(Ease.InBack);
        upSlot.DOAnchorPos(new Vector2(0, 26), .15f).SetEase(Ease.InBack);
        matSlot.DOAnchorPos(new Vector2(0, 26), .15f).SetEase(Ease.InBack);
    }
    public void DeactivateSpecialSlots() // 업그레이드, 재료 슬롯들을 비활성화
    {
        haloUpCard.SetActive(false);
        upSlot.gameObject.SetActive(false);
        matSlot.gameObject.SetActive(false);
        plus.gameObject.SetActive(false);
    }

// 강화 성공 패널
    public void OpenUpgradeSuccessPanel(CardData cardData, DisplayCardOnSlot displayCardOnSlot)
    {
        upgradeSuccessPanel.SetActive(true);
        EquipSlot successCardSlot = upgradeSuccessPanel.GetComponentInChildren<EquipSlot>();
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