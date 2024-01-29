using UnityEngine;

public class StageInfoUI : MonoBehaviour
{
    [SerializeField] PlayerDataManager stageManager;
    [SerializeField] TMPro.TextMeshProUGUI Title;
    [SerializeField] TMPro.TextMeshProUGUI TitleShadow;

    internal void Init(Stages _currentStage)
    {
        Title.text = _currentStage.Title;
        TitleShadow.text = _currentStage.Title;
    }
}