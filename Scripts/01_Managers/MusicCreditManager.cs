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
        // ⭐ 게임 모드 확인
        PlayerDataManager playerDataManager = PlayerDataManager.Instance;
        GameMode currentMode = playerDataManager.GetGameMode();

        if (currentMode == GameMode.Infinite)
        {
            InitBGMInfiniteStage();
        }
        else
        {
            InitBGMRegularStage();
        }

        if (musicVisualizer == null) musicVisualizer = GetComponent<MusicVisualizer>();
    }

    /// <summary>
    /// 일반 스테이지 음악 초기화 (기존 로직)
    /// </summary>
    void InitBGMRegularStage()
    {
        // 스테이지 넘버 가져오기
        StageEvenetManager eventManager = FindObjectOfType<StageEvenetManager>();
        StageMusicType musicType = eventManager.GetStageMusicType();

        // 크레딧 제목으로 음악 파일 찾기
        int index = 0;
        for (int i = 0; i < creditData.AudioCredits.Length; i++)
        {
            if (creditData.AudioCredits[i].MusicType == musicType)
                index = i;
        }

        // 음악 크레딧 UI 표시
        if (creditUI == null) creditUI = FindObjectOfType<MusicCreditUI>();
        string title = "\"" + musicType.GetDescription() + "\"";
        string credit = "작곡 " + creditData.AudioCredits[index].Credit;
        StartCoroutine(ShowCreditUI(title, credit, index));
    }

    /// <summary>
    /// ⭐ 무한 모드 음악 초기화
    /// </summary>
    void InitBGMInfiniteStage()
    {
        Logger.Log("[MusicCreditManager] InitBGMInfiniteStage 시작");

        // 무한 모드 음악 배열이 비어있는지 검증
        if (creditData.InfiniteModeAudioCredits == null || creditData.InfiniteModeAudioCredits.Length == 0)
        {
            Logger.LogError("[MusicCreditManager] InfiniteModeAudioCredits가 비어있습니다!");
            return;
        }

        // InfiniteStageManager에서 현재 웨이브 가져오기
        InfiniteStageManager infiniteManager = FindObjectOfType<InfiniteStageManager>();
        if (infiniteManager == null)
        {
            Logger.LogError("[MusicCreditManager] InfiniteStageManager를 찾을 수 없습니다!");
            return;
        }

        // ⭐⭐⭐ 이벤트 구독
        infiniteManager.OnWaveComplete += OnInfiniteWaveComplete;
        Logger.Log("[MusicCreditManager] Subscribed to OnWaveComplete event");

        int currentWave = infiniteManager.GetCurrentWave();

        // ⭐⭐⭐ currentWave가 0이면 1로 간주 (초기화 시점)
        if (currentWave == 0) currentWave = 1;

        // 음악 인덱스 계산: (웨이브 - 1) / 6 % 음악개수
        // 웨이브 1-6: 0, 웨이브 7-12: 1, 웨이브 13-18: 2, ...
        int musicIndex = ((currentWave - 1) / 6) % creditData.InfiniteModeAudioCredits.Length;

        Logger.Log($"[MusicCreditManager] Infinite Mode - Wave {currentWave}, Music Index {musicIndex}");

        // ⭐ 음악 크레딧 UI 찾기
        if (creditUI == null) creditUI = FindObjectOfType<MusicCreditUI>();

        if (creditUI == null)
        {
            Logger.LogError("[MusicCreditManager] MusicCreditUI를 찾을 수 없습니다!");
            return;
        }
        else
        {
            Logger.Log("[MusicCreditManager] MusicCreditUI 찾음");
        }

        AudioCredit currentMusic = creditData.InfiniteModeAudioCredits[musicIndex];
        string title = "\"" + currentMusic.MusicType.GetDescription() + "\"";
        string credit = "작곡 " + currentMusic.Credit;

        // ⭐⭐⭐ 웨이브 1 또는 6의 배수 + 1 웨이브(7, 13, 19...)에서만 크레딧 UI 표시
        bool showCredit = (currentWave == 1) || ((currentWave - 1) % 6 == 0 && currentWave > 1);

        Logger.Log($"[MusicCreditManager] Show Credit: {showCredit}, Title: {title}, Credit: {credit}");

        StartCoroutine(ShowCreditUI(title, credit, musicIndex, true, showCredit));
    }

    /// <summary>
    /// ⭐⭐⭐ 무한 모드 웨이브 완료 이벤트 핸들러
    /// </summary>
    void OnInfiniteWaveComplete(int completedWave)
    {
        Logger.Log($"[MusicCreditManager] OnInfiniteWaveComplete called - Wave {completedWave}");

        // 6의 배수 웨이브가 아니면 무시
        if (completedWave % 6 != 0)
        {
            Logger.Log($"[MusicCreditManager] Wave {completedWave} is not a multiple of 6, skipping music change");
            return;
        }

        // 무한 모드가 아니면 무시
        if (PlayerDataManager.Instance.GetGameMode() != GameMode.Infinite)
        {
            Logger.LogWarning("[MusicCreditManager] Not in Infinite mode, skipping music change");
            return;
        }

        // 다음 음악 인덱스 계산
        int nextMusicIndex = (completedWave / 6) % creditData.InfiniteModeAudioCredits.Length;
        
        Logger.Log($"[MusicCreditManager] Wave {completedWave} complete! Playing music {nextMusicIndex}");

        AudioCredit nextMusic = creditData.InfiniteModeAudioCredits[nextMusicIndex];
        string title = "\"" + nextMusic.MusicType.GetDescription() + "\"";
        string credit = "작곡 " + nextMusic.Credit;

        // 새로운 음악 재생 및 크레딧 표시
        StartCoroutine(ShowCreditUI(title, credit, nextMusicIndex, true, true));
    }

    /// <summary>
    /// ⭐ 무한 모드에서 다음 음악 재생 (직접 호출용 - 필요 시)
    /// </summary>
    public void PlayNextInfiniteMusic(int completedWave)
    {
        OnInfiniteWaveComplete(completedWave);
    }

    void PlayBGM(int _index, bool isInfiniteMode = false)
    {
        AudioClip stageMusic = isInfiniteMode 
            ? creditData.InfiniteModeAudioCredits[_index].Clip 
            : creditData.AudioCredits[_index].Clip;
            
        MusicManager musicManager = FindObjectOfType<MusicManager>();
        musicManager.InitBGM(stageMusic);
    }

    IEnumerator ShowCreditUI(string _title, string _credit, int _index, bool isInfiniteMode = false, bool showCredit = true)
    {
        PlayBGM(_index, isInfiniteMode);

        yield return new WaitForSeconds(.5f);

        if (showCredit)
        {
            yield return new WaitForSeconds(1f);
            creditUI.CreditFadeIn(_title, _credit);
            musicVisualizer.Init(MusicManager.instance.GetAudioSource());

            yield return new WaitForSeconds(3.5f);
            HideCreditUI();

            yield return new WaitForSeconds(2f);
            musicVisualizer.FinishSync();
        }
        else
        {
            // 크레딧 UI를 표시하지 않을 경우에도 뮤직 비주얼라이저는 초기화
            musicVisualizer.Init(MusicManager.instance.GetAudioSource());
        }
    }

    void HideCreditUI()
    {
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

    /// <summary>
    /// ⭐ 이벤트 구독 해제 (메모리 누수 방지)
    /// </summary>
    void OnDestroy()
    {
        // 무한 모드인 경우에만 구독 해제
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetGameMode() == GameMode.Infinite)
        {
            InfiniteStageManager infiniteManager = FindObjectOfType<InfiniteStageManager>();
            if (infiniteManager != null)
            {
                infiniteManager.OnWaveComplete -= OnInfiniteWaveComplete;
                Logger.Log("[MusicCreditManager] Unsubscribed from OnWaveComplete event");
            }
        }
    }
}