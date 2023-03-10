using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] Transform rootSlot;
    [SerializeField] Store store;
    [SerializeField] GameObject itemInfoPanel;
    List<Slot> slots;

    public System.Action<ItemProperty> onInvenSlot;

    void Start()
    {
        slots = new List<Slot>();

        int slotCnt = rootSlot.childCount;

        for (int i = 0; i < slotCnt; i++)
        {
            Slot slot = rootSlot.GetChild(i).GetComponent<Slot>();
            slots.Add(slot);
        }

        store.onSlotClick += BuyItem;
    }

    void BuyItem(ItemProperty item)
    {
        Slot emptySlot = slots.Find(t => t.item == null || t.item.name == string.Empty);

        if(emptySlot == null)
        return;
        
        emptySlot.SetItem(item);
    }
    public void OpenItemInfo(Slot slot)
    {
        if(slot.IsEmpty)
            return;
        itemInfoPanel.SetActive(true);
        itemInfoPanel.GetComponent<ItemInfo>().SetInfo(slot.item);
    }
    public void CloseItemInfo()
    {
        itemInfoPanel.SetActive(false);
    }
}
