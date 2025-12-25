using UnityEngine;
using UnityEngine.UI;

public class StageInfoUI : MonoBehaviour
{
    PlayerDataManager PlayerDataManager;
    [SerializeField] TMPro.TextMeshProUGUI Title;
    [SerializeField] TMPro.TextMeshProUGUI StageNumber;

    [Header("Boss Image")]
    [SerializeField] Animator anim;
    [SerializeField] Transform stageBossImageTrns;
    [SerializeField] Image stageBG;
    GameObject stageBossImage;

    void OnEnable()
    {
        // OnEnable에서 이벤트 구독
        LocalizationManager.OnLanguageChanged += UpdateLanguage;
        
        // 활성화 시 초기화
        InitStageInfoUI();
    }
    
    void OnDisable()
    {
        // OnDisable에서 이벤트 구독 해제
        LocalizationManager.OnLanguageChanged -= UpdateLanguage;
    }
    
    // 언어 변경 시 텍스트만 업데이트
    private void UpdateLanguage()
    {
        if (PlayerDataManager != null && LocalizationManager.Game != null)
        {
            int currentStageIndex = PlayerDataManager.GetCurrentStageNumber();
            Title.text = currentStageIndex.ToString() + ". " + 
                        LocalizationManager.Game.stageBossName[currentStageIndex - 1];
        }
    }
    
    internal void InitStageInfoUI()
    {
        if (PlayerDataManager == null)
            PlayerDataManager = FindObjectOfType<PlayerDataManager>();
            
        int currentStageIndex = PlayerDataManager.GetCurrentStageNumber();
        
        // 텍스트 업데이트
        if (LocalizationManager.Game != null)
        {
            Title.text = currentStageIndex.ToString() + ". " + 
                        LocalizationManager.Game.stageBossName[currentStageIndex - 1];
        }

        StageInfo stageInfo = FindObjectOfType<StageInfo>();
        
        // 보스 이미지 업데이트
        if (stageBossImage != null) 
            Destroy(stageBossImage);
            
        stageBossImage = Instantiate(
            stageInfo.GetStageInfo(currentStageIndex).bossImagePrefab,
            stageBossImageTrns.position, 
            Quaternion.identity,
            stageBossImageTrns); // 부모 설정 추가

        stageBG.sprite = stageInfo.GetStageInfo(currentStageIndex).stageBG;
    }
}