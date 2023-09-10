using UnityEngine;
using UnityEngine.UI;

public class EquipDisplayUI : MonoBehaviour
{
    [SerializeField] Image charImage;
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] EquipSlot[] equipSlots; // 4개의 장비 슬롯
    [SerializeField] GameObject atkLabel, hpLabel;

    public void SetWeaponDisply(CardData cardData)
    {
        OnDisplay();
        WeaponData wd = cardDictionary.GetWeaponData(cardData);
        charImage.sprite = wd.charImage;
    }
    public void SetItemDisplay(CardData cardData)
    {
        OnDisplay();
        Item item = cardDictionary.GetItemData(cardData);
        charImage.sprite = item.charImage;
    }
    public void OffDisplay()
    {
        EmptyEquipSlots();
        atkLabel.SetActive(false);
        hpLabel.SetActive(false);
        charImage.color = new Color(1, 1, 1, 0);
    }
    public void OnDisplay()
    {
        atkLabel.SetActive(true);
        hpLabel.SetActive(true);
        charImage.color = new Color(1, 1, 1, 1);
    }
    void EmptyEquipSlots()
    {
        foreach (var item in equipSlots)
        {
            item.EmptySlot();
        }
    }
    public void EmptyEquipSlot(int index)
    {
        equipSlots[index].EmptySlot();
    }

    public void SetSlot(int index, Item itemData, CardData cardToEquip)
    {
        equipSlots[index].SetItemCard(cardToEquip, itemData);
        equipSlots[index].GetComponent<CardDisp>().InitItemCardDisplay(itemData);
    }
    public void UpdateSlots(EquipmentCard[] equipmentCards)
    {
        for (int i = 0; i < 4; i++)
        {
            // 일단 기존 슬롯을 다 비우고
            equipSlots[i].EmptySlot();
            
            // 장착되어 있는 장비가 있다면
            if (equipmentCards[i].CardData.Name != null)
            {
                CardData cardData = equipmentCards[i].CardData;
                WeaponItemData weaponItemData =
                cardDictionary.GetWeaponItemData(cardData);
                // Debug.Log("ItemData = " + weaponItemData.itemData.Name);
                
                equipSlots[i].SetItemCard(cardData, weaponItemData.itemData);
            }
        }
    }
}
