using UnityEngine;

public enum SlotType { upSlot, matSlot, listSlot, none }

public class CardAction : MonoBehaviour
{
    UpPanelManager upPanelManager;
    SlotType currentSlotType;

    void Awake()
    {
        upPanelManager = FindAnyObjectByType<UpPanelManager>();
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
            upPanelManager.GetIntoAllField();
            GetComponent<CardSlot>().EmptySlot();
            return; // return이 없으면 Destroy 이후에도 아래로 내려가서 실행한다
        }

        // 재료 슬롯에 올려진 카드를 클릭해서 취소
        if (currentSlotType == SlotType.matSlot)
        {
            upPanelManager.BackToMatField();
            GetComponent<CardSlot>().EmptySlot();
            return;
        }

        // 업그레이드 슬롯, 재료 슬롯 위에 있지 않은 카드들은 아래쪽 리스트 카드들이다
        // 리스트 카드를 클릭했을 때
        if (currentSlotType == SlotType.listSlot)
        {
            upPanelManager.AcquireCard(GetComponent<CardSlot>().GetCardData());
        }
        else
        {
            // SlotType이 none이라는 뜻. 아무 일도 일어나지 않음
            return;
        }
    }
}
