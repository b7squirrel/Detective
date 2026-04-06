using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stages
{
    public string Title;
    public GameObject bossImagePrefab;
    public Sprite stageBG;
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
    public bool IsFinalStage(int stageIndex)
    {
        return stages.Count == stageIndex;
    }
    public int GetMaxStage()
    {
        return stages.Count;
    }
    public Sprite GetStageBGSrpite(int stageIndex)
    {
        Logger.LogError($"stageIndex = {stageIndex}");
        StageGroundType groundType = GetStageInfo(stageIndex).stageGroundType;
        int index = (int)groundType;
        return LobbyStageBGs[index]; //stageIndex는 실제 스테이지 숫자임
    }
}