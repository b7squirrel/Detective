using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스테이지 이미지 탭 시 오버레이를 표시하고, 뷰어 인덱스로 스테이지를 탐색합니다.
/// PlayerDataManager의 실제 스테이지 번호는 변경하지 않습니다.
/// Stage info UI 오브젝트에 부착합니다.
/// </summary>
public class StageViewerController : MonoBehaviour
{
    [Header("오버레이")]
    [SerializeField] GameObject stageOverlay;           // Stage Overlay 오브젝트
    [SerializeField] Button stageImageButton;            // Stage Image (탭 감지용 버튼)
    [SerializeField] Button closeButton;                 // 닫기 버튼

    [Header("뷰어 화살표 (오버레이 열릴 때만 활성화)")]
    [SerializeField] Button nextViewButton;              // Next Stage Button (뷰어용)
    [SerializeField] Button prevViewButton;              // Previous Stage Button (뷰어용)

    [Header("스테이지 설명 패널 (오버레이 열릴 때만 활성화)")]
    [SerializeField] GameObject stageDescriptionPanel;  // Stage Description Panel

    [Header("자물쇠")]
    [SerializeField] GameObject imageLock;               // Image Lock 오브젝트

    [Header("스테이지 이미지 확대 및 이동")]
    [SerializeField] RectTransform stageInfoUI;          // Stage info UI RectTransform (스케일 및 이동 대상)
    [SerializeField] RectTransform stageTitleRibbon;     // Stage Title Ribbon RectTransform (함께 이동)
    [SerializeField] float enlargedScale = 1.4f;         // 확대 배율
    [SerializeField] float scaleSpeed = 20f;             // 확대/축소 속도
    [SerializeField] float moveDownAmount = 40f;         // Stage info UI 아래 이동량 (Inspector에서 조정)
    // 리본은 Stage info UI 이동량 + 이미지 팽창량만큼 추가로 내려야 이미지와 간격 유지
    // 팽창량 = stageImageHeight * (enlargedScale - 1) * 0.5f
    // stageImageHeight: Stage Image RectTransform의 Height 값 (408)
    [SerializeField] float stageImageHeight = 408f;      // Stage Image Height (팽창량 계산용)

    [Header("애니메이션")]
    [SerializeField] Animator enemyImagePanelAnim;       // Enemy Image Panel 애니메이터

    [Header("화살표 끝 도달 사운드")]
    [SerializeField] AudioClip arrowLimitClip;           // 더 이상 이동 불가 시 재생할 사운드 클립

    // 참조
    StageInfoUI stageInfoUIScript;
    StageInfo stageInfo;
    PlayerDataManager playerDataManager;

    // 화살표 버튼 Animator 캐싱 (Start에서 한 번만 가져옴)
    Animator nextViewButtonAnim;
    Animator prevViewButtonAnim;

    // 뷰어 상태
    int viewingStageIndex;          // 현재 보고 있는 스테이지 인덱스 (PlayerData 변경 안 함)
    bool isOverlayOpen = false;

    // 화살표 끝 도달 상태 추적 (상태가 변할 때만 Enabled/Disabled 트리거)
    bool wasFirst = false;
    bool wasLast  = false;

    // 스케일 애니메이션
    Vector3 normalScale;
    Vector3 targetScale;

    // 위치 애니메이션
    float stageInfoOriginalPosY;    // Stage info UI 원래 Pos Y
    float ribbonOriginalPosY;       // Stage Title Ribbon 원래 Pos Y
    float stageInfoTargetPosY;      // Stage info UI 목표 Pos Y
    float ribbonTargetPosY;         // Stage Title Ribbon 목표 Pos Y

    void Awake()
    {
        // 초기 스케일 및 위치 저장
        if (stageInfoUI != null)
        {
            normalScale = stageInfoUI.localScale;
            stageInfoOriginalPosY = stageInfoUI.anchoredPosition.y;
            stageInfoTargetPosY = stageInfoOriginalPosY;
        }

        // 타이틀 리본 원래 Pos Y 저장
        if (stageTitleRibbon != null)
        {
            ribbonOriginalPosY = stageTitleRibbon.anchoredPosition.y;
            ribbonTargetPosY = ribbonOriginalPosY;
        }

        targetScale = normalScale;

        // 오버레이 관련 오브젝트들은 항상 비활성으로 시작
        SetOverlayObjectsActive(false);
    }

    void Start()
    {
        // 참조 초기화
        stageInfoUIScript = FindObjectOfType<StageInfoUI>();
        stageInfo = FindObjectOfType<StageInfo>();

        // PlayerDataManager.Instance가 null일 경우 대비해 FindObjectOfType 병행
        playerDataManager = PlayerDataManager.Instance ?? FindObjectOfType<PlayerDataManager>();

        if (playerDataManager == null)
            Logger.LogError("[StageViewerController] PlayerDataManager를 찾을 수 없습니다!");

        if (stageInfoUIScript == null)
            Logger.LogError("[StageViewerController] StageInfoUI를 찾을 수 없습니다!");

        if (stageInfo == null)
            Logger.LogError("[StageViewerController] StageInfo를 찾을 수 없습니다!");

        // 화살표 버튼 Animator 캐싱 (매번 GetComponent 호출 방지)
        if (nextViewButton != null)
            nextViewButtonAnim = nextViewButton.GetComponent<Animator>();

        if (prevViewButton != null)
            prevViewButtonAnim = prevViewButton.GetComponent<Animator>();

        // 버튼 이벤트 연결
        if (stageImageButton != null)
            stageImageButton.onClick.AddListener(OnStageImageTapped);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseOverlay);

        if (nextViewButton != null)
            nextViewButton.onClick.AddListener(OnNextView);

