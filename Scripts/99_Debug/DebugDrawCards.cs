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
    int weaponGrade, itemGrade;
    int weaponEvoIndex, itemEvoIndex;
    string[] cardGrade = { "일반", "희귀", "고급", "신화" };
    int weaponSkillIndex, itemSkillIndex;

    [Header("UI")]
    [SerializeField] TMPro.TextMeshProUGUI weaponNumText;
    [SerializeField] TMPro.TextMeshProUGUI itemNumText;
    [SerializeField] TMPro.TextMeshProUGUI weaponGradeText;
    [SerializeField] TMPro.TextMeshProUGUI itemGradeText;
    [SerializeField] TMPro.TextMeshProUGUI weaponSkillText;
    [SerializeField] TMPro.TextMeshProUGUI itemSkillText;
    [SerializeField] TMPro.TextMeshProUGUI weaponEvoText;
    [SerializeField] TMPro.TextMeshProUGUI itemEvoText;

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

        SetWeaponGrade(0);
        SetItemGrade(0);

        SetWeaponSkill(0);
        SetItemSkill(0);

        SetWeaponEvo(0);
        SetItemEvo(0);
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

    #region 등급 설정
    public void SetWeaponGrade(int steps)
    {
        weaponGrade += steps;
        int addition = steps > 0 ? 1 : -1;
        if (steps == 0) addition = 0;

        if (weaponGrade < 0)
        {
            weaponGrade = StaticValues.MaxGrade - 1;
            addition = StaticValues.MaxGrade - 1;
        }
        if (weaponGrade > StaticValues.MaxGrade - 1)
        {
            weaponGrade = 0;
            addition = -(StaticValues.MaxGrade - 1);
        }

        weaponGradeText.text = cardGrade[weaponGrade];
        weaponGradeText.color = MyGrade.GradeColors[weaponGrade];

        SetWeaponCard(addition);
    }
    public void SetItemGrade(int steps)
    {
        itemGrade += steps;
        int addition = steps > 0 ? 1 : -1;
        if (steps == 0) addition = 0;

        if (itemGrade < 0)
        {
            itemGrade = StaticValues.MaxGrade - 1;
            addition = StaticValues.MaxGrade - 1;
        }
        if (itemGrade > StaticValues.MaxGrade - 1)
        {
            itemGrade = 0;
            addition = -(StaticValues.MaxGrade - 1);
        }

        itemGradeText.text = cardGrade[itemGrade];
        itemGradeText.color = MyGrade.GradeColors[itemGrade];

        SetItemCard(addition);
    }
    #endregion

    #region 스킬 설정
    public void SetWeaponSkill(int steps)
    {
        weaponSkillIndex = steps == 0 ? weaponSkillIndex = 0 : weaponSkillIndex + steps; // 초기화를 위해
        if (weaponSkillIndex < 0) weaponSkillIndex = 4;
        if (weaponSkillIndex > 4) weaponSkillIndex = 0;

        weaponSkillText.text = Skills.SkillNames[weaponSkillIndex];
        Debug.Log($"스킬 인덱스 = {weaponSkillIndex}");
    }

    public void SetItemSkill(int steps)
    {
        itemSkillIndex = steps == 0 ? itemSkillIndex = 0 : itemSkillIndex + steps; // 초기화를 위해
        if (itemSkillIndex < 0) itemSkillIndex = 3;
        if (itemSkillIndex > 3) itemSkillIndex = 0;

        itemSkillText.text = Skills.itemSkillNames[itemSkillIndex];
        Debug.Log($"아이템 스킬 인덱스 = {itemSkillIndex}");
    }
    #endregion

    #region EVO 설정
    public void SetWeaponEvo(int steps)
    {
        weaponEvoIndex = steps == 0 ? weaponEvoIndex = 0 : weaponEvoIndex + steps; // 초기화를 위해
        if (weaponEvoIndex < 0) weaponEvoIndex = 2;
        if (weaponEvoIndex > 2) weaponEvoIndex = 0;

        weaponEvoText.text = (weaponEvoIndex + 1).ToString();
        SetWeaponCard(0);
    }

    public void SetItemEvo(int steps)
    {
        itemEvoIndex = steps == 0 ? itemEvoIndex = 0 : itemEvoIndex + steps; // 초기화를 위해
        if (itemEvoIndex < 0) itemEvoIndex = 2;
        if (itemEvoIndex > 2) itemEvoIndex = 0;

        itemEvoText.text = (itemEvoIndex + 1).ToString();
        SetItemCard(0);
    }
    #endregion

    #region 카드 개수 설정
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
    #endregion

    #region 오리 종류 설정
    public void SetWeaponCard(int steps)
    {
        weaponIndex += steps;
        // 등급이 StaticValues.MaxGrade(현재 4)단계이므로, 한 무기당 블록 크기는 (MaxGrade - 1)만큼 빼서
        // 최소값 아래로 내려갔을 때 이전 무기의 '일반' 등급 위치로 정렬되도록 한다.
        if (weaponIndex < 0) weaponIndex = weaponPools.Count - 1 - (StaticValues.MaxGrade - 1); // 최소값 아래로 내려가면 최대값으로 가서 루프가 되도록 (다시 MaxGrade-1을 빼서 일반 그레이드로 가도록)
        if (weaponIndex > weaponPools.Count - 1) weaponIndex = 0; // 최대값을 넘어가면 0으로 가서 루프가 되도록

        weaponPools[weaponIndex].EvoStage = weaponEvoIndex;
        InitWeaponSlot(cardDictionary.GetWeaponItemData(weaponPools[weaponIndex]).weaponData, weaponPools[weaponIndex]);
    }

    public void SetItemCard(int steps)
    {
        itemIndex += steps;
        // 등급이 StaticValues.MaxGrade(현재 4)단계이므로, 한 아이템당 블록 크기는 (MaxGrade - 1)만큼 빼서
        // 최소값 아래로 내려갔을 때 이전 아이템의 '일반' 등급 위치로 정렬되도록 한다.
        if (itemIndex < 0) itemIndex = itemPools.Count - 1 - (StaticValues.MaxGrade - 1); // 최소값 아래로 내려가면 최대값으로 가서 루프가 되도록 (다시 MaxGrade-1을 빼서 일반 그레이드로 가도록)
        if (itemIndex > itemPools.Count - 1) itemIndex = 0; // 최대값을 넘어가면 0으로 가서 루프가 되도록

        itemPools[itemIndex].EvoStage = itemEvoIndex;
        InitItemSlot(cardDictionary.GetWeaponItemData(itemPools[itemIndex]).itemData, itemPools[itemIndex]);
    }
    #endregion

    #region 특정 카드 뽑기
    public void DrawWeaponCard()
    {
        gachaSystem.DrawSpecificCard("Weapon", weaponIndex, weaponGrade, weaponNum, weaponSkillIndex, weaponEvoIndex);
    }
    public void DrawItemCard()
    {
        gachaSystem.DrawSpecificCard("Item", itemIndex, itemGrade, itemNum, itemSkillIndex, itemEvoIndex);
        Debug.Log($"[DebugDrawCard] 아이템 스킬 = {itemSkillIndex}");
    }
    #endregion
}