using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipDisplayUI : MonoBehaviour
{
    [SerializeField] Image charImage;
    [SerializeField] TMPro.TextMeshProUGUI atk, hp;
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] CardSlot[] equipSlots; // 4개의 장비 슬롯
    [SerializeField] GameObject atkLabel, hpLabel;

    public void SetWeaponDisply(CardData cardData)
    {
        OnDisplay();
        WeaponData wd = cardDictionary.GetWeaponData(cardData);
        charImage.sprite = wd.charImage;

        atk.text = cardData.Atk;
        hp.text = cardData.Hp;
    }
    // 아직 사용되지 않았음
    public void SetItemDisplay(CardData cardData)
    {
        OnDisplay();
        Item item = cardDictionary.GetItemData(cardData);
        charImage.sprite = item.charImage;

        if(cardData.EquipmentType == EquipmentType.Weapon.ToString())
        {
            atk.text = cardData.Atk;
        }
        else
        {
            hp.text = cardData.Hp;
        }
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
        for (int i = 0; i < equipSlots.Length; i++)
        {
            equipSlots[i].EmptySlot();
        }
    }
    public void EmptyEquipSlot(int index)
    {
        equipSlots[index].EmptySlot();
    }

    public void SetSlot(int index, Item itemData, CardData cardToEquip)
    {
        equipSlots[index].SetItemCard(cardToEquip, itemData, false); // 장착 중 text가 표시될 필요가 없음
        equipSlots[index].GetComponent<CardDisp>().InitItemCardDisplay(itemData, false); // 장착 중 text가 표시될 필요가 없음
    }
    public void UpdateSlots(EquipmentCard[] equipmentCards)
    {
        for (int i = 0; i < 4; i++)
        {
            // 일단 기존 슬롯을 다 비우고
            equipSlots[i].EmptySlot();
            
            // 장착되어 있는 장비가 있다면
            if (equipmentCards[i] != null)
            {
                CardData cardData = equipmentCards[i].CardData;
                WeaponItemData weaponItemData =
                cardDictionary.GetWeaponItemData(cardData);
                if(weaponItemData.itemData == null) continue;
                equipSlots[i].SetItemCard(cardData, weaponItemData.itemData, false); // 장착 중 text가 표시될 필요가 없음
            }
        }
    }

    public bool IsEmpty(int index)
    {
        if (equipSlots[index].IsEmpty)
            return true;
        return false;
    }
}
