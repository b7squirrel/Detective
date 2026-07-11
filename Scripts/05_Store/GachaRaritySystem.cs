using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaRaritySystem : MonoBehaviour
{
    public int GetRandomRarity(string gachaTableId, bool isGuaranteedSlot)
    {
        if (GachaRarityTable.Instance == null)
        {
            Logger.LogError("[GachaRaritySystem] RarityTable이 없습니다. 기본값 반환.");
            return 1;
        }

        string slotType = isGuaranteedSlot ? "Guarantee" : "Normal";
        var probabilities = GachaRarityTable.Instance.GetProbabilities(gachaTableId, slotType);

        if (probabilities == null || probabilities.Count == 0)
        {
            Logger.LogError($"[GachaRaritySystem] 확률 테이블을 찾을 수 없습니다: {gachaTableId}/{slotType}");
            return 1;
        }

        int selectedRarity = WeightedRandomPick(probabilities);
        Logger.Log($"[GachaRaritySystem] {gachaTableId}/{slotType} → Grade {selectedRarity} ({GetRarityName(selectedRarity)})");
        return selectedRarity;
    }

    private int WeightedRandomPick(Dictionary<int, int> probabilities)
    {
        int totalWeight = 0;
        foreach (var prob in probabilities.Values)
        {
            totalWeight += prob;
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        int currentWeight = 0;
        foreach (var kvp in probabilities)
        {
            currentWeight += kvp.Value;
            if (randomValue < currentWeight)
            {
                return kvp.Key;
            }
        }

        return probabilities.Keys.First();
    }

    private string GetRarityName(int rarity)
    {
        switch (rarity)
        {
            case 0: return "Common";
            case 1: return "Rare";
            case 2: return "Epic";
            case 3: return "Legendary";
            case 4: return "Mythic";
            default: return "Unknown";
        }
    }
}