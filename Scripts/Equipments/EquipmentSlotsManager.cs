using UnityEngine;

public class EquipmentSlotsManager : MonoBehaviour
{
    [SerializeField] CardSlot[] equipSlots; // 4개의 장비 슬롯
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] CardList cardList;
    OriAttribute currentAttribute;

    // 오리가 display될 때 처음 한 번 순회하며 업데이트
    public void InitEquipSlots(CardData charCardData)
    {
        int charAtk = cardDictionary.GetWeaponItemData(charCardData).weaponData.stats.damage;
        int charHp = new Convert().StringToInt(charCardData.Hp);
        currentAttribute = new OriAttribute(charAtk, charHp);

        EquipmentCard[] equips = cardList.GetEquipmentsCardData(charCardData);
        if (equips == null) return;

        for (int i = 0; i < 4; i++)
        {
            // 일단 기존 슬롯을 다 비우고
            equipSlots[i].EmptySlot();

            // 장착되어 있는 장비가 있다면 순회하며 장착 (logic + UI)
            if (equips[i] != null)
            {
                Debug.Log(i + " 번째 장비 = " + equips[i].cardName);
                CardData cardData = equips[i].CardData;
                WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(cardData);
                if (weaponItemData.itemData == null) continue;
                SetEquipSlot(i, weaponItemData.itemData, cardData);
                UpdateAttribute(cardData, weaponItemData.itemData, true);
            }
        }
    }

    // 장비를 장착하면 equipment panel manager에서 Set slot 호출해서 해당 장비 슬롯에 배정
    public void SetEquipSlot(int index, Item itemData, CardData cardToEquip)
    {
        // 장착 중 text가 표시될 필요가 없음
        equipSlots[index].SetItemCard(cardToEquip, itemData, false);

        // 장비가 더해지면 오리 스탯 업데이트
        UpdateAttribute(cardToEquip, itemData, true);
    }

    // 장비가 장착될 때, 해제될 때 각각 스탯을 업데이트
    void UpdateAttribute(CardData _equipCardData, Item itemData, bool isAdding)
    {
        // 장비 카드의 attribute
        int Hp = new Convert().StringToInt(_equipCardData.Hp);
        int Atk = new Convert().StringToInt(_equipCardData.Atk);

        int addingFactor = isAdding ? 1 : -1; // 장착이면 더하기, 해제면 빼기

        // 해당 장비의 attribute 더해줌
        if (_equipCardData.EquipmentType == EquipmentType.Weapon.ToString())
        {
            currentAttribute = new OriAttribute(Atk, addingFactor * Hp + itemData.stats.hp);
        }
        else
        {
            currentAttribute = new OriAttribute(addingFactor * Atk + (int)itemData.stats.damage, Hp);
        }
    }

    public OriAttribute GetCurrentAttribute() => currentAttribute;
    
    public void EmptyEquipSlots()
    {
        for (int i = 0; i < equipSlots.Length; i++)
        {
            equipSlots[i].EmptySlot();
        }
    }
    public void EmptyEquipSlot(int index)
    {
        CardData cardDataToRemove = equipSlots[index].GetCardData();
        equipSlots[index].EmptySlot();

        // 장비가 빠지면 오리 스탯 업데이트
        UpdateAttribute(cardDataToRemove, cardDictionary.GetWeaponItemData(cardDataToRemove).itemData, false);

    }
    public bool IsEmpty(int index)
    {
        if (equipSlots[index].IsEmpty)
            return true;
        return false;
    }
}
