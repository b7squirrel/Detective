using System.Collections.Generic;
using UnityEngine;

public class SetBonusChecker : MonoBehaviour
{
    [SerializeField] List<SetBonusDefinition> setDefinitions;

    public void CheckSetBonus(CharCard charCard)
    {
        string detectedSet = GetFullSetName(charCard);

        if (detectedSet != null)
        {
            SetBonusDefinition bonus = setDefinitions.Find(s => s.setName == detectedSet);
            if (bonus != null)
            {
                Logger.Log($"[세트 완성] {charCard.CardData.Name} → 세트 장비 : {bonus.bonusDescription}");
                // 나중에: ApplySetBonus(charCard, bonus);
            }
            else
            {
                Logger.Log($"[세트 완성] {charCard.CardData.Name} → 세트명 '{detectedSet}' (보너스 미정의)");
            }
        }
        else
        {
            Logger.Log($"[세트 없음] {charCard.CardData.Name}");
            // 나중에: RemoveSetBonus(charCard);
        }
    }

    string GetFullSetName(CharCard charCard)
    {
        string candidate = null;

        for (int i = 0; i < 4; i++)
        {
            var slot = charCard.equipmentCards[i];
            if (slot == null) return null;

            string setName = slot.CardData.SetName;
            if (string.IsNullOrEmpty(setName)) return null;

            if (candidate == null)
                candidate = setName;
            else if (setName != candidate)
                return null;
        }

        return candidate;
    }
}