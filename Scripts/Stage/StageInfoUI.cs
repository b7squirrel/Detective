using Unity.Mathematics;
using UnityEngine;

public class StageInfoUI : MonoBehaviour
{
    [SerializeField] PlayerDataManager stageManager;
    [SerializeField] TMPro.TextMeshProUGUI Title;
    [SerializeField] TMPro.TextMeshProUGUI StageNumber;

    [Header("Boss Image")]
    [SerializeField] Animator anim;
    [SerializeField] Transform stageBossImageTrns; // 보스 이미지를 위치시킬 장소
    GameObject stageBossImage;


    internal void Init(Stages _currentStage)
    {
        stageManager = FindObjectOfType<PlayerDataManager>();
        Title.text = stageManager.GetCurrentStageNumber().ToString() + ". " + _currentStage.Title;

        StageInfo stageInfo = FindObjectOfType<StageInfo>();
        // anim.runtimeAnimatorController = stageInfo.GetStageInfo(stageManager.GetCurrentStageNumber()).bossImage;
        if (stageBossImage != null) Destroy(stageBossImage);
        stageBossImage = Instantiate(stageInfo.GetStageInfo(stageManager.GetCurrentStageNumber()).bossImagePrefab,
                                    stageBossImageTrns.position, Quaternion.identity);
    }

    public void PlayFromStart()
    {
        anim.SetTrigger("Init");
    }
}