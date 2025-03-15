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
                // Debug.Log($"{charCard.Name}의 {i}번째 장비칸을 비활성화 합니다.");
                equipSpriteAnim.SetEquipCardDisplay(i, null, false, Vector2.zero); // 이미지 오브젝트를 비활성화
                continue;
            }

            // 장비의 runtimeAnimatorController 구하기
            CardData equipCardData = equipCards[i].CardData;
            WeaponItemData weaponItemData = cardDictionary.GetWeaponItemData(equipCardData);

            if (weaponItemData.itemData == null)
            {
                // Debug.Log($"{charCard.Name}의 {i}번째 장비가 NULL입니다.");
                continue;
            }

            // Debug.Log($"{weaponItemData.itemData.DisplayName}의 스프라이트를 넘겨줍니다.");

            // weapon data의 디폴트 아이템의 이미지를 넘겨주는 것이 아니라 장비 카드 데이터로 검색한 아이템의 이미지를 넘겨주어야 함.
            Item item = cardDictionary.GetWeaponItemData(equipCardData).itemData;
            SpriteRow equipmentSpriteRow = item.spriteRow;
            Vector2 offset = item.needToOffset ? item.posHead : Vector2.zero;

            // Debug.Log($"{cardDictionary.GetWeaponItemData(equipCardData).itemData.DisplayName}을 디스플레이 할 것입니다.");
            equipSpriteAnim.SetEquipCardDisplay(i, equipmentSpriteRow, item.needToOffset, offset);
        }
    }
}
