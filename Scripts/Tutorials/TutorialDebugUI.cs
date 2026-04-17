// TutorialDebugUI.cs — 새 파일로 생성
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialDebugUI : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("디버그 UI")]
    [SerializeField] GameObject debugPanel;
    [SerializeField] TextMeshProUGUI currentStepText;

    void Start()
    {
        debugPanel.SetActive(true);
        UpdateStepText();
        TutorialManager.OnStepChanged += OnStepChanged;
    }

    void OnDestroy()
    {
        TutorialManager.OnStepChanged -= OnStepChanged;
    }

    void OnStepChanged(TutorialStep step)
    {
        UpdateStepText();
    }

    void UpdateStepText()
    {
        if (TutorialManager.instance == null) return;
        currentStepText.text = $"Tutorial: {TutorialManager.instance.CurrentStep}";
    }

    // Inspector 버튼 이벤트에 연결
    public void OnResetButton()
    {
        TutorialManager.instance.ResetTutorialState();
        Debug.Log("[TutorialDebug] 리셋 완료");
    }

    // 강제로 다음 단계로 진행 (테스트용)
    public void OnAdvanceButton()
    {
        TutorialManager.instance.AdvanceStep();
        Debug.Log($"[TutorialDebug] 강제 진행 → {TutorialManager.instance.CurrentStep}");
    }

    // 특정 단계로 바로 이동
    public void SetStep(int stepIndex)
    {
        if (TutorialManager.instance == null) return;
        TutorialManager.instance.SetStep((TutorialStep)stepIndex);
        Debug.Log($"[TutorialDebug] 강제 설정 → {(TutorialStep)stepIndex}");
    }
#endif
}