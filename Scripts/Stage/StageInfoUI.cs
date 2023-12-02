using UnityEngine;

public class StageInfoUI : MonoBehaviour
{
    [SerializeField] StageManager stageManager;
    [SerializeField] TMPro.TextMeshProUGUI Title;

    internal void Init(Stages _currentStage)
    {
        Title.text = _currentStage.Title;
    }
}