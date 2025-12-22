using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaRaritySystem : MonoBehaviour
{
    private GachaRarityTable rarityTable;

    void Awake()
    {
        rarityTable = GetComponent<GachaRarityTable>();
    }

    public int GetRandomRarity(string gachaTableId, bool isGuaranteedSlot)
    {
        if (rarityTable == null)
        {
            Logger.LogError("[GachaRaritySystem] RarityTable이 없습니다. 기본값 반환.");
            return 1;
        }

        string slotType = isGuaranteedSlot ? "Guarantee" : "Normal";
        var probabilities = rarityTable.GetProbabilities(gachaTableId, slotType);

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
        // ⭐ 총 가중치 계산 (10000 = 100%)
        int totalWeight = 0;
        foreach (var prob in probabilities.Values)
        {
            totalWeight += prob;
        }

        // ⭐ 디버깅용 로그 (필요시 주석 해제)
        // Logger.Log($"[GachaRaritySystem] Total Weight: {totalWeight}");

        // ⭐ 0 ~ totalWeight 사이의 랜덤 값
        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        // ⭐ 누적 확률로 선택
        int currentWeight = 0;
        foreach (var kvp in probabilities)
        {
            currentWeight += kvp.Value;
            if (randomValue < currentWeight)
            {
                return kvp.Key; // 등급 반환
            }
        }

        // ⭐ 만약 여기까지 왔다면 첫 번째 등급 반환 (fallback)
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