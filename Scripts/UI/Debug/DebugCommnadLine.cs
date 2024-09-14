using UnityEngine;

public class DebugCommnadLine : MonoBehaviour
{
    public TMPro.TMP_InputField commandInputField; // UI에서 Input Field를 드래그하여 연결
    public TMPro.TextMeshProUGUI outputText; // 명령어 실행 결과를 보여줄 텍스트 필드(선택 사항)

    void Start()
    {
        // 입력 필드 초기화
        commandInputField.onEndEdit.AddListener(HandleCommand);
        commandInputField.ActivateInputField(); // 게임 시작 시 입력 필드에 포커스
    }

    // 명령어 처리 함수
    private void HandleCommand(string input)
    {
        input = input.Trim(); // 입력 값 앞뒤 공백 제거

        // 특정 명령어에 따른 기능 실행
        switch (input.ToLower())
        {
            case "unlockallweapon":
                UnlockAllWeapons();
                break;
            case "givegold":
                GiveGold();
                break;
            case "levelup":
                LevelUp();
                break;
            case "win":
                Debug.Log("디버그 커멘드 라인에서 호출");

                FindObjectOfType<StageEvenetManager>().IsWinningStage = true;
                PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
                // 스테이지가 클리어 된 것을 기록
                playerData.SetCurrentStageCleared();
                playerData.SaveResourcesBeforeQuitting();
                break;
            default:
                if (outputText != null)
                    outputText.text += "\n" + "Unknown command: " + input;
                break;
        }

        // 입력 필드 초기화 및 포커스 설정
        commandInputField.text = "";
        commandInputField.ActivateInputField();
    }

    public void ClearLogs()
    {
        outputText.text = string.Empty;
    }

    // 명령어 실행 함수들
    private void UnlockAllWeapons()
    {
        // 모든 무기 해금 기능
        if (outputText != null)
            outputText.text += "\n" + "All weapons have been unlocked.";
    }

    private void GiveGold()
    {
        // 골드 추가 기능
        if (outputText != null)
            outputText.text += "\n" + "Gold added.";
    }

    private void LevelUp()
    {
        // 레벨 업 기능
        if (outputText != null)
            outputText.text += "\n" + "Level up.";
    }
}