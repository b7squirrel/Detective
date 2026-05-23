using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stages
{
    // ✅ Title 제거 - 보스 이름은 Localization SO에서만 관리
    public GameObject bossImagePrefab;
    public StageGroundType stageGroundType;
}

public class StageInfo : MonoBehaviour
{
    public List<Stages> stages;
    [SerializeField] Sprite[] LobbyStageBGs;

    public Stages GetStageInfo(int stageIndex)
    {
        return stages[stageIndex - 1];
    }

    // ✅ 새로 추가 - 언어 설정에 맞는 보스 이름 반환
    public string GetStageBossName(int stageIndex)
    {
        if (LocalizationManager.Game != null &&
            LocalizationManager.Game.stageBossName != null &&
            stageIndex >= 1 &&
            stageIndex - 1 < LocalizationManager.Game.stageBossName.Length)
        {
            return LocalizationManager.Game.stageBossName[stageIndex - 1];
        }
        Debug.LogWarning($"[StageInfo] stageBossName 없음 (index: {stageIndex})");
        return "";
    }

    public bool IsFinalStage(int stageIndex) => stages.Count == stageIndex;
    public int GetMaxStage() => stages.Count;

    public Sprite GetStageBGSrpite(int stageIndex)
    {
        StageGroundType groundType = GetStageInfo(stageIndex).stageGroundType;
        return LobbyStageBGs[(int)groundType];
    }
}