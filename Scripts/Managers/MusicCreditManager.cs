using UnityEngine;

/// <summary>
/// ���� ũ���� ����, �ش� ���������� ���� ���
/// </summary>
public class MusicCreditManager : MonoBehaviour
{
    [SerializeField] AudioCreditData creditData;
    MusicCreditUI creditUI;

    private void Start()
    {
        Init();
    }
    void Init()
    {
        // �������� �ѹ� ��������
        PlayerDataManager playerDataManager = FindObjectOfType<PlayerDataManager>();
        int index = playerDataManager.GetCurrentStageNumber();

        // �ش� ���������� �ش��ϴ� ���� ���
        AudioClip stageMusic = creditData.AudioCredits[index - 1].Clip;
        MusicManager musicManager = FindObjectOfType<MusicManager>();
        musicManager.InitBGM(stageMusic);

        // ���� ũ���� UI ǥ��
        if (creditUI == null) creditUI = FindObjectOfType<MusicCreditUI>();
        string title = creditData.AudioCredits[index - 1].Title;
        string credit = title + " - " + creditData.AudioCredits[index - 1].Credit;
        creditUI.CreditFadeIn(credit);
    }
}