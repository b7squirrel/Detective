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
        // 활성화 시 초기화. GameInitializer에서 초기화 되어야 InitStageInfoUI가 초기화 되도록 구독
        GameInitializer.OnGameInitialized += InitStageInfoUI;
    }

    void OnDisable()
    {
        // OnDisable에서 이벤트 구독 해제
        LocalizationManager.OnLanguageChanged -= UpdateLanguage;
        GameInitializer.OnGameInitialized -= InitStageInfoUI;
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
        if (!GameInitializer.IsInitialized)
        {
            Logger.LogWarning("[StageInfoUI] Game not initialized yet");
            return;
        }

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

        stageBG.sprite = stageInfo.GetStageBGSrpite(currentStageIndex);
    }

    // ⭐ 추가: StageViewerController에서 뷰어 인덱스로 UI 업데이트 요청 시 호출
    // PlayerDataManager의 실제 스테이지 번호를 변경하지 않고 UI만 갱신합니다.
    public void UpdateStageInfoUIByIndex(int stageIndex)
    {
        if (!GameInitializer.IsInitialized)
        {
            Logger.LogWarning("[StageInfoUI] Game not initialized yet");
            return;
        }

        StageInfo stageInfo = FindObjectOfType<StageInfo>();
        if (stageInfo == null)
        {
            Logger.LogWarning("[StageInfoUI] StageInfo를 찾을 수 없습니다.");
            return;
        }

        // 유효 범위 체크
        int clampedIndex = Mathf.Clamp(stageIndex, 1, stageInfo.GetMaxStage());

        // 텍스트 업데이트
        if (LocalizationManager.Game != null)
        {
            Title.text = clampedIndex.ToString() + ". " +
                LocalizationManager.Game.stageBossName[clampedIndex - 1];
        }

        // 보스 이미지 업데이트
        if (stageBossImage != null)
            Destroy(stageBossImage);

        stageBossImage = Instantiate(
            stageInfo.GetStageInfo(clampedIndex).bossImagePrefab,
            stageBossImageTrns.position,
            Quaternion.identity,
            stageBossImageTrns);

        stageBG.sprite = stageInfo.GetStageBGSrpite(clampedIndex);
    }
}