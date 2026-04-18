// TutorialManager.cs — 기존 코드에 추가/수정
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialData
{
    public string Name;
    public GameObject tutorialObject;
    public bool hasShown;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("Tutorial Panels")]
    [SerializeField] Transform tutorialParent;
    [SerializeField] List<TutorialData> tutorials = new List<TutorialData>();

    // ✅ 신규 추가: 현재 튜토리얼 단계
    public TutorialStep CurrentStep { get; private set; }

    // ✅ 신규 추가: 단계 변경 시 다른 스크립트에 알림
    public static event Action<TutorialStep> OnStepChanged;

    private const string STEP_KEY = "TutorialStep";

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LoadTutorialState();
        StartCoroutine(InitWithDelay());
    }

    IEnumerator InitWithDelay()
    {
        yield return null;

        // 기존 Move 튜토리얼 유지
        foreach (var item in tutorials)
        {
            if (item.Name == "Move") ActivateTutorial("Move");
        }

        // ✅ 신규: 현재 단계 브로드캐스트 (탭 버튼들이 초기 상태 세팅하도록)
        OnStepChanged?.Invoke(CurrentStep);
    }

    // ─────────────────────────────────────────
    // ✅ 신규: 다음 단계로 진행
    // 스테이지 클리어, 뽑기 완료 등 각 이벤트에서 호출
    // ─────────────────────────────────────────
    public void AdvanceStep()
    {
        if (CurrentStep == TutorialStep.Completed) return;

        CurrentStep++;
        SaveTutorialState();

        Debug.Log($"[Tutorial] Step Advanced → {CurrentStep}");
        OnStepChanged?.Invoke(CurrentStep);
    }

    // ✅ 신규: 특정 단계인지 확인 (탭 버튼에서 사용)
    public bool IsUnlocked(TutorialStep requiredStep)
    {
        return CurrentStep >= requiredStep;
    }

    // ✅ 신규: 튜토리얼 완전히 끝났는지 확인
    public bool IsCompleted()
    {
        return CurrentStep == TutorialStep.Completed;
    }

    // ─────────────────────────────────────────
    // 기존 코드 유지
    // ─────────────────────────────────────────
    public void ActivateTutorial(string _name)
    {
        var tutorial = tutorials.Find(t => t.Name == _name);
        if (tutorial == null || tutorial.hasShown) return;
        ShowTutorial(tutorial);
    }

    void ShowTutorial(TutorialData t)
    {
        var obj = Instantiate(t.tutorialObject, tutorialParent);
        obj.SetActive(true);
        t.hasShown = true;
        SaveTutorialState();
        Debug.Log($"[Tutorial] Show: {t.Name}");
    }

    void SaveTutorialState()
    {
        // 기존 hasShown 저장
        foreach (var t in tutorials)
        {
            PlayerPrefs.SetInt(t.Name, t.hasShown ? 1 : 0);
        }

        // ✅ 신규: 현재 단계 저장
        PlayerPrefs.SetInt(STEP_KEY, (int)CurrentStep);
        PlayerPrefs.Save();
    }

    void LoadTutorialState()
    {
        // 기존 hasShown 불러오기
        foreach (var t in tutorials)
        {
            t.hasShown = PlayerPrefs.GetInt(t.Name, 0) == 1;
        }

        // ✅ 신규: 저장된 단계 불러오기 (없으면 0 = Step0)
        CurrentStep = (TutorialStep)PlayerPrefs.GetInt(STEP_KEY, 0);
        Debug.Log($"[Tutorial] Loaded Step: {CurrentStep}");
    }

    // ─────────────────────────────────────────
    // 디버깅
    // ─────────────────────────────────────────
    [ContextMenu("튜토리얼 리셋")]
    public void ResetTutorialState()
    {
        foreach (var t in tutorials)
        {
            t.hasShown = false;
            PlayerPrefs.SetInt(t.Name, 0);
        }

        PlayerPrefs.SetInt(STEP_KEY, 0);
        // ✅ 보석 지급 플래그도 리셋
        PlayerPrefs.DeleteKey("TutorialCrystalGiven");
        PlayerPrefs.DeleteKey("TutorialShopPhase");
        CurrentStep = TutorialStep.Step0_OnlyBattle;
        PlayerPrefs.Save();

        OnStepChanged?.Invoke(CurrentStep);
        Debug.Log("[Tutorial] Reset Complete → Step0_OnlyBattle");
    }

    [ContextMenu("다음 단계로 강제 진행")]
	public void DebugAdvanceStep()
	{
		AdvanceStep();
		Debug.Log($"[Tutorial] 강제 진행 → {CurrentStep}");
	}

	[ContextMenu("Step0 - OnlyBattle")]
	public void DebugSetStep0() => SetStep(TutorialStep.Step0_OnlyBattle);

	[ContextMenu("Step1 - ShopUnlocked")]
	public void DebugSetStep1() => SetStep(TutorialStep.Step1_ShopUnlocked);

	[ContextMenu("Step2 - GearUnlocked")]
	public void DebugSetStep2() => SetStep(TutorialStep.Step2_GearUnlocked);

	[ContextMenu("Step3 - MergeUnlocked")]
	public void DebugSetStep3() => SetStep(TutorialStep.Step3_MergeUnlocked);

	[ContextMenu("Step4 - AchievementUnlocked")]
	public void DebugSetStep4() => SetStep(TutorialStep.Step4_AchievementUnlocked);

	[ContextMenu("Step5 - Completed")]
	public void DebugSetStepCompleted() => SetStep(TutorialStep.Completed);

	public void SetStep(TutorialStep step)
	{
		CurrentStep = step;
		SaveTutorialState();
		OnStepChanged?.Invoke(CurrentStep);
		Debug.Log($"[Tutorial] 강제 설정 → {CurrentStep}");
	}
}