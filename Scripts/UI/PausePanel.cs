using System.Collections.Generic;
using UnityEngine;

public class PausePanel : MonoBehaviour
{
    [SerializeField] GameObject cardSlot;
    [SerializeField] GameObject itemSlot;
    [SerializeField] Transform weaponContents;
    [SerializeField] Transform itemContents;
    // [SerializeField] GameObject BG;

    List<CardDisp> weaponCards;
    List<PauseCardDisp> itemCards;
    Dictionary<string, PauseCardDisp> pauseCardDisps = new Dictionary<string, PauseCardDisp>();

    // ⭐ 변경: 스쿼드 동료의 실제 장착 아이템을 전달받기 위해 companionEquippedItems 파라미터 추가
    // 로비에서 선택한 스쿼드 동료라면 실제 장착 카드의 아이템을, 필드에서 얻은 일반 동료라면 기존처럼 defaultItems 사용
    public void InitWeaponSlot(WeaponData wd, bool isLead, List<Item> companionEquippedItems = null)
    {
        if (weaponCards == null) weaponCards = new();

        GameObject wSlot = Instantiate(cardSlot, weaponContents.transform);
        CardSlot slot = wSlot.GetComponent<CardSlot>();
        weaponCards.Add(slot.GetComponent<CardDisp>());

        PauseCardDisp pauseDisp = slot.GetComponent<PauseCardDisp>();
        pauseCardDisps.Add(wd.Name, pauseDisp);

        Debug.Log($"Is Lead = {isLead}");
        pauseDisp.EnableLeadTag(isLead);

        pauseDisp.InitWeaponCardDisplay(wd);

        SetEquipSpriteRow(slot, wd, isLead, companionEquippedItems);

        // // 시너지 아이템이 있다면 선으로 연결
    }

    public void InitItemSlot(Item _item)
    {
        if (itemCards == null) itemCards = new();

        GameObject iSlot = Instantiate(itemSlot, itemContents.transform);
        itemCards.Add(iSlot.GetComponent<PauseCardDisp>());

        PauseCardDisp pauseCardDisp = iSlot.GetComponent<PauseCardDisp>();
        CardDisp cardDisp = iSlot.GetComponent<CardDisp>();
        pauseCardDisp.InitItemCardDisplay(_item);
        cardDisp.InitItemCardDisplay(_item, null, false);
    }

    public void UpdateWeaponLevel(string _weaponName, int _level, bool _isSynergy)
    {
        if (pauseCardDisps.TryGetValue(_weaponName, out var disp))
            disp.UpdatePauseCardLevel(_level, true, _isSynergy);
    }

    public void UpdateItemLevel(Item _item)
    {
        foreach (var item in itemCards)
        {
            if (item.Name == _item.Name)
            {
                item.UpdatePauseCardLevel(_item.stats.currentLevel, false, false);
                return;
            }
        }
    }

    // public void EnableBG(bool enable) => BG.SetActive(enable);

    // weapon data의 시너지를 이루는 아이템이 있는지 검색
    RectTransform GetSynergyItemPos(WeaponData wd)
    {
        Item coupleItem = GameManager.instance.character
            .GetComponent<PassiveItems>()
            .GetSynergyCouple(wd.SynergyWeapon);

        if (coupleItem == null) return null;

        foreach (var item in itemCards)
        {
            if (item.Name == coupleItem.Name)
            {
                return item.GetSynergyInPoint();
            }
        }
        return null;
    }

    // ⭐ 변경: companionEquippedItems 파라미터 추가
    void SetEquipSpriteRow(CardSlot targetSlot, WeaponData wd, bool isLead, List<Item> companionEquippedItems)
    {
        CardDisp cardDisp = targetSlot.GetComponent<CardDisp>();
        cardDisp.InitWeaponCardDisplay(wd, null);
        cardDisp.InitSpriteRow();

        for (int i = 0; i < 4; i++)
        {
            Item item;

            if (isLead)
            {
                item = GameManager.instance.startingDataContainer.GetItemDatas()[i];
            }
            else
            {
                // ⭐ 변경: 스쿼드 동료라면 실제 장착 아이템, 없으면(필드 획득 등) 기존처럼 defaultItems로 폴백
                item = (companionEquippedItems != null && i < companionEquippedItems.Count)
                    ? companionEquippedItems[i]
                    : null;

                if (item == null) item = wd.defaultItems[i];
            }

            if (item == null)
            {
                cardDisp.SetEquipCardDisplay(i, null, false, Vector2.zero);
                continue;
            }
            Vector2 offset = item.needToOffset ? item.posHead : Vector2.zero;
            cardDisp.SetEquipCardDisplay(i, item.spriteRow, item.needToOffset, offset);
        }
    }
}