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

    CardsDictionary cardDictionary;
    GachaSystem gachaSystem;

    // 인덱스 및 개수
    int weaponNum, itemNum;
    int weaponIndex, itemIndex;

    [Header("UI")]
    [SerializeField] TMPro.TextMeshProUGUI weaponNumText;
    [SerializeField] TMPro.TextMeshProUGUI itemNumText;

    void Start()
    {
        cardDictionary = FindObjectOfType<CardsDictionary>();
        gachaSystem = FindObjectOfType<GachaSystem>();

        weaponPools = new ReadCardData().GetCardsList(weaponPoolDatabase);
        itemPools = new ReadCardData().GetCardsList(itemPoolDatabase);

        // index 0 항목 카드에 보여주기
        InitWeaponSlot(cardDictionary.GetWeaponItemData(weaponPools[0]).weaponData, weaponPools[0]);
        InitItemSlot(cardDictionary.GetWeaponItemData(itemPools[0]).itemData, itemPools[0]);

        // UI에 현재 개수 업데이트. 디폴트 0이므로 1로 시작하게 됨
        SetWeaponNum(true);
        SetItemNum(true);
    }

    public void InitWeaponSlot(WeaponData wd, CardData cardData)
    {
        SetEquipSpriteRow(weaponCardSlot, wd, cardData);
    }

    public void InitItemSlot(Item _item, CardData cardData)
    {
        CardDisp cardDisp = itemCardSlot.GetComponent<CardDisp>();
        cardDisp.InitItemCardDisplay(_item, cardData, false);
    }

    void SetEquipSpriteRow(CardSlot targetSlot, WeaponData wd, CardData cardData)
    {
        CardDisp cardDisp = targetSlot.GetComponent<CardDisp>();
        cardDisp.InitWeaponCardDisplay(wd, cardData);
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

    #region UI
    public void SetWeaponNum(bool addition)
    {
        weaponNum = addition ? weaponNum + 1 : weaponNum - 1;
        if (weaponNum <= 0) weaponNum = 1;
        weaponNumText.text = weaponNum.ToString();
    }

    public void SetItemNum(bool addition)
    {
        itemNum = addition ? itemNum + 1 : itemNum - 1;
        if (itemNum <= 0) itemNum = 1;
        itemNumText.text = itemNum.ToString();
    }

    public void SetWeaponCard(bool addition)
    {
        weaponIndex = addition ? weaponIndex + 1 : weaponIndex - 1;
        if (weaponIndex < 0) weaponIndex = weaponPools.Count - 1; // 최소값 아래로 내려가면 최대값으로 가서 루프가 되도록
        if (weaponIndex > weaponPools.Count - 1) weaponIndex = 0; // 최대값을 넘어가면 0으로 가서 루프가 되도록
        InitWeaponSlot(cardDictionary.GetWeaponItemData(weaponPools[weaponIndex]).weaponData, weaponPools[weaponIndex]);
    }

    public void SetItemCard(bool addition)
    {
        itemIndex = addition ? itemIndex + 1 : itemIndex - 1;
        if (itemIndex < 0) itemIndex = itemPools.Count - 1;
        if (itemIndex > itemPools.Count - 1) itemIndex = 0;
        InitItemSlot(cardDictionary.GetWeaponItemData(itemPools[itemIndex]).itemData, itemPools[itemIndex]);
    }

    public void DrawWeaponCard()
    {
        gachaSystem.DrawSpecificCard("Weapon", weaponIndex, weaponNum);
    }
    public void DrawItemCard()
    {
        gachaSystem.DrawSpecificCard("Item", itemIndex, itemNum);
    }
    #endregion
}