        if (prevViewButton != null)
            prevViewButton.onClick.AddListener(OnPrevView);
    }

    void Update()
    {
        // Stage info UI 스케일 + 위치 애니메이션
        if (stageInfoUI != null)
        {
            stageInfoUI.localScale = Vector3.Lerp(
                stageInfoUI.localScale,
                targetScale,
                Time.deltaTime * scaleSpeed
            );

            Vector2 pos = stageInfoUI.anchoredPosition;
            pos.y = Mathf.Lerp(pos.y, stageInfoTargetPosY, Time.deltaTime * scaleSpeed);
            stageInfoUI.anchoredPosition = pos;
        }

        // Stage Title Ribbon 위치 애니메이션 (스케일 없이 위치만 이동)
        if (stageTitleRibbon != null)
        {
            Vector2 pos = stageTitleRibbon.anchoredPosition;
            pos.y = Mathf.Lerp(pos.y, ribbonTargetPosY, Time.deltaTime * scaleSpeed);
            stageTitleRibbon.anchoredPosition = pos;
        }
    }

    void OnDestroy()
    {
        // 버튼 이벤트 정리
        if (stageImageButton != null)
            stageImageButton.onClick.RemoveListener(OnStageImageTapped);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseOverlay);

        if (nextViewButton != null)
            nextViewButton.onClick.RemoveListener(OnNextView);

        if (prevViewButton != null)
            prevViewButton.onClick.RemoveListener(OnPrevView);
    }

    // ───────────────────────────────────────────
    // 오버레이 열기 / 닫기
    // ───────────────────────────────────────────

    void OnStageImageTapped()
    {
        if (isOverlayOpen) return;

        // 참조가 아직 없으면 재시도
        if (playerDataManager == null)
            playerDataManager = PlayerDataManager.Instance ?? FindObjectOfType<PlayerDataManager>();

        // 뷰어 인덱스를 현재 실제 스테이지로 초기화
        if (playerDataManager != null)
            viewingStageIndex = playerDataManager.GetCurrentStageNumber();
        else
            viewingStageIndex = 1;

        Logger.Log($"[StageViewerController] 오버레이 열기 - 현재 스테이지: {viewingStageIndex}");
        OpenOverlay();
    }

    void OpenOverlay()
    {
        isOverlayOpen = true;

        // 오버레이 및 관련 UI 활성화
        SetOverlayObjectsActive(true);

        // Stage info UI: 스케일업 + 아래로 이동
        targetScale = normalScale * enlargedScale;
        stageInfoTargetPosY = stageInfoOriginalPosY - moveDownAmount;

        // Stage Title Ribbon:
        // Stage info UI 이동량 + 이미지 팽창량(스케일업으로 이미지 하단이 늘어나는 만큼) 추가
        // 팽창량 = 이미지높이 * (배율-1) * 0.5 (피벗 center 기준 하단만)
        float expansion = stageImageHeight * (enlargedScale - 1f) * 0.5f;
        ribbonTargetPosY = ribbonOriginalPosY - moveDownAmount - expansion;

        // 오버레이 열릴 때 상태 초기화: 이전 끝 도달 상태를 리셋해서
        // UpdateViewerUI에서 Enabled 트리거가 정상 작동하도록 함
        wasFirst = false;
        wasLast  = false;

        UpdateViewerUI();

        // 오버레이 열릴 때 애니메이션 재생
        if (enemyImagePanelAnim != null)
            enemyImagePanelAnim.Play(0);
    }

    public void CloseOverlay()
    {
        isOverlayOpen = false;

        // 오버레이 및 관련 UI 비활성화
        SetOverlayObjectsActive(false);

        // Stage info UI: 원래 스케일 + 원래 위치로
        targetScale = normalScale;
        stageInfoTargetPosY = stageInfoOriginalPosY;

        // Stage Title Ribbon: 원래 위치로
        ribbonTargetPosY = ribbonOriginalPosY;

        // 뷰어 탐색 중 다른 스테이지를 보고 있었을 수 있으므로
        // 현재 실제 도전 중인 스테이지 UI로 복원
        if (stageInfoUIScript != null)
            stageInfoUIScript.InitStageInfoUI();
    }

    // ───────────────────────────────────────────
    // 오버레이 관련 오브젝트 일괄 활성/비활성
    // ───────────────────────────────────────────

    void SetOverlayObjectsActive(bool isActive)
    {
        if (stageOverlay != null)
            stageOverlay.SetActive(isActive);

        if (nextViewButton != null)
            nextViewButton.gameObject.SetActive(isActive);

        if (prevViewButton != null)
            prevViewButton.gameObject.SetActive(isActive);

        if (stageDescriptionPanel != null)
            stageDescriptionPanel.SetActive(isActive);

        if (closeButton != null)
            closeButton.gameObject.SetActive(isActive);

        // 자물쇠는 열릴 때 UpdateViewerUI에서 판단하고, 닫힐 때는 숨김
        if (!isActive && imageLock != null)
            imageLock.SetActive(false);
    }

    // ───────────────────────────────────────────
    // 뷰어 화살표
    // ───────────────────────────────────────────

    void OnNextView()
    {
        if (stageInfo == null) return;

        if (viewingStageIndex < stageInfo.GetMaxStage())
        {
            viewingStageIndex++;
            // 클릭한 버튼에만 Hit 트리거
            if (nextViewButtonAnim != null)
                nextViewButtonAnim.SetTrigger("Hit");
            UpdateViewerUI();
        }
    }

    void OnPrevView()
    {
        if (viewingStageIndex > 1)
        {
            viewingStageIndex--;
            // 클릭한 버튼에만 Hit 트리거
            if (prevViewButtonAnim != null)
                prevViewButtonAnim.SetTrigger("Hit");
            UpdateViewerUI();
        }
    }

    // ───────────────────────────────────────────
    // UI 업데이트
    // ───────────────────────────────────────────

    void UpdateViewerUI()
    {
        if (stageInfoUIScript == null || playerDataManager == null || stageInfo == null)
        {
            Logger.LogError($"[StageViewerController] UpdateViewerUI 참조 누락 - " +
                $"stageInfoUI:{stageInfoUIScript != null}, " +
                $"playerDataManager:{playerDataManager != null}, " +
                $"stageInfo:{stageInfo != null}");
            return;
        }

        // StageInfoUI에 뷰어 인덱스 기준으로 UI 업데이트 요청
        stageInfoUIScript.UpdateStageInfoUIByIndex(viewingStageIndex);

        // 실제 저장된 현재 스테이지와 비교해 자물쇠 여부 판단
        int currentStage = playerDataManager.GetCurrentStageNumber();
        bool isLocked = viewingStageIndex > currentStage;

        Logger.Log($"[StageViewerController] viewingIndex:{viewingStageIndex} / currentStage:{currentStage} / isLocked:{isLocked}");

        // 자물쇠 표시
        if (imageLock != null)
            imageLock.SetActive(isLocked);

        // 끝 도달 여부 판단 (stageInfo.GetMaxStage()로 동적으로 최고 스테이지 파악)
        bool isFirst = viewingStageIndex <= 1;
        bool isLast  = viewingStageIndex >= stageInfo.GetMaxStage();

        // 이전 화살표: 상태가 변할 때만 Disabled/Enabled 트리거
        if (prevViewButton != null && prevViewButtonAnim != null)
        {
            if (isFirst && !wasFirst)
            {
                // 방금 첫 스테이지에 도달 → Disabled
                prevViewButtonAnim.SetTrigger("Disabled");
                if (arrowLimitClip != null) SoundManager.instance?.Play(arrowLimitClip);
            }
            else if (!isFirst && wasFirst)
            {
                // 첫 스테이지에서 벗어남 → Enabled
                prevViewButtonAnim.SetTrigger("Enabled");
            }
            prevViewButton.interactable = !isFirst;
        }

        // 다음 화살표: 상태가 변할 때만 Disabled/Enabled 트리거
        if (nextViewButton != null && nextViewButtonAnim != null)
        {
            if (isLast && !wasLast)
            {
                // 방금 마지막 스테이지에 도달 → Disabled
                nextViewButtonAnim.SetTrigger("Disabled");
                if (arrowLimitClip != null) SoundManager.instance?.Play(arrowLimitClip);
            }
            else if (!isLast && wasLast)
            {
                // 마지막 스테이지에서 벗어남 → Enabled
                nextViewButtonAnim.SetTrigger("Enabled");
            }
            nextViewButton.interactable = !isLast;
        }

        // 현재 상태를 다음 호출을 위해 저장
        wasFirst = isFirst;
        wasLast  = isLast;
    }
}