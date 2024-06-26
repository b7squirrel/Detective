using System.Collections;
using UnityEngine;

/// <summary>
/// 음악 크레딧 관리, 해당 스테이지의 음악 재생
/// </summary>
public class MusicCreditManager : MonoBehaviour
{
    [SerializeField] AudioCreditData creditData;
    [SerializeField] AudioClip panelUpSound;
    [SerializeField] AudioClip panelDownSound;
    MusicCreditUI creditUI;
    MusicVisualizer musicVisualizer;

    public void Init()
    {
        // 스테이지 넘버 가져오기
        PlayerDataManager playerDataManager = FindObjectOfType<PlayerDataManager>();
        StageEvenetManager eventManager = FindObjectOfType<StageEvenetManager>();
        StageMusicType musicType = eventManager.GetStageMusicType();
        int index = 0;
        for (int i = 0; i < creditData.AudioCredits.Length; i++)
        {
            if (creditData.AudioCredits[i].MusicType == musicType)
                index = i;
        }

        //int index = playerDataManager.GetCurrentStageNumber();

        // 음악 크레딧 UI 표시
        if (creditUI == null) creditUI = FindObjectOfType<MusicCreditUI>();
        string title = musicType.GetDescription();
        string credit = title + " - " + creditData.AudioCredits[index].Credit;
        StartCoroutine(ShowCreditUI(credit, index));

        if(musicVisualizer == null) musicVisualizer = GetComponent<MusicVisualizer>();
    }

    void PlayBGM(int _index)
    {
        AudioClip stageMusic = creditData.AudioCredits[_index].Clip;
        MusicManager musicManager = FindObjectOfType<MusicManager>();
        musicManager.InitBGM(stageMusic);
    }
    IEnumerator ShowCreditUI(string _credit, int _index)
    {
        PlayBGM(_index);
        //yield return new WaitForSeconds(2f);
        //PlayBGM(_index);

        yield return new WaitForSeconds(.5f); // 패널이 올라오고 나서 사운드 재생
        //PlayPanelUpSound();

        yield return new WaitForSeconds(1f); // 패널 사운드와 음악이 동시에 겹치면서 나오지 않게
        creditUI.CreditFadeIn(_credit);
        musicVisualizer.Init(MusicManager.instance.GetAudioSource());

        yield return new WaitForSeconds(3.5f); // 5초 후에 패널 내림
        HideCreditUI();

        yield return new WaitForSeconds(2f); // 적당히 2초 정도 후부터는 음악이 싱크되지 않도롤
        musicVisualizer.FinishSync();
    }
    void HideCreditUI()
    {
        //PlayPanelDownSound();
        creditUI.CreditFadeOut();
    }
    void PlayPanelUpSound()
    {
        SoundManager.instance.Play(panelUpSound);

    }
    // 애니메이션 이벤트
    public void PlayPanelDownSound()
    {
        SoundManager.instance.Play(panelDownSound);
    }
}