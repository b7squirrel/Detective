using UnityEngine;

public class StageInfoUI : MonoBehaviour
{
    [SerializeField] PlayerDataManager stageManager;
    [SerializeField] TMPro.TextMeshProUGUI Title;
    [SerializeField] TMPro.TextMeshProUGUI StageNumber;

    [Header("Boss Image")]
    [SerializeField] Animator anim;

    internal void Init(Stages _currentStage)
    {
        stageManager = FindObjectOfType<PlayerDataManager>();
        Title.text = stageManager.GetCurrentStageNumber().ToString() + ". " + _currentStage.Title;

        StageInfo stageInfo = FindObjectOfType<StageInfo>();
        anim.runtimeAnimatorController = stageInfo.GetStageInfo(stageManager.GetCurrentStageNumber()).bossImage;
    }
}