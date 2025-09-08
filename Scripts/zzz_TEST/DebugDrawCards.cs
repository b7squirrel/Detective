using System.Collections.Generic;
using UnityEngine;

public class DebugDrawCards : MonoBehaviour
{
    [Header("카드 데이터들")]
    [SerializeField] TextAsset weaponPoolDatabase;
    [SerializeField] TextAsset itemPoolDatabase;
    List<CardData> weaponPools;
    List<CardData> itemPools;

    [Header("디스플레이 관련")]
    [SerializeField] CardSlot weaponCardSlot;
    [SerializeField] CardSlot itemCardSlot;

    List<CardDisp> weaponCards;
    List<PauseCardDisp> itemCards;
    CardsDictionary cardDictionary;

    // [SerializeField] Image 
    void Start()
    {
        cardDictionary = FindObjectOfType<CardsDictionary>();

        weaponPools = new ReadCardData().GetCardsList(weaponPoolDatabase);
        itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);

        // index 0 항목 카드에 보여주기
        InitWeaponSlot(cardDictionary.GetWeaponItemData(weaponPools[0]).weaponData);
        InitItemSlot(cardDictionary.GetWeaponItemData(itemPools[0]).itemData);
    }

    public void InitWeaponSlot(WeaponData wd)
    {
        if (weaponCards == null) weaponCards = new();

        weaponCards.Add(weaponCardSlot.GetComponent<CardDisp>());

        SetEquipSpriteRow(weaponCardSlot, wd);

        // // 시너지 아이템이 있다면 선으로 연결
    }

    public void InitItemSlot(Item _item)
    {
        if (itemCards == null) itemCards = new();

        itemCards.Add(itemCardSlot.GetComponent<PauseCardDisp>());

        CardDisp cardDisp = itemCardSlot.GetComponent<CardDisp>();
        cardDisp.InitItemCardDisplay(_item, null, false);
    }

    void SetEquipSpriteRow(CardSlot targetSlot, WeaponData wd)
    {
        CardDisp cardDisp = targetSlot.GetComponent<CardDisp>();
        cardDisp.InitWeaponCardDisplay(wd, null);
        cardDisp.InitSpriteRow();

        for (int i = 0; i < 4; i++)
        {
            Item item = wd.defaultItems[i];

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
