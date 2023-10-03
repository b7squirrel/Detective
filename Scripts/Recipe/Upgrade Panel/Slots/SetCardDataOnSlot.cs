using UnityEngine;

public class SetCardDataOnSlot : MonoBehaviour
{
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] CardList cardList;
    [SerializeField] EquipmentsAnimators equipAnims;

    /// <summary>
    /// CardData를 넣으면 Weapon인지 Item인지 판별해서 원하는 슬롯에 표시해 줌
    /// </summary>
    public void PutCardDataIntoSlot(CardData targetCardData, CardSlot targetSlot)
    {
        if (targetCardData.Type == CardType.Weapon.ToString())
        {
            WeaponData wData = cardDictionary.GetWeaponData(targetCardData);

            bool onEquipment = cardList.FindCharCard(targetCardData).IsEquipped;

            Debug.Log("target slot = " + targetSlot.name);
            if(equipAnims.GetEquipmentAnimators(targetCardData, targetSlot) == null)
            Debug.Log("NUll");
            RuntimeAnimatorController[] anims = equipAnims.GetEquipmentAnimators(targetCardData, targetSlot);

            foreach (var item in anims)
            {
                if(item == null) continue;
                Debug.Log(item.name);
            }
            targetSlot.SetWeaponCard(targetCardData, wData, anims, onEquipment);
        }
        else
        {
            Item iData = cardDictionary.GetItemData(targetCardData);

            bool onEquipment = cardList.FindEquipmentCard(targetCardData).IsEquipped;
            targetSlot.SetItemCard(targetCardData, iData, onEquipment);
        }
    }
}
