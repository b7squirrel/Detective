using System.Collections.Generic;
using UnityEngine;

public class GachaRaritySystem : MonoBehaviour
{
    private GachaRarityTable rarityTable;

    void Awake()
    {
        rarityTable = GetComponent<GachaRarityTable>();
        if (rarityTable == null)
        {
            Logger.LogError("[GachaRaritySystem] GachaRarityTable을 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 가챠 테이블 ID와 슬롯 타입에 따라 랜덤 등급 반환
    /// </summary>
    /// <param name="gachaTableId">가챠 테이블 ID (예: "single_duck", "ten_duck")</param>
    /// <param name="isGuaranteedSlot">확정 슬롯 여부 (10연차의 마지막 슬롯)</param>
    /// <returns>뽑힌 등급 (0=Common, 1=Rare, 2=Epic, 3=Legendary, 4=Mythic)</returns>
    public int GetRandomRarity(string gachaTableId, bool isGuaranteedSlot)
    {
        if (rarityTable == null)
        {
            Logger.LogError("[GachaRaritySystem] RarityTable이 없습니다. 기본값 반환.");
            return 1; // 기본값: Rare
        }

        // 확률 테이블 가져오기
        string slotType = isGuaranteedSlot ? "Guarantee" : "Normal";
        var probabilities = rarityTable.GetProbabilities(gachaTableId, slotType);

        if (probabilities == null || probabilities.Count == 0)
        {
            Logger.LogError($"[GachaRaritySystem] 확률 테이블을 찾을 수 없습니다: {gachaTableId}/{slotType}");
            return 1;
        }

        // 가중치 기반 랜덤 선택
        int selectedRarity = WeightedRandomPick(probabilities);
        
        Logger.Log($"[GachaRaritySystem] {gachaTableId}/{slotType} → Grade {selectedRarity} ({GetRarityName(selectedRarity)})");
        
        return selectedRarity;
    }

    /// <summary>
    /// 가중치 기반 랜덤 선택
    /// </summary>
    private int WeightedRandomPick(Dictionary<int, float> probabilities)
    {
        // 전체 확률 합계 계산
        float totalWeight = 0f;
        foreach (var prob in probabilities.Values)
        {
            totalWeight += prob;
        }

        // 0~totalWeight 사이 랜덤값
        float randomValue = Random.Range(0f, totalWeight);
        
        // 누적 확률로 선택
        float cumulativeWeight = 0f;
        foreach (var kvp in probabilities)
        {
            cumulativeWeight += kvp.Value;
            if (randomValue <= cumulativeWeight)
            {
                return kvp.Key; // 등급 반환
            }
        }

        // 혹시 모를 오류 방지
        Logger.LogWarning("[GachaRaritySystem] 가중치 선택 실패, 첫 번째 등급 반환");
        foreach (var kvp in probabilities)
        {
            return kvp.Key;
        }

        return 1; // 최종 폴백
    }

    /// <summary>
    /// 등급 숫자를 이름으로 변환 (디버깅용)
    /// </summary>
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