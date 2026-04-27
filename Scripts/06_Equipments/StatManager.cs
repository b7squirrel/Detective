using UnityEngine;

public class OriAttribute
{
    public OriAttribute(int atk, int hp)
    {
        Atk = atk;
        Hp = hp;
    }
    public int Atk, Hp;
}

/// <summary>
/// 오리, 아이템 구분해서 업그레이드 하면서 스탯 업데이트
/// </summary>
public class StatManager : MonoBehaviour
{
    [SerializeField] CardDataManager cardDataManager;
    [SerializeField] EquipmentSlotsManager equipmentSlotManager;
    [SerializeField] EquipInfoPanel equipInfoPanel;
    [SerializeField] CardList cardList;
    [SerializeField] StartingDataContainer statContainer;
    OriAttribute leadAttribute;

    readonly int[] oriHpPerLevel = { 100, 200, 350, 600, 1000 };
    readonly int[] oriAtkPerLevel = { 1, 2, 3, 5, 8 };
    readonly int[] itemHpPerLevel = { 4, 6, 8, 12, 17 };
    readonly int[] itemAtkPerLevel = { 1, 2, 3, 5, 8 };

    /// <summary>
    /// 오리, 아이템 구분해서 레벨업, 스탯업
    /// </summary>
    public void LevelUp(CardData _cardData)
    {
        int level = _cardData.Level;
        int newHp = _cardData.Hp;
        int newAtk = _cardData.Atk;

        if (_cardData.EquipmentType == "Ori")
        {
            newHp += oriHpPerLevel[_cardData.Grade];
            newAtk += oriAtkPerLevel[_cardData.Grade];
        }
        else
        {
            if (_cardData.Hp > 0)
                newHp += itemHpPerLevel[_cardData.Grade];
            if (_cardData.Atk > 0)
            {
                int atkIncrease = itemAtkPerLevel[_cardData.Grade];
                newAtk += atkIncrease;
                Character character = FindObjectOfType<Character>();
                if (character != null && atkIncrease > 0)
                    character.AddDamageBonus(atkIncrease);
            }
        }

        level++;
        UpdateStat(_cardData, level, newHp, newAtk);
    }
    public void ApplyEvoStats(CardData cardData, int evoStage)
    {
        if (evoStage <= 0) return;

        int levelsPerEvo = StaticValues.MaxLevel - 1;

        for (int e = 0; e < evoStage; e++)
        {
            if (cardData.EquipmentType == "Ori")
            {
                cardData.Hp += oriHpPerLevel[cardData.Grade] * levelsPerEvo;
                cardData.Atk += oriAtkPerLevel[cardData.Grade] * levelsPerEvo;
            }
            else
            {
                if (cardData.Hp > 0)
                    cardData.Hp += itemHpPerLevel[cardData.Grade] * levelsPerEvo;
                if (cardData.Atk > 0)
                    cardData.Atk += itemAtkPerLevel[cardData.Grade] * levelsPerEvo;
            }
        }

        Logger.Log($"[StatManager] EvoStats 적용: {cardData.Name} Evo{evoStage} HP:{cardData.Hp} ATK:{cardData.Atk}");
    }
    void UpdateStat(CardData _cardData, int _level, int _hp, int _atk)
    {
        cardDataManager.UpgradeCardData(_cardData, _level, _hp, _atk);

        if (_cardData.EquipmentType == EquipmentType.Ori.ToString()) // 오리라면
        {
            // 오리 레벨, 속성 UI 업데이트 - 이걸 equipment panel manager의 UpgradeCardOnDisplay에서 해주고 있음
        }
        // 필수 무기라면 Atk을 info UI에 보여줌
        else if (_cardData.EssentialEquip == EssentialEquip.Essential.ToString())
        {
            equipInfoPanel.UpdatePanel(_level, _atk);
        }
        else // 방어구 카드라면
        {
            equipInfoPanel.UpdatePanel(_level, _hp);
        }
    }
    /// <summary>
    /// 리드 오리 공격력, 체력
    /// </summary>
    public OriAttribute GetLeadAttribute(CardData oriCard)
    {
        CharCard lead = cardList.FindCharCard(oriCard);
        EquipmentCard[] equipments = lead.equipmentCards;

        int totalAtk = oriCard.Atk;
        int totalHp = oriCard.Hp;

        if (lead != null)
        {
            for (int i = 0; i < equipments.Length; i++)
            {
                if (equipments[i] == null) continue;

                // ⭐ Essential/방어구 구분 없이 Atk, Hp 모두 누적 합산
                totalAtk += equipments[i].CardData.Atk;
                totalHp += equipments[i].CardData.Hp;
            }
        }

        Logger.Log(lead.CardData.Name + "의 HP = " + totalHp + " Atk = " + totalAtk);
        return new OriAttribute(totalAtk, totalHp);
    }
}
