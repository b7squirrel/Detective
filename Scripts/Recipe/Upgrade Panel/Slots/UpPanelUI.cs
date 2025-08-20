using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UpPanelUI : MonoBehaviour
{
    [SerializeField] GameObject confirmationButtonContainer;
    [SerializeField] Transform upgradeConfirmationButton;
    [SerializeField] GameObject fieldSlotPanel;
    [SerializeField] GameObject upgradeSuccessPanel;
    [SerializeField] RectTransform scrollContent;
    [SerializeField] RectTransform UpPanelBG;

    [SerializeField] RectTransform upSlot, matSlot, plus, upSuccess;

    [Header("합성 이펙트")]
    [SerializeField] Transform stars; // 합성된 카드의 별을 반짝이게 하기 위해서
    [SerializeField] Animator upEffectAnim; // 합성이 성공한 순간 번쩍하는 이펙트의 animator
    [SerializeField] GameObject upgradeEffect; // 합성이 성공한 순간 번찍하는 이펙트
    [SerializeField] GameObject blingBlingEffect; // 카드 주변에서 반짝반짝
    [SerializeField] Animator starBlingEffect;
    [SerializeField] AudioClip[] mergeSuccessSounds; // 카드 성공 짠 하는 소리들
    [SerializeField] AudioClip gradeBlingSound;
    [SerializeField] AudioClip starBlingBeamSound;
    [SerializeField] AudioClip starBlingSound;
    [SerializeField] RectTransform whiteEffectOnMerge;
    [SerializeField] GameObject newStatPanel;
    [SerializeField] GameObject tabToContinue;
    UpPanelManager upPanelManager;
    UICameraShake uiCameraShake;
    Coroutine starCoroutine;

    [Header("아래쪽 탭 제어")]
    [SerializeField] RectTransform tabs;
    Button[] tabButtons = new Button[5];

    public void Init()
    {
        upSlot.gameObject.SetActive(true);
        matSlot.gameObject.SetActive(false);
        fieldSlotPanel.gameObject.SetActive(true);
        plus.gameObject.SetActive(false);
        confirmationButtonContainer.SetActive(false);
        upgradeSuccessPanel.SetActive(false);
        upgradeEffect.SetActive(true);
        blingBlingEffect.SetActive(false);
        starBlingEffect.gameObject.SetActive(false);

        fieldSlotPanel.transform.localScale = Vector2.one;
        //fieldSlotPanel.transform.localScale = new Vector2(.95f, 1f);
        //fieldSlotPanel.transform.DOScale(1, .3f).SetEase(Ease.OutBack);

        //UpSlotInitAnimtion();
        BGInitAnimation();
    }

    #region Animation
    void BGInitAnimation() // 배경 초기화 애님
    {
        //UpPanelBG.transform.rotation = Quaternion.Euler(0, 0, -10f);
        //UpPanelBG.anchoredPosition = new Vector2(0, -70);
        //UpPanelBG.DOAnchorPosY(0, .2f).SetEase(Ease.OutElastic);
        //UpPanelBG.DORotate(new Vector3(0, 0, -30f), .2f).SetEase(Ease.OutBack);
        //UpPanelBG.transform.localScale = Vector2.zero;
        //UpPanelBG.DOScale(1.5f, .2f).SetEase(Ease.OutBack);
    }
    void UpSlotInitAnimtion() // 업그레이드 슬롯 초기화 애님
    {
        StartCoroutine(UpSlotInitAnimationCo());
    }
    IEnumerator UpSlotInitAnimationCo() // 업그레이드 슬롯이 조금 늦게 나타나도록
    {
        upSlot.transform.localScale = Vector2.one;
        //upSlot.transform.localScale = Vector2.zero;
        yield return new WaitForSeconds(.1f);
        //upSlot.DOScale(1f, .15f).SetEase(Ease.OutBack);
    }
    void UpCardAcquiredAnimation() // 업그레이드 슬롯에 카드를 올렸을 때 애님
    {
        upSlot.transform.localScale = .8f * Vector2.one;
        upSlot.DOScale(1f, .6f).SetEase(Ease.OutElastic);
        upSlot.DOAnchorPos(new Vector2(-205, 26), .15f);
        matSlot.DOAnchorPos(new Vector2(205, 26), .15f);
    }
    void MatCardAcquiredAnimation() // 재료 슬롯에 카드를 올렸을 때 애님
    {
        matSlot.transform.localScale = .8f * Vector2.one;
        matSlot.DOScale(1f, .6f).SetEase(Ease.OutElastic);
    }

    void UpSlotCanceledAnimation() // 업그레이드 슬롯 취소 탭
    {
        upSlot.anchoredPosition = Vector2.zero;
        matSlot.DOAnchorPos(new Vector2(0, 26), .15f).SetEase(Ease.OutBack);
        UpSlotInitAnimtion();
    }

    void UpgradeConfirmationAnimation() // 강화 확인 애님
    {
        upgradeConfirmationButton.localScale = Vector2.zero;
        fieldSlotPanel.transform.localScale = Vector2.zero;

        //fieldSlotPanel.GetComponent<RectTransform>().DOScale(new Vector2(0, 0), .15f).SetEase(Ease.InBack);
        upgradeConfirmationButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -560f);
        StartCoroutine(UpgradeConfirmationAnimationCo());
    }
    IEnumerator UpgradeConfirmationAnimationCo() // 강화 확인 버튼이 살짝 늦게 나오도록
    {
        yield return new WaitForSeconds(.15f);
        upgradeConfirmationButton.localScale = .7f * Vector2.one;
        upgradeConfirmationButton.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -560f), .15f).SetEase(Ease.OutBack);
        upgradeConfirmationButton.DOScale(1f, .15f).SetEase(Ease.OutBack);
    }
    void DeactivateUpgradeConfimation()
    {
        // StartCoroutine(DeactivateUpgradeConfirmationCo());

        upgradeConfirmationButton.transform.localScale = 0f * Vector2.one;
        fieldSlotPanel.gameObject.SetActive(true);
        fieldSlotPanel.transform.localScale = Vector2.one;
    }
    IEnumerator DeactivateUpgradeConfirmationCo()
    {
        // upgradeConfirmationButton.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -560), .15f).SetEase(Ease.OutBack);
        // upgradeConfirmationButton.DOScale(0, .15f).SetEase(Ease.InBack);

        upgradeConfirmationButton.transform.localScale = 0f * Vector2.one;

        yield return new WaitForSeconds(.15f);

        fieldSlotPanel.gameObject.SetActive(true);
        //fieldSlotPanel.GetComponent<RectTransform>().DOScale(new Vector2(1, 1), .15f).SetEase(Ease.InBack);
        //fieldSlotPanel.transform.localScale = new Vector2(.8f, .8f);
        //fieldSlotPanel.transform.DOScale(1, .15f).SetEase(Ease.OutBack);
        fieldSlotPanel.transform.localScale = Vector2.one;


    }
    #endregion

    public void UpCardAcquiredUI() // 업그레이드 슬롯 위에 카드 올렸을 때 UI
    {
        UpCardAcquiredAnimation();

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
        // 합성 연출 동안 클릭이 안되도록 하기


        plus.transform.localScale = new Vector3(.6f, .6f, .6f); // default .48f
        plus.DOScale(0, .2f).SetEase(Ease.InBack);
        upSlot.DOAnchorPos(new Vector2(0, 26), .15f).SetEase(Ease.InBack); // 0.15초 동안 가운데로 이동
        matSlot.DOAnchorPos(new Vector2(0, 26), .15f).SetEase(Ease.InBack);
    }
    public void DeactivateSpecialSlots() // 업그레이드, 재료 슬롯들을 비활성화
    {
        upSlot.gameObject.SetActive(false);
        matSlot.gameObject.SetActive(false);
        plus.gameObject.SetActive(false);
    }

    // 강화 성공 패널
    public void OpenUpgradeSuccessPanel(CardData cardData, bool isGradeUp)
    {
        StartCoroutine(OpenSuccessPanelCo(cardData, isGradeUp));
    }

    IEnumerator OpenSuccessPanelCo(CardData cardData, bool isGradeUp)
    {
        fieldSlotPanel.SetActive(false);
        // 합성 성공 패널 활성화. 그러나 아직 합성된 카드, 새로운 스탯 패널, 탭해서 계속하기는 숨김 
        upgradeSuccessPanel.SetActive(true);
        upSuccess.gameObject.SetActive(false);
        newStatPanel.SetActive(false);
        tabToContinue.SetActive(false);

        // 타격감 White 이펙트
        whiteEffectOnMerge.gameObject.SetActive(true);
        whiteEffectOnMerge.GetComponent<Image>().color = Color.white;
        whiteEffectOnMerge.localScale = Vector2.one;
        // whiteEffectOnMerge.DOScale(1.2f, .4f);
        whiteEffectOnMerge.DOScale(.5f, 1f); // 1초 동안 절반으로 줄어들기 (antic)
        
        yield return new WaitForSeconds(1.7f);
        foreach (var item in mergeSuccessSounds)
        {
            SoundManager.instance.Play(item); // 짠 하는 사운드
        }
        upgradeEffect.SetActive(true);  // 방사형 이펙트. 버튼 클릭 이펙트 재활용
        upEffectAnim.SetTrigger("On");

        upSuccess.gameObject.SetActive(true); // 합성된 카드 절반 크기로 활성화
        upSuccess.localScale = .5f * Vector2.one;
        upSuccess.DOScale(1.3f, .07f);

        whiteEffectOnMerge.DOScale(1.8f, .1f); // 0.2초 동안 1.3배 커지기 (overshoot)
        whiteEffectOnMerge.GetComponent<Image>().DOFade(0, .6f); // 0.6초 동안 사라지기

        yield return new WaitForSeconds(.6f);
        blingBlingEffect.SetActive(true);
        newStatPanel.SetActive(true);
        tabToContinue.SetActive(true);
        upgradeEffect.SetActive(false);
        //upSuccess.localScale = .8f * Vector2.one;
        //upSuccess.DOScale(1f, .5f).SetEase(Ease.OutBack);

        whiteEffectOnMerge.gameObject.SetActive(false);

        // 등급이 올랐다면 타이틀 리본 반짝 이펙트
        if (isGradeUp)
        {

            yield return new WaitForSeconds(.5f); // 0.5초 후에 별이 반짝 하도록
        }

        // 별 반짝 이펙트
        GlimmerStar();
    }

    // 별 반짝임
    void GlimmerStar()
    {
        starBlingEffect.gameObject.SetActive(true);
        starBlingEffect.SetTrigger("On");
        SoundManager.instance.Play(starBlingBeamSound);

        if (starCoroutine != null) StopCoroutine(starCoroutine);
        starCoroutine = StartCoroutine(GlimmerStarCo());
    }

    IEnumerator GlimmerStarCo()
    {
        yield return new WaitForSeconds(.2f);

        starBlingEffect.gameObject.SetActive(false);



        Animator[] starAnims = stars.GetComponentsInChildren<Animator>();
        for(int i = 0; i < starAnims.Length; i++)
        {
            starAnims[i].SetTrigger("Blink");
            SoundManager.instance.Play(starBlingSound);
            yield return new WaitForSeconds(.1f);
        }
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