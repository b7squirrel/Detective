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

    // ⭐ 추가: 동료 오리 (최대 4마리)
    List<CompanionData> companions = new();

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

    // ⭐ 추가: 로비에서 고른 동료 카드 최대 4장을 받아 CompanionData 리스트 구성
    // 각 카드의 실제 장착 장비(스프라이트용)와 장비 포함 Atk/Hp(스탯용)를 함께 계산해둔다
    public void SetCompanions(List<CardData> companionCards)
    {
        companions.Clear();

        if (companionCards == null || companionCards.Count == 0) return;

        CardsDictionary cardDic = FindAnyObjectByType<CardsDictionary>();
        CardList cardList = FindObjectOfType<CardList>();

        for (int i = 0; i < companionCards.Count && i < 4; i++)
        {
            CardData card = companionCards[i];
            if (card == null) continue;

            CompanionData companion = new CompanionData();
            companion.cardData = card;
            companion.weaponData = cardDic.GetWeaponItemData(card).weaponData;
            companion.equippedItems = new List<Item>();

            int totalAtk = card.Atk;
            int totalHp = card.Hp;

            EquipmentCard[] equipCard = cardList.GetEquipmentsCardData(card);
            for (int j = 0; j < 4; j++)
            {
                if (equipCard[j] == null)
                {
                    companion.equippedItems.Add(null);
                    continue;
                }

                companion.equippedItems.Add(cardDic.GetWeaponItemData(equipCard[j].CardData).itemData);
                totalAtk += equipCard[j].CardData.Atk;
                totalHp += equipCard[j].CardData.Hp;
            }

            companion.totalAtk = totalAtk;
            companion.totalHp = totalHp;

            companions.Add(companion);
            Debug.Log($"[StartingDataContainer] 동료 등록: {card.Name} (Atk:{totalAtk}, Hp:{totalHp})");
        }
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

    // ⭐ 추가: 동료 관련 조회 함수
    public List<CompanionData> GetCompanions() => this.companions;

    // 로비 필드 피커에서 이미 스쿼드에 있는 종을 숨기기 위한 이름 집합 (리드 + 동료 전체)
    public HashSet<string> GetSquadWeaponNames()
    {
        HashSet<string> names = new HashSet<string>();

        if (leadWd != null) names.Add(leadWd.Name);

        for (int i = 0; i < companions.Count; i++)
        {
            if (companions[i].weaponData != null)
                names.Add(companions[i].weaponData.Name);
        }

        return names;
    }

    // 동료들의 Atk 합산 (카드 자체 Atk + 장착 장비 Atk) - Character.DamageBonus에 그대로 더해짐
    public int GetCompanionAtkTotal()
    {
        int total = 0;
        for (int i = 0; i < companions.Count; i++)
        {
            total += companions[i].totalAtk;
        }
        return total;
    }

    // 동료들의 Hp 합산 (카드 자체 Hp + 장착 장비 Hp) - Character에서 Armor로 환산되어 반영됨
    public int GetCompanionHpTotal()
    {
        int total = 0;
        for (int i = 0; i < companions.Count; i++)
        {
            total += companions[i].totalHp;
        }
        return total;
    }
}