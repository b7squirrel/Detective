using UnityEngine;

public class StatManager : MonoBehaviour
{
    [SerializeField] CardDataManager cardDataManager;

    #region 스탯 업
    public void AddExperience(CardData _cardData)
    {
        int.TryParse(_cardData.Exp, out int cardExp);

        cardExp += 300; // Temp
        CheckLevelUp(_cardData, cardExp);
    }
    int GetCardLevel(int _exp, CardData _cardData)
    {
        int level = (int)Mathf.Floor(_exp / 10000); // Temp
        return level;
    }
    void CheckLevelUp(CardData _cardData, int _upgradedExp)
    {
        int level = GetCardLevel(_upgradedExp, _cardData);
        int ExpToLevelUp = GetExpToLevelUp(_upgradedExp, _cardData);

        if (_upgradedExp >= ExpToLevelUp)
        {
            LevelUp(_upgradedExp, ExpToLevelUp, level, _cardData);
        }
    }
    // 현재 레벨에서 필요한 경험치 구하기
    int GetExpToLevelUp(int _exp, CardData _cardData)
    {
        int level = GetCardLevel(_exp, _cardData);
        return level * 1000; // Temp
    }

    void LevelUp(int _Exp, int cardToLevelUp, int level, CardData _cardData)
    {
        _Exp -= cardToLevelUp;
        level++;

        int.TryParse(_cardData.Hp, out int newHp);
        int.TryParse(_cardData.Atk, out int newAtk);
        newHp += level * 100; // Temp
        newAtk += level * 100; // Temp

        UpdateStat(_cardData, _Exp, newHp, newAtk);
    }
    void UpdateStat(CardData _cardData, int _exp, int _hp, int _atk)
    {
        cardDataManager.UpgradeCardData(_cardData, _exp, _hp, _atk);
    }
    #endregion
}
