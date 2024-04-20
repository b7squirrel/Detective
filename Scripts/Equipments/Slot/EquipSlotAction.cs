using System.Collections;
using UnityEngine;
using DG.Tweening;

public class EquipSlotAction : MonoBehaviour
{
    [SerializeField] protected EquipSlotType currentSlotType;
    [SerializeField] protected EquipmentType equipmentType;

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

        //yield return new WaitForSeconds(.066f);
        yield return new WaitForSeconds(.001f);

        ActionType();
    }
    
    void ActionType()
    {
        if (currentSlotType == EquipSlotType.FieldOri)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            EquipmentPanelManager equipPanelManager = GetComponentInParent<EquipmentPanelManager>();
            equipPanelManager.InitDisplay(cardData);
            equipPanelManager.SetAllFieldTypeOf("Item");
            return;
        }
        if (currentSlotType == EquipSlotType.FieldEquipment)
        {
            EquipmentPanelManager equipPanelManager = GetComponentInParent<EquipmentPanelManager>();
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            equipPanelManager.ActivateEquipInfoPanel(cardData, GetComponent<CardDisp>(), true, equipmentType);
            return;
        }
        if (currentSlotType == EquipSlotType.UpEquipment)
        {
            EquipmentPanelManager equipPanelManager = GetComponentInParent<EquipmentPanelManager>();
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            equipPanelManager.ActivateEquipInfoPanel(cardData, GetComponent<CardDisp>(), false, equipmentType);
            return;
        }
        if (currentSlotType == EquipSlotType.None)
            return;
    }
    public void SetSlotType(EquipSlotType equipSLotType)
    {
        currentSlotType = equipSLotType;
    }
}
