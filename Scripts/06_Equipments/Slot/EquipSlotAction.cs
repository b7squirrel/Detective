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
        RectTransform slotRec = GetComponent<RectTransform>();
        float initialValue = slotRec.transform.localScale.x;

        // 부드럽게 크기 증가 후 감소
        Sequence clickSequence = DOTween.Sequence();
        clickSequence.Append(slotRec.DOScale(initialValue * 1.1f, 0.08f).SetEase(Ease.OutQuad))
                    .Append(slotRec.DOScale(initialValue, 0.12f).SetEase(Ease.OutBack));

        // 전체 애니메이션 완료까지 대기
        yield return new WaitForSeconds(0.2f);

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
