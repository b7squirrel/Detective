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
            WeaponData wData = cardDictionary.GetWeaponItemData(targetCardData).weaponData;

            targetSlot.SetWeaponCard(targetCardData, wData);
            SetAnimController(targetCardData, targetSlot);
        }
        else
        {
            Item iData = cardDictionary.GetWeaponItemData(targetCardData).itemData;

            bool onEquipment = cardList.FindEquipmentCard(targetCardData).IsEquipped;
            targetSlot.SetItemCard(targetCardData, iData, onEquipment);
        }
    }

    // 카드의 장비칸을 순회하면서 아이템이 있다면 표시해 줌
    void SetAnimController(CardData charCard, CardSlot targetSlot)
    {
        CardDisp cardDisp = targetSlot.GetComponent<CardDisp>();
        EquipmentCard[] equipCards = cardList.GetEquipmentsCardData(charCard);
        for (int i = 0; i < 4; i++)
        {
            if (equipCards[i] == null)
            {
                cardDisp.SetRunTimeAnimController(i, null);

                cardDisp.SetEquipCardImage(i, null);
                continue;
            }

            // 장비의 runtimeAnimatorController 구하기
            CardData equipCardData = equipCards[i].CardData;
            WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(equipCardData);

            if (weaponItemData.itemData == null) continue;

            //cardDisp.SetRunTimeAnimController(i, weaponItemData.itemData.CardItemAnimator.CardImageAnim);

            cardDisp.SetEquipCardImage(i, weaponItemData.itemData.charImage);
        }
    }
}
