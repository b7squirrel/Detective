using UnityEngine;

public class SetCardDataOnSlot : MonoBehaviour
{
    [SerializeField] CardsDictionary cardDictionary;
    [SerializeField] CardList cardList;

    /// <summary>
    /// CardData를 넣으면 Weapon인지 Item인지 판별해서 원하는 슬롯에 표시해 줌
    /// </summary>
    public void PutCardDataIntoSlot(CardData targetCardData, CardSlot targetSlot)
    {
        if (targetCardData.Type == CardType.Weapon.ToString())
        {
            WeaponData wData = cardDictionary.GetWeaponData(targetCardData);

            bool onEquipment = cardList.FindCharCard(targetCardData).IsEquipped;

            targetSlot.SetWeaponCard(targetCardData, wData, onEquipment);
            SetAnimController(targetCardData, targetSlot);
        }
        else
        {
            Item iData = cardDictionary.GetItemData(targetCardData);

            bool onEquipment = cardList.FindEquipmentCard(targetCardData).IsEquipped;
            targetSlot.SetItemCard(targetCardData, iData, onEquipment);
        }
    }

    void SetAnimController(CardData charCard, CardSlot targetSlot)
    {
        CardDisp cardDisp = targetSlot.GetComponent<CardDisp>();
        EquipmentCard[] equipCards = cardList.GetEquipmentsCardData(charCard);
        for (int i = 0; i < 4; i++)
        {
            if (equipCards[i] == null)
            {
                cardDisp.SetRunTimeAnimController(i, null);
                continue;
            }

            // 장비의 runtimeAnimatorController 구하기
            CardData equipCardData = equipCards[i].CardData;
            WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(equipCardData);

            if (weaponItemData.itemData == null) continue;

            cardDisp.SetRunTimeAnimController(i, weaponItemData.itemData.CardItemAnimator.CardImageAnim);
        }
    }
}
