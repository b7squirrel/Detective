using UnityEngine;

public class EquipmentSlotsManager : MonoBehaviour
{
    [SerializeField] CardSlot[] equipSlots; // 4개의 장비 슬롯
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] EquipDisplayUI equipDisplayUI;
    [SerializeField] CardList cardList;
    [SerializeField] CardDataManager cardDataManager;
    OriAttribute currentAttribute;

    CardData instantCharCard; // 현재 display 되고 있는 charCard 임시 저장

    // 오리가 display될 때 처음 한 번 순회하며 업데이트
    public void InitEquipSlots(CardData charCardData)
    {
        instantCharCard = charCardData;

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
            if (equips[i] == null)
                continue;

            CardData cardData = equips[i].CardData;
            WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(cardData);
            if (weaponItemData.itemData == null) continue;
            SetEquipSlot(i, weaponItemData.itemData, cardData);
            equipDisplayUI.SetEquipmentDisplay(cardData, true);
        }
    }

    /// <summary>
    /// 장비를 장착하면 equipment panel manager에서 Set slot 호출해서 해당 장비 슬롯에 배정
    /// </summary>
    public void SetEquipSlot(int index, Item itemData, CardData cardToEquip)
    {
        // 장착 중 text가 표시될 필요가 없음
        equipSlots[index].SetItemCard(cardToEquip, itemData, false);

        // 장비가 더해지면 오리 스탯 업데이트
        UpdateAttribute(cardToEquip, true); // attr data 업데이트
        equipDisplayUI.SetEquipmentDisplay(cardToEquip, true);
    }

    /// <summary>
    /// 장비가 장착될 때, 해제될 때 각각 스탯을 업데이트, UI도 업데이트
    /// </summary>
    void UpdateAttribute(CardData _equipCardData, bool isAdding)
    {
        // 장비 카드의 attribute
        int Hp = new Convert().StringToInt(_equipCardData.Hp);
        int Atk = new Convert().StringToInt(_equipCardData.Atk);

        int addingFactor = isAdding ? 1 : -1; // 장착이면 더하기, 해제면 빼기

        // 해당 장비의 attribute 더해줌
        if (_equipCardData.EssentialEquip == EssentialEquip.Essential.ToString())
        {
            currentAttribute = new OriAttribute(addingFactor * Atk + currentAttribute.Atk, currentAttribute.Hp);
        }
        else
        {
            currentAttribute = new OriAttribute(currentAttribute.Atk, addingFactor * Hp + currentAttribute.Hp);
        }

        equipDisplayUI.SetWeaponDisplay(instantCharCard, currentAttribute); // attr ui 업데이트
    }

    public OriAttribute GetCurrentAttribute() => currentAttribute;

    public void ClearEquipSlots()
    {
        for (int i = 0; i < equipSlots.Length; i++)
        {
            equipSlots[i].EmptySlot();
        }
    }
    /// <summary>
    /// 장비 칸의 index를 받아서 비움, Stat UI 업데이트
    /// </summary>
    public void EmptyEquipSlot(int index)
    {
        CardData cardDataToRemove = equipSlots[index].GetCardData();
        equipSlots[index].EmptySlot();

        UpdateAttribute(cardDataToRemove, false);

        // 장비가 빠지면 오리 스탯 업데이트
        equipDisplayUI.SetEquipmentDisplay(cardDataToRemove, false);
    }
    public bool IsEmpty(int index)
    {
        if (equipSlots[index].IsEmpty)
            return true;
        return false;
    }
}
