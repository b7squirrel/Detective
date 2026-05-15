using System.Collections.Generic;
using UnityEngine;

public class StartingDataContainer : MonoBehaviour
{
    // ✅ 추가: 항상 최신 인스턴스를 가리키는 static 참조
    public static StartingDataContainer instance;
    OriAttribute leadAttr = new OriAttribute(0, 0);
    WeaponData leadWd;
    List<Item> itemDatas = new();
    int essectialEquipmentIndex;

    int skillName;
    CardData playerCardData;
    SetBonusDefinition setBonus;
    int setBonusGrade;
    public SetBonusDefinition GetSetBonus() => setBonus;
    public int GetSetBonusGrade() => setBonusGrade;

    [Header("Debugging")]
    [SerializeField] int hp = 0;
    [SerializeField] int atk = 0;
    [SerializeField] List<Item> itemDatasDebug = new();
    [SerializeField] int essectialIndexDebug;

    void Awake()
    {
        // ✅ 기존 인스턴스가 있으면 오래된 것을 파괴하고 새것으로 교체
        if (instance != null && instance != this)
        {
            Debug.Log("[StartingDataContainer] 구 인스턴스 파괴, 신규로 교체");
            Destroy(instance.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);  // DontDestroy 컴포넌트 제거 후 여기서 처리
    }

    public void SetLead(CardData lead, OriAttribute leadAttr)
    {
        itemDatas.Clear();
        itemDatasDebug.Clear();
        essectialEquipmentIndex = -1;
        essectialIndexDebug = -1;

        this.leadAttr = leadAttr;
        // debugging
        hp = this.leadAttr.Hp;
        atk = this.leadAttr.Atk;

        CardsDictionary cardDic = FindAnyObjectByType<CardsDictionary>();
        CardDataManager cardDataManager = FindAnyObjectByType<CardDataManager>();
        CardList cardList = FindObjectOfType<CardList>();

        leadWd = cardDic.GetWeaponItemData(lead).weaponData;

        // 장비 데이터 넘기기
        EquipmentCard[] equipCard = cardList.GetEquipmentsCardData(lead);
        for (int i = 0; i < 4; i++)
        {
            if (equipCard[i] == null)
            {
                itemDatas.Add(null);
                itemDatasDebug.Add(null);
                continue;
            }
            itemDatas.Add(cardDic.GetWeaponItemData(equipCard[i].CardData).itemData);
            itemDatasDebug.Add(cardDic.GetWeaponItemData(equipCard[i].CardData).itemData);
            if (equipCard[i].CardData.EssentialEquip == EssentialEquip.Essential.ToString())
            {
                essectialEquipmentIndex = i;
                essectialIndexDebug = i;
            }
        }
        Debug.Log($"[StartingDataContainer] 최종 essentialIndex: {essectialEquipmentIndex}");

        // 세자리 수로 스킬을 구분
        skillName = lead.PassiveSkill * 100
                    + lead.Grade * 10
                    + lead.EvoStage;

        playerCardData = lead;

        SetBonusChecker setBonusChecker = FindObjectOfType<SetBonusChecker>();
        if (setBonusChecker != null)
        {
            setBonus = setBonusChecker.GetSetBonus(lead);
            setBonusGrade = setBonusChecker.GetLowestGrade(lead); // ← 추가
            Debug.Log($"[StartingDataContainer] 세트 보너스: {(setBonus != null ? setBonus.bonusDescription : "없음")}, 등급: {setBonusGrade}");
        }

        // SetLead() 마지막 부분에 추가
        Debug.Log($"[StartingDataContainer] 최종 Hp={this.leadAttr.Hp}, Atk={this.leadAttr.Atk}");
    }
    public void DestroyStartingDataContainer()
    {
        Destroy(gameObject);
    }

    // Player loads the following information after starting the game
    public OriAttribute GetLeadAttr() => this.leadAttr;
    public WeaponData GetLeadWeaponData() => this.leadWd;
    public List<Item> GetItemDatas() => this.itemDatas;
    public int GetEssectialIndex() => this.essectialEquipmentIndex;
    public int GetSkillName() => this.skillName;
    public CardData GetPlayerCardData() => this.playerCardData;
}
