using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stages
{
    public string Title;
}

public class StageInfo : MonoBehaviour
{
    public List<Stages> stages;
    public Stages GetStageInfo(int stageIndex)
    {
        return stages[stageIndex - 1];
    }
    public bool IsFinalStage(int stageIndex)
    {
        Debug.Log("stage count = " + stages.Count + "  stage index = " + stageIndex);
        return stages.Count == stageIndex;
    }
}