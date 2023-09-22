using UnityEngine;

public class StatManager : MonoBehaviour
{
    [SerializeField] CardDataManager cardDataManager;
    [SerializeField] EquipInfoPanel equipInfoPanel;

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
        else if(_cardData.EquipmentType == EquipmentType.Weapon.ToString()) // 무기 카드라면
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
        else if(_cardData.EquipmentType == EquipmentType.Weapon.ToString()) // 무기 카드라면
        {
            equipInfoPanel.UpdatePanel(_level, _atk);
        }
        else // 방어구 카드라면
        {
            equipInfoPanel.UpdatePanel(_level, _hp);
        }
    }
}
