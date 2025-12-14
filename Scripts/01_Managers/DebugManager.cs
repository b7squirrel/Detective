using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI stageEvenetIndex;

    public void SetStageEventIndex(int _index)
    {
        stageEvenetIndex.text = "Stage Event Index = " + _index.ToString();
    }
}