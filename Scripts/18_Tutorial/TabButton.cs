// TabButton.cs — 각 하단 탭 버튼 GameObject에 추가
using UnityEngine;
using UnityEngine.UI;

public class TabButton : MonoBehaviour
{
    [Header("언락 조건")]
    [SerializeField] private TutorialStep requiredStep = TutorialStep.Step0_OnlyBattle;

    [Header("잠금 표시 (선택사항)")]
    [SerializeField] private GameObject lockIcon;        // 자물쇠 아이콘 오브젝트
    [SerializeField] private float lockedAlpha = 0.4f;  // 잠금 시 버튼 투명도

    private Button button;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        button = GetComponent<Button>();

        // CanvasGroup 없으면 자동 추가 (투명도 조절용)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // 이벤트 구독
        TutorialManager.OnStepChanged += OnStepChanged;

        // 현재 단계로 즉시 상태 업데이트
        // (씬 로드 시 이미 진행된 단계 반영)
        if (TutorialManager.instance != null)
            UpdateTabState(TutorialManager.instance.CurrentStep);
    }

    void OnDisable()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        TutorialManager.OnStepChanged -= OnStepChanged;
    }

    // OnStepChanged 이벤트 수신 시 호출
    private void OnStepChanged(TutorialStep currentStep)
    {
        UpdateTabState(currentStep);
    }

    private void UpdateTabState(TutorialStep currentStep)
    {
        bool isUnlocked = currentStep >= requiredStep;

        // 버튼 클릭 가능 여부
        button.interactable = isUnlocked;

        // 투명도 조절
        canvasGroup.alpha = isUnlocked ? 1f : lockedAlpha;

        // 자물쇠 아이콘 (있을 경우)
        if (lockIcon != null)
            lockIcon.SetActive(!isUnlocked);

        Debug.Log($"[TabButton] {gameObject.name} → {(isUnlocked ? "활성" : "잠금")}");
    }
}