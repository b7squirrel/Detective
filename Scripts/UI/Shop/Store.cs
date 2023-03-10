using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Store : MonoBehaviour
{
    [SerializeField] ItemBuffer itemBuffer;
    [SerializeField] Transform slotRoot;
    [SerializeField] GameObject itemInfoPanel;
    List<Slot> slots;
    Slot currentSlot;

    public System.Action<ItemProperty> onSlotClick;

    void Start() // 상점에 아이템을 배치하는 부분. 나중에 다시 셋업 해야함
    {
        slots = new List<Slot>();

        int slotCnt = slotRoot.childCount;
        for (int i = 0; i < slotCnt; i++)
        {
            Slot slot = slotRoot.GetChild(i).GetComponent<Slot>();

            if (i < itemBuffer.items.Count)
            {
                slot.SetItem(itemBuffer.items[i]);
            }

            slots.Add(slot);
        }

        itemInfoPanel.SetActive(false);
    }
    void Update()
    {

    }

    public void OnClickSlot(Slot slot)
    {
        currentSlot = slot.GetComponent<Slot>();
        OpenStoreItemInfo(currentSlot);
    }
    public void OnClickPrice()
    {
        onSlotClick?.Invoke(currentSlot.item);
        CloseStoreItemInfo();
    }
    public void OpenStoreItemInfo(Slot slot)
    {
        if(slot.IsEmpty)
            return;
        itemInfoPanel.SetActive(true);
        itemInfoPanel.GetComponent<StoreItemInfo>().SetInfo(slot.item);
    }
    public void CloseStoreItemInfo()
    {
        itemInfoPanel.SetActive(false);
    }
}
