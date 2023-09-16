using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatUpgradeManager : MonoBehaviour
{

    // public void AddExperience(int expAmount)
    // {
    //     experience += expAmount;
    //     CheckLevelUp();
    //     experienceBar.UpdateExperienceSlider(experience, To_Level_Up);
    // }
    // int GetLevel(CardData _cardData)
    // {
    //     int level = Mathf.Floor(_cardData.Exp / 10000)
    // }
    // void CheckLevelUp(CardData _cardData)
    // {
    //     int.TryParse(_cardData.Exp, out int cardExp);
    //     if (cardExp >= To_Level_Up(_cardData))
    //     {
    //         LevelUp();
    //     }
    // }
    // int To_Level_Up(CardData _cardData)
    // {
    //     int level = GetLevel(_cardData);
    //     return level * 1000;
    // }

    // void LevelUp()
    // {
    //     if (selectedUpgrads == null)
    //     {
    //         selectedUpgrads = new List<UpgradeData>();
    //     }
    //     selectedUpgrads.Clear();
    //     selectedUpgrads.AddRange(GetRandomUpgrades());
    //     experience -= To_Level_Up;
    //     level++;
    //     experienceBar.SetLevelText(level);

    //     if (NoMoreUpgrade)
    //         return;

    //     upgradeManager.OpenPanel(selectedUpgrads);
    //     expBarAnim.ExpBarEffect();
    // }
    // public void UpdateStat(CardData _cardData)
    // {

    // }
}
