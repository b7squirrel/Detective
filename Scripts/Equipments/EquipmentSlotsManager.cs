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

        //int charAtk = cardDictionary.GetWeaponItemData(charCardData).weaponData.stats.damage;
        int charAtk = charCardData.Atk;
        int charHp = charCardData.Hp;
        currentAttribute = new OriAttribute(charAtk, charHp);
        Logger.Log($"Equipment Slot Manager. 오리만의 ATK = {charAtk}");

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
        UpdateAttributeOnEquipment(cardToEquip, 1); // attr data 업데이트
    }

    /// <summary>
    /// 장비가 장착될 때, 해제될 때 각각 스탯을 업데이트, UI도 업데이트
    /// </summary>
    void UpdateAttributeOnEquipment(CardData _equipCardData, int addingFactor)
    {
        // 장비 카드의 attribute
        int Hp = _equipCardData.Hp;
        int Atk = _equipCardData.Atk;

        // 해당 장비의 attribute 더해줌. 필수 무기들은 모두 공격력. 나머지는 모두 방어력
        if (_equipCardData.EssentialEquip == EssentialEquip.Essential.ToString())
        {
            currentAttribute = new OriAttribute(addingFactor * Atk + currentAttribute.Atk, currentAttribute.Hp);
        }
        else
        {
            currentAttribute = new OriAttribute(currentAttribute.Atk, addingFactor * Hp + currentAttribute.Hp);
        }

        equipDisplayUI.SetWeaponDisplay(instantCharCard, currentAttribute, cardDictionary.GetDisplayName(instantCharCard)); // attr ui 업데이트
        Logger.Log($"Equipment Slot Manager. charATK 장비 모두 장착 후 = {currentAttribute.Atk}"); // 여기까지 좋음
    }

    public OriAttribute GetCurrentAttribute() => currentAttribute;

    /// <summary>
    /// 디스플레이 되고 있는 오리의 어트리뷰트 업데이트. 레벨업 시.
    /// </summary>
    public void UpdateCurrentAttribute(CardData cardOnDisplay)
    {
        InitEquipSlots(cardOnDisplay);
    }

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

        UpdateAttributeOnEquipment(cardDataToRemove, -1);

        // 장비가 빠지면 오리 스탯 업데이트
        // equipDisplayUI.SetEquipmentDisplay(cardDataToRemove, false);
    }
    public bool IsEmpty(int index)
    {
        if (equipSlots[index].IsEmpty)
            return true;
        return false;
    }
    public CardData GetSlotCardData(int index)
    {
        return equipSlots[index].GetCardData();
    }
}
