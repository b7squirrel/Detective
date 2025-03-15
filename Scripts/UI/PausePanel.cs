using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// weapon manager에서 무기를 추가할 떄 호출해서 초기화.
/// 무기를 업그레이드 할 때도 호출해서 업데이트.
/// </summary>
public class PausePanel : MonoBehaviour
{
    [SerializeField] GameObject cardSlot; // 오리 카드 슬롯 프리펩
    [SerializeField] GameObject itemSlot; // 아이템 카드 슬롯 프리펩
    [SerializeField] Transform weaponContents; // 무기 슬롯들을 집어넣을 레이아웃
    [SerializeField] Transform itemContents; // 아이템 슬롯들을 집어넣을 레이아웃
    List<PauseCardDisp> weaponCards;
    List<PauseCardDisp> itemCards;

    public void InitWeaponSlot(WeaponData wd, bool isLead)
    {
        if (weaponCards == null) weaponCards = new();

        GameObject wSlot = Instantiate(cardSlot, weaponContents.transform);
        CardSlot slot = wSlot.GetComponent<CardSlot>();
        weaponCards.Add(slot.GetComponent<PauseCardDisp>());

        SetEquipSpriteRow(slot, wd, isLead);
    }

    void SetEquipSpriteRow(CardSlot targetSlot, WeaponData wd, bool isLead)
    {
        // card disp와 Equip Disp UI에서 IEquipSpriteAnim을 인터페이스로 사용
        CardDisp cardDisp = targetSlot.GetComponent<CardDisp>();

        cardDisp.InitWeaponCardDisplay(wd, null);
        // CardDisp cardDisp = targetSlot.GetComponent<CardDisp>();
        cardDisp.InitSpriteRow(); // card sprite row의 이미지 참조들이 남지 않게 초기화

        for (int i = 0; i < 4; i++)
        {
            Item item = isLead ? GameManager.instance.startingDataContainer.GetItemDatas()[i] : wd.defaultItems[i];

            if (item == null)
            {
                cardDisp.SetEquipCardDisplay(i, null, false, Vector2.zero); // 이미지 오브젝트를 비활성화
                continue;
            }
            SpriteRow equipmentSpriteRow = item.spriteRow;
            Vector2 offset = item.needToOffset ? item.posHead : Vector2.zero;

            cardDisp.SetEquipCardDisplay(i, equipmentSpriteRow, item.needToOffset, offset);
        }
    }
    public void InitItemSlot(Item _item)
    {
        if (itemCards == null) itemCards = new();

        GameObject iSlot = Instantiate(itemSlot, itemContents.transform);
        itemCards.Add(iSlot.GetComponent<PauseCardDisp>());

        iSlot.GetComponent<PauseCardDisp>().InitItemCardDisplay(_item);
    }
    public void UpdateWeaponLevel(string _weaponName, int _levelToUpdate, bool _isSynergy)
    {
        for (int i = 0; i < weaponCards.Count; i++)
        {
            if (weaponCards[i].Name == _weaponName)
            {
                weaponCards[i].UpdatePauseCardLevel(_levelToUpdate, true, _isSynergy);
                return;
            }
        }

    }
    public void UpdateItemLevel(Item _item)
    {
        for (int i = 0; i < itemCards.Count; i++)
        {
            if (itemCards[i].Name == _item.Name)
            {
                itemCards[i].UpdatePauseCardLevel(_item.stats.currentLevel,false, false);
                return;
            }
        }
    }
}