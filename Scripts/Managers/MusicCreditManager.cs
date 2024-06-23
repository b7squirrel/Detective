using UnityEngine;

/// <summary>
/// 음악 크레딧 관리, 해당 스테이지의 음악 재생
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
        // 스테이지 넘버 가져오기
        PlayerDataManager playerDataManager = FindObjectOfType<PlayerDataManager>();
        int index = playerDataManager.GetCurrentStageNumber();

        // 해당 스테이지에 해당하는 음악 재생
        AudioClip stageMusic = creditData.AudioCredits[index - 1].Clip;
        MusicManager musicManager = FindObjectOfType<MusicManager>();
        musicManager.InitBGM(stageMusic);

        // 음악 크레딧 UI 표시
        if (creditUI == null) creditUI = FindObjectOfType<MusicCreditUI>();
        string title = creditData.AudioCredits[index - 1].Title;
        string credit = title + " - " + creditData.AudioCredits[index - 1].Credit;
        creditUI.CreditFadeIn(credit);
    }
}