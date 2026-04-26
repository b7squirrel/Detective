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

    public SetBonusDefinition GetSetBonus(CardData leadCardData)
    {
        // CardList에서 CharCard를 찾아서 세트 체크
        CardList cardList = FindObjectOfType<CardList>();
        if (cardList == null) return null;

        CharCard charCard = cardList.FindCharCard(leadCardData);
        if (charCard == null) return null;

        string detectedSet = GetFullSetName(charCard);
        if (detectedSet == null) return null;

        return setDefinitions.Find(s => s.setName == detectedSet);
    }

    public int GetLowestGrade(CardData leadCardData)
    {
        CardList cardList = FindObjectOfType<CardList>();
        if (cardList == null) return 0;

        CharCard charCard = cardList.FindCharCard(leadCardData);
        if (charCard == null) return 0;

        int lowestGrade = 4;
        for (int i = 0; i < 4; i++)
        {
            if (charCard.equipmentCards[i] == null) return 0;
            if (charCard.equipmentCards[i].CardData.Grade < lowestGrade)
                lowestGrade = charCard.equipmentCards[i].CardData.Grade;
        }

        Logger.Log($"[SetBonusChecker] 가장 낮은 등급: {lowestGrade}");
        return lowestGrade;
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