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
    [SerializeField] GameObject BG;
    List<CardDisp> weaponCards;
    List<PauseCardDisp> itemCards;

    // 이름으로 검색해서 업그레이드 상태를 업데이트하기 위한 딕셔너리
    Dictionary<string, PauseCardDisp> pauseCardDisps = new Dictionary<string, PauseCardDisp>();

    public void InitWeaponSlot(WeaponData wd, bool isLead)
    {
        if (weaponCards == null) weaponCards = new();

        GameObject wSlot = Instantiate(cardSlot, weaponContents.transform);
        CardSlot slot = wSlot.GetComponent<CardSlot>();
        weaponCards.Add(slot.GetComponent<CardDisp>());

        PauseCardDisp pauseDisp = slot.GetComponent<PauseCardDisp>();
        pauseCardDisps.Add(wd.Name, pauseDisp);
        
        if(isLead) 
        
        pauseDisp.InitWeaponCardDisplay(wd);

        SetEquipSpriteRow(slot, wd, isLead);
    }

    // 여기에서 card disp 만 이용할 때 쓰는 3개의 함수를 순서대로 호출 init weapon card dsplay, init sprite row, set equipcard display
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

    // 업그레이드가 일어나면 pause panel의 weapon 혹은 item 레벨 업데이트 함수들을 호출함.
    public void UpdateWeaponLevel(string _weaponName, int _levelToUpdate, bool _isSynergy)
    {
        for (int i = 0; i < weaponCards.Count; i++)
        {
            if (pauseCardDisps.ContainsKey(_weaponName))
            {
                pauseCardDisps[_weaponName].UpdatePauseCardLevel(_levelToUpdate, true, _isSynergy);
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

    public void EnableBG(bool EnableBG)
    {
        BG.SetActive(EnableBG);
    }
}