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

// 카드를 업그레이드 할 떄 스탯 업데이트
public class StatManager : MonoBehaviour
{
    [SerializeField] CardDataManager cardDataManager;
    [SerializeField] EquipInfoPanel equipInfoPanel;
    [SerializeField] CardList cardList;
    [SerializeField] StartingDataContainer statContainer;
    OriAttribute leadAttribute;

    public void LevelUp(CardData _cardData)
    {
        int level = new Convert().StringToInt(_cardData.Level);

        int.TryParse(_cardData.Hp, out int newHp);
        int.TryParse(_cardData.Atk, out int newAtk);

        if(_cardData.EquipmentType == "Ori") // 오리라면
        {
            newAtk += level * 100; // Temp
            newHp += level * 100; // Temp
        }
        else if(_cardData.EquipmentType == EquipmentType.Hand.ToString()) // 무기 카드라면
        {
            newAtk += level * 100; // Temp
        }
        else // 방어구 카드라면
        {
            newHp += level * 100; // Temp
        }

        level++;
        
        UpdateStat(_cardData, level.ToString(), newHp.ToString(), newAtk.ToString());
    }
    void UpdateStat(CardData _cardData, string _level, string _hp, string _atk)
    {
        cardDataManager.UpgradeCardData(_cardData, _level, _hp, _atk);

        if(_cardData.EquipmentType == "Ori") // 오리라면
        {
            // 오리 레벨, 속성 UI 업데이트
        }
        else if(_cardData.EquipmentType == EquipmentType.Hand.ToString()) // 무기 카드라면
        {
            equipInfoPanel.UpdatePanel(_level, _atk);
        }
        else // 방어구 카드라면
        {
            equipInfoPanel.UpdatePanel(_level, _hp);
        }
    }
    // 리드 오리 공격력, 체력
    public OriAttribute GetLeadAttribute(CardData oriCard)
    {
        CharCard lead = cardList.FindCharCard(oriCard);
        EquipmentCard[] equipments = lead.equipmentCards;

        int oriAtk = new Convert().StringToInt(oriCard.Atk);
        int oriHp = new Convert().StringToInt(oriCard.Hp);
        OriAttribute leadAttr = new OriAttribute(oriAtk, oriHp);

        if(lead != null)
        {
            for (int i = 0; i < equipments.Length; i++)
            {
                if (equipments[i] == null)
                    continue;

                int equipmentAtk = new Convert().StringToInt(equipments[i].CardData.Atk);
                int equipmentHp = new Convert().StringToInt(equipments[i].CardData.Hp);
                leadAttr = new OriAttribute(oriAtk + equipmentAtk, oriHp + equipmentHp);
            }
        }
        return leadAttr;
    }
}
