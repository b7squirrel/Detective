using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlotType { upSlot, matSlot, listSlot, none }

public class CardAction : MonoBehaviour
{
    RectTransform rect;

    SlotUpCard slotUpCard;
    SlotManager slotManager;
    SlotType currentSlotType;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        slotUpCard = FindAnyObjectByType<SlotUpCard>();
        slotManager = FindAnyObjectByType<SlotManager>();
        currentSlotType = SlotType.listSlot;
    }

    public void OnClick()
    {
        // currentSlotType : 초기값은 listSlot
        // 한 번이라도 클릭되었다면 GetSlotType으로 값이 조정됨 
        if (currentSlotType == SlotType.none)
            return;

        // 업그레이드 슬롯에 올려진 카드를 클릭해서 취소
        if (currentSlotType == SlotType.upSlot)
        {
            slotManager.GetIntoMyCards();
            Destroy(gameObject);
            return; // return이 없으면 Destroy 이후에도 아래로 내려가서 실행한다
        }

        // 재료 슬롯에 올려진 카드를 클릭해서 취소
        if (currentSlotType == SlotType.matSlot)
        {
            slotManager.BackToMatCards();
            Destroy(gameObject);
            return;
        }

        // 업그레이드 슬롯, 재료 슬롯 위에 있지 않은 카드들은 아래쪽 리스트 카드들이다
        if (currentSlotType == SlotType.listSlot)
        {
            currentSlotType = slotUpCard.GetSlotType(GetComponent<Card>());
            if (currentSlotType == SlotType.none) // 재료 슬롯에 이미 다른 카드가 올라가 있다면
                return;
            slotUpCard.AcquireCard(GetComponent<Card>());
        }
        else
        {
            // SlotType이 none이라는 뜻. 아무 일도 일어나지 않음
            return;
        }
    }
}
