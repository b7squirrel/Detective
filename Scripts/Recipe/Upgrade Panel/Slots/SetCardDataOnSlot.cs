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
            SetEquipSpriteRow(targetCardData, targetSlot);
        }
        else
        {
            Item iData = cardDictionary.GetWeaponItemData(targetCardData).itemData;

            bool onEquipment = cardList.FindEquipmentCard(targetCardData).IsEquipped;
            targetSlot.SetItemCard(targetCardData, iData, onEquipment);
        }
    }

    // 카드의 장비칸을 순회하면서 아이템이 있다면 이미지를 넘겨줌
    void SetEquipSpriteRow(CardData charCard, CardSlot targetSlot)
    {
        // card disp와 Equip Disp UI에서 IEquipSpriteAnim을 인터페이스로 사용
        IEquipSpriteAnim equipSpriteAnim = targetSlot.GetComponent<IEquipSpriteAnim>();
        // CardDisp cardDisp = targetSlot.GetComponent<CardDisp>();
        equipSpriteAnim.InitSpriteRow(); // card sprite row의 이미지 참조들이 남지 않게 초기화

        EquipmentCard[] equipCards = cardList.GetEquipmentsCardData(charCard);
        for (int i = 0; i < 4; i++)
        {
            if (equipCards[i] == null)
            {
                // cardDisp.SetRunTimeAnimController(i, null);

                equipSpriteAnim.SetEquipCardDisplay(i, null); // 이미지 오브젝트를 비활성화
                continue;
            }

            // 장비의 runtimeAnimatorController 구하기
            CardData equipCardData = equipCards[i].CardData;
            WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(equipCardData);

            if (weaponItemData.itemData == null) continue;

            // Debug.Log($"{weaponItemData.itemData.Name}을 디스플레이 할 것입니다.");
            equipSpriteAnim.SetEquipCardDisplay(i, weaponItemData.itemData.spriteRow);
        }
    }
}
