using UnityEngine;

public class DebugCommnadLine : MonoBehaviour
{
    public TMPro.TMP_InputField commandInputField; // UI���� Input Field�� �巡���Ͽ� ����
    public TMPro.TextMeshProUGUI outputText; // ��ɾ� ���� ����� ������ �ؽ�Ʈ �ʵ�(���� ����)

    void Start()
    {
        // �Է� �ʵ� �ʱ�ȭ
        commandInputField.onEndEdit.AddListener(HandleCommand);
        commandInputField.ActivateInputField(); // ���� ���� �� �Է� �ʵ忡 ��Ŀ��
    }

    // ��ɾ� ó�� �Լ�
    private void HandleCommand(string input)
    {
        input = input.Trim(); // �Է� �� �յ� ���� ����

        // Ư�� ��ɾ ���� ��� ����
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
                Debug.Log("����� Ŀ��� ���ο��� ȣ��");

                FindObjectOfType<StageEvenetManager>().IsWinningStage = true;
                PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
                // ���������� Ŭ���� �� ���� ���
                playerData.SetCurrentStageCleared();
                playerData.SaveResourcesBeforeQuitting();
                break;
            default:
                if (outputText != null)
                    outputText.text += "\n" + "Unknown command: " + input;
                break;
        }

        // �Է� �ʵ� �ʱ�ȭ �� ��Ŀ�� ����
        commandInputField.text = "";
        commandInputField.ActivateInputField();
    }

    public void ClearLogs()
    {
        outputText.text = string.Empty;
    }

    // ��ɾ� ���� �Լ���
    private void UnlockAllWeapons()
    {
        // ��� ���� �ر� ���
        if (outputText != null)
            outputText.text += "\n" + "All weapons have been unlocked.";
    }

    private void GiveGold()
    {
        // ��� �߰� ���
        if (outputText != null)
            outputText.text += "\n" + "Gold added.";
    }

    private void LevelUp()
    {
        // ���� �� ���
        if (outputText != null)
            outputText.text += "\n" + "Level up.";
    }
}