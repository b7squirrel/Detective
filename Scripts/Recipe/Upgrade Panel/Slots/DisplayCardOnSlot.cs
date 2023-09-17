using UnityEngine;

public class DisplayCardOnSlot : MonoBehaviour
{
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] CardList cardList;

    /// <summary>
    /// CardData를 넣으면 Weapon인지 Item인지 판별해서 원하는 슬롯에 표시해 줌
    /// </summary>
    public void DispCardOnSlot(CardData targetCardData, CardSlot targetSlot)
    {
        if (targetCardData.Type == CardType.Weapon.ToString())
        {
            WeaponData wData = cardDictionary.GetWeaponData(targetCardData);
            WeaponItemData data = new(wData, null);
            bool onEquipment = cardList.FindCharCard(targetCardData).IsEquipped;

            targetSlot.SetWeaponCard(targetCardData, data.weaponData, onEquipment);

        }
        else
        {
            Item iData = cardDictionary.GetItemData(targetCardData);
            WeaponItemData data = new WeaponItemData(null, iData);
            bool onEquipment = cardList.FindEquipmentCard(targetCardData).IsEquipped;
            targetSlot.SetItemCard(targetCardData, data.itemData, onEquipment);
        }
    }
}
