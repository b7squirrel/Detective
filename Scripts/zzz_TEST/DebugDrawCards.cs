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
    int weaponEvoIndex, itemEvo;
    string[] cardGrade = { "일반", "희귀", "고급", "전설", "신화" };
    int weaponSkillIndex;

    [Header("UI")]
    [SerializeField] TMPro.TextMeshProUGUI weaponNumText;
    [SerializeField] TMPro.TextMeshProUGUI itemNumText;
    [SerializeField] TMPro.TextMeshProUGUI weaponGradeText;
    [SerializeField] TMPro.TextMeshProUGUI itemGradeText;
    [SerializeField] TMPro.TextMeshProUGUI weaponSkillText;
    [SerializeField] TMPro.TextMeshProUGUI weaponEVOText;

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

        SetWeaponEvo(0);
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
        int addition = steps > 0 ? 1 : -1; // 0에서 4 범위 안에 있다면 1 혹은 -1씩 이동
        if (steps == 0) addition = 0; // 초기화 때 변화를 원치 않을 경우 0을 입력

        // 0~4 범위로 제한하고 순환되도록 설정
        if (weaponGrade < 0)
        {
            weaponGrade = 4;
            addition = 4; // 4칸 올려서 해당 오리의 최고 등급으로 이동하도록 해서 순회하는 것처럼 보이게 하기
        }
        if (weaponGrade > 4)
        {
            weaponGrade = 0;
            addition = -4; // 4칸 내려서 해당 오리의 최고 등급으로 이동하도록 해서 순회하는 것처럼 보이게 하기
        }

        weaponGradeText.text = cardGrade[weaponGrade];
        weaponGradeText.color = MyGrade.GradeColors[weaponGrade]; // 등급별 색상 적용

        // 등급이 올라가거나 내려가면 한 칸씩 이동. 범위를 벗어나면 순회
        SetWeaponCard(addition);
    }
    public void SetItemGrade(int steps)
    {
        itemGrade += steps;
        int addition = steps > 0 ? 1 : -1; // 0에서 4 범위 안에 있다면 1 혹은 -1씩 이동
        if (steps == 0) addition = 0; // 초기화 때 변화를 원치 않을 경우 0을 입력

        // 0~4 범위로 제한하고 순환되도록 설정
        if (itemGrade < 0)
        {
            itemGrade = 4;
            addition = 4; // 4칸 올려서 해당 오리의 최고 등급으로 이동하도록 해서 순회하는 것처럼 보이게 하기
        }
        if (itemGrade > 4)
        {
            itemGrade = 0;
            addition = -4; // 4칸 내려서 해당 오리의 최고 등급으로 이동하도록 해서 순회하는 것처럼 보이게 하기
        }

        itemGradeText.text = cardGrade[itemGrade];
        itemGradeText.color = MyGrade.GradeColors[itemGrade]; // 등급별 색상 적용

        // 등급이 올라가거나 내려가면 한 칸씩 이동. 범위를 벗어나면 순회
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
    #endregion

    #region EVO 설정
    public void SetWeaponEvo(int steps)
    {
        weaponEvoIndex = steps == 0 ? weaponEvoIndex = 0 : weaponEvoIndex + steps; // 초기화를 위해
        if (weaponEvoIndex < 0) weaponEvoIndex = 2;
        if (weaponEvoIndex > 2) weaponEvoIndex = 0;

        weaponEVOText.text = (weaponEvoIndex + 1).ToString();
        SetWeaponCard(0);
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
        if (weaponIndex < 0) weaponIndex = weaponPools.Count - 1 - 4; // 최소값 아래로 내려가면 최대값으로 가서 루프가 되도록 (다시 4를 빼서 일반 그레이드로 가도록)
        if (weaponIndex > weaponPools.Count - 1) weaponIndex = 0; // 최대값을 넘어가면 0으로 가서 루프가 되도록

        weaponPools[weaponIndex].EvoStage = weaponEvoIndex;
        InitWeaponSlot(cardDictionary.GetWeaponItemData(weaponPools[weaponIndex]).weaponData, weaponPools[weaponIndex]);
    }

    public void SetItemCard(int steps)
    {
        itemIndex += steps;
        if (itemIndex < 0) itemIndex = itemPools.Count - 1 - 4; // 최소값 아래로 내려가면 최대값으로 가서 루프가 되도록 (다시 4를 빼서 일반 그레이드로 가도록)
        if (itemIndex > itemPools.Count - 1) itemIndex = 0; // 최대값을 넘어가면 0으로 가서 루프가 되도록
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
        gachaSystem.DrawSpecificCard("Item", itemIndex, weaponGrade, itemNum, 0, 0); // 아이템은 스킬이 없으니 그냥 0을 입력
    }
    #endregion
}
