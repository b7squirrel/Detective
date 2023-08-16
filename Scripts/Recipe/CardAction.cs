using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAction : MonoBehaviour
{
    RectTransform rect;
    
    SlotUpCard slotUpCard;
    SlotManager slotManager;

    bool isOnUpSlot; // 업그레이드 슬롯 위에 올라가 있는지 여부

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        slotUpCard = FindAnyObjectByType<SlotUpCard>();
        slotManager = FindAnyObjectByType<SlotManager>();
    }
    
    public void OnClick()
    {
        if (isOnUpSlot)
        {
            slotManager.GetIntoMyCardsmanager();
            Destroy(gameObject);
            return; // return이 없으면 Destroy 이후에도 아래로 내려가서 실행한다
        }

        // 업그레이드 슬롯 위로 올릴 수 있는지 체크
        if (slotUpCard.IsAvailable(GetComponent<Card>()))
        {
            // 업그레이드 슬롯위로 카드를 떨어트리면 draggable의 역할은 끝
            slotUpCard.AcquireCard(GetComponent<Card>());
            isOnUpSlot = true;
        }
        else
        {
            // 아무 일도 일어나지 않음
            return;
        }
    }
}
