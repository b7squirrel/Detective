using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// 업그레이드 슬롯
/// 클릭하면 상황에 따라 슬롯 타입을 결정
/// </summary>
public class SlotAction : MonoBehaviour
{
    [Header("Equip Slot")]
    [SerializeField] protected SlotType equipSlotType;

    [Header("Equipment Type If Needed")]
    [SerializeField] protected EquipmentType equipmentType;

    [Header("Launch Slot")]
    [SerializeField] protected SlotType launchSlotType;

    [Header("Merge Slot")]
    [SerializeField] protected SlotType mergeSlotType;

    // 애니메이션 중복 클릭 방지용 플래그
    private bool isAnimating = false;

    SlotType currentSlotType;
    protected UpPanelManager upPanelManager;
    protected MainMenuManager mainMenuManager;
    protected EquipmentPanelManager equipPanelManager;
    protected LaunchManager launchManager;

    void Awake()
    {

    }

    public void Onclick()
    {
        // 애니메이션 중이면 클릭 무시
        if (isAnimating) 
        {
            Debug.Log("Animation in progress, click ignored");
            return;
        }
        
        StartCoroutine(OnClickCo());
    }

    IEnumerator OnClickCo()
    {
        // 애니메이션 시작
        isAnimating = true;

        RectTransform slotRec = GetComponent<RectTransform>();
        float initialValue = slotRec.transform.localScale.x;

        // 부드럽게 크기 증가 후 감소
        Sequence clickSequence = DOTween.Sequence();
        clickSequence.Append(slotRec.DOScale(initialValue * 1.1f, 0.08f).SetEase(Ease.OutQuad))
                    .Append(slotRec.DOScale(initialValue, 0.12f).SetEase(Ease.OutBack));

        RectTransform cardRec = GetComponent<RectTransform>();
        FindObjectOfType<CardEffect>().SetEffectPosition(cardRec);

        // 전체 애니메이션 완료까지 대기
        yield return new WaitForSeconds(0.2f);

        // 애니메이션 완료
        isAnimating = false;

        if (upPanelManager == null)
        {
            upPanelManager = FindObjectOfType<UpPanelManager>();
        }
        if (upPanelManager == null) Debug.Log("UP Panel Manager가 Null입니다.");
        ActionType();
    }

    void ActionType()
    {
        // 어떤 탭에 들어와 있는지에 따라 슬롯 타입 정하기
        SetSlotType();
        Debug.Log($"Current Slot Type은 {currentSlotType}");

        // 머지 슬롯
        if (currentSlotType == SlotType.M_Field)
        {
            // 장착이 되어 있는지 확인하는 메서드 실행
            upPanelManager.CheckIsEquipped(GetComponent<CardSlot>().GetCardData());
            return;
        }
        if (currentSlotType == SlotType.M_Up)
        {
            upPanelManager.SetUpSlotCanceled(true);
            upPanelManager.GetIntoAllField(GetComponent<CardSlot>().GetCardData().Type);
            GetComponent<CardSlot>().EmptySlot();
            return;
        }
        if (currentSlotType == SlotType.M_Mat)
        {
            upPanelManager.BackToMatField();
            GetComponent<CardSlot>().EmptySlot();
            return;
        }

        // 론치 슬롯
        if (currentSlotType == SlotType.L_Up)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            launchManager.SetAllFieldTypeOf("Weapon", cardData);
            return;
        }
        if (currentSlotType == SlotType.L_Field)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            launchManager.UpdateLead(cardData);
            return;
        }

        // 장비 슬롯
        if (currentSlotType == SlotType.E_FieldOri)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            equipPanelManager.InitDisplay(cardData);
            equipPanelManager.SetAllFieldTypeOf("Item");
            return;
        }
        if (currentSlotType == SlotType.E_FieldEquipment)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            equipPanelManager.ActivateEquipInfoPanel(cardData, GetComponent<CardDisp>(), true, equipmentType);
            return;
        }
        if (currentSlotType == SlotType.E_UpEquipment)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            equipPanelManager.ActivateEquipInfoPanel(cardData, GetComponent<CardDisp>(), false, equipmentType);
            return;
        }
        if (currentSlotType == SlotType.None)
            return;
    }

    void SetSlotType()
    {
        if (mainMenuManager == null) mainMenuManager = FindObjectOfType<MainMenuManager>();
        int tabindex = mainMenuManager.GetTabIndex();
        // 1-equip 2-launch 3-merge
        if (tabindex == 1)
        {
            currentSlotType = equipSlotType;

            if (GetComponent<CardSlot>().GetCardData().Type == "Item")
                currentSlotType = SlotType.E_FieldEquipment;
                
            if (equipPanelManager == null) equipPanelManager = FindObjectOfType<EquipmentPanelManager>();
        }
        else if (tabindex == 2)
        {
            currentSlotType = launchSlotType;
            if (launchManager == null) launchManager = FindObjectOfType<LaunchManager>();
        }
        else if (tabindex == 3)
        {
            currentSlotType = mergeSlotType;
            if(upPanelManager == null) upPanelManager = FindObjectOfType<UpPanelManager>();
        }
        else
        {
            Debug.Log("장비, 론치, 머지 탭 외의 상태에서 슬롯이 눌러졌습니다. 오류입니다.");
        }
    }

    public void SetEquipSlotType(SlotType equipSlotType)
    {
        currentSlotType = equipSlotType;
    }

    // 외부에서 애니메이션 상태 확인용 (필요시)
    public bool IsAnimating()
    {
        return isAnimating;
    }

    // 외부에서 강제로 애니메이션 상태 리셋용 (필요시)
    public void ResetAnimationState()
    {
        isAnimating = false;
    }
}