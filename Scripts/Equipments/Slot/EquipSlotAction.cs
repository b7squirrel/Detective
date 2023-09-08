using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum EquipSlotType { FieldOri, FieldEquipment, UpEquipment, None }

public class EquipSlotAction : MonoBehaviour
{
    [SerializeField] protected EquipSlotType currentSlotType;

    public void Onclick()
    {
        StartCoroutine(OnClickCo());
    }
    IEnumerator OnClickCo()
    {
        // 터치하면 일단 또잉또잉
        RectTransform slotRec = GetComponent<RectTransform>();
        float initialValue = slotRec.transform.localScale.x;
        slotRec.transform.localScale = new Vector2(initialValue * 1.3f, initialValue * 1.3f);
        slotRec.DOScale(initialValue, .07f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(.066f);

        ActionType();
    }
    
    void ActionType()
    {
        if (currentSlotType == EquipSlotType.FieldOri)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            EquipmentPanelManager equipPanelManager = GetComponentInParent<EquipmentPanelManager>();
            equipPanelManager.SetDisplay(cardData);
            equipPanelManager.SetAllFieldTypeOf("Item");
            return;
        }
        if (currentSlotType == EquipSlotType.FieldEquipment)
        {
            EquipmentPanelManager equipPanelManager = GetComponentInParent<EquipmentPanelManager>();
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            equipPanelManager.ActivateEquipInfoPanel(cardData);
            return;
        }
        if (currentSlotType == EquipSlotType.UpEquipment)
        {
            GetComponent<CardSlot>().EmptySlot();
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
