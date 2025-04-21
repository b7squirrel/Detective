using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// 업그레이드 슬롯
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
        StartCoroutine(OnClickCo());
    }
    IEnumerator OnClickCo()
    {
        // 터치하면 일단 또잉또잉
        RectTransform slotRec = GetComponent<RectTransform>();
        float initialValue = slotRec.transform.localScale.x;
        slotRec.transform.localScale = new Vector2(initialValue * 1.1f, initialValue * 1.1f);
        slotRec.DOScale(initialValue, .04f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(.066f);

        if (upPanelManager == null)
        {
            upPanelManager = FindObjectOfType<UpPanelManager>();
        }

        if(upPanelManager == null) Debug.Log("UP Panel Manager가 Null입니다.");

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

    // public void SetSlotType(SlotType slotType)
    // {
    //     currentSlotType = slotType;
    // }
}
