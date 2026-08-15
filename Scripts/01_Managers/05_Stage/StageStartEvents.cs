using System.Collections;
using UnityEngine;
/// <summary>
/// 플레이어를 찾아서 텔레포트 이펙트
/// 스테이지 시작 사운드
/// 스테이지 시작 UI
/// </summary>
public class StageStartEvents : MonoBehaviour
{
    [SerializeField] AudioClip stageStartSound;
    [SerializeField] AudioClip notificationSound;
    [SerializeField] AudioClip stageStartUISwooshSound;
    [SerializeField] AudioClip stageStartVoice;
    [SerializeField] AudioClip stageTextSwipeOutSound;
    [SerializeField] AudioClip getReadySound;
    [SerializeField] AudioClip goSound;
    [SerializeField] GameObject playerTeleportEffect;
    [SerializeField] GameObject startUIPanel;
    [SerializeField] TMPro.TextMeshProUGUI stageNumText;
    [SerializeField] GameObject stageTextGroup;
    [SerializeField] GameObject waveTextGroup;
    [SerializeField] GameObject[] wobbleImages;
    [SerializeField] GameObject[] swipeImages;
    [SerializeField] SpawnGemsOnStart spawnGemsOnStart; // 인스펙터에서 연결
    PlayerDataManager PlayerDataManager;
    void Start()
    {
        StartCoroutine(StageStartSequence());
    }

    void InitUI()
    {
        if (PlayerDataManager == null)
            PlayerDataManager = FindObjectOfType<PlayerDataManager>();

        if (spawnGemsOnStart == null)
            spawnGemsOnStart = FindObjectOfType<SpawnGemsOnStart>();

        int currentStageIndex = PlayerDataManager.GetCurrentStageNumber();

        // Logger.LogError($"[StageInfoUI] currentStageIndex: {currentStageIndex}");
        // Logger.LogError($"[StageInfoUI] stageBossName Length: {LocalizationManager.Game.stageBossName.Length}");

        GameMode gameMode = FindObjectOfType<PlayerDataManager>().GetGameMode();
        if (gameMode == GameMode.Regular)
        {
            // 텍스트 업데이트
            stageNumText.text = currentStageIndex.ToString();
            stageTextGroup.SetActive(true);
            waveTextGroup.SetActive(false);
        }
        else
        {
            stageTextGroup.SetActive(false);
            waveTextGroup.SetActive(true);
        }

        startUIPanel.SetActive(true);
    }

    IEnumerator StageStartSequence()
    {
        InitUI();

        // ⭐ 추가: 시퀀스 시작부터 플레이어 조작을 잠금
        if (Player.instance != null)
            Player.instance.ShouldBeStill = true;

        yield return new WaitForSecondsRealtime(.1f);

        GameManager.instance.pauseManager.PauseGame();

        GetComponent<Animator>().SetTrigger("Init");
    }

    // ⭐ 애니메이션 이벤트로 재생 - 프레임 151 (UI 이탈 시작 시점)
    public void SpawnFieldItems()
    {
        if (spawnGemsOnStart != null)
            spawnGemsOnStart.GenGemsAndChest();
    }

    // ⭐ 애니메이션 이벤트로 재생 - 프레임 153 (UI 완전히 벗어난 시점)
    // ⭐ 애니메이션 이벤트로 재생 - 프레임 153 (UI 완전히 벗어난 시점)
    // 시간 재개 + 스폰 + 조작 허용까지 한 번에 순서대로 처리
    public void OnStageStartSequenceEnd()
    {
        SoundManager.instance.Play(stageStartSound);

        // 1단계: 시간(애니메이션)은 다시 흐르게 함
        GameManager.instance.pauseManager.UnPauseGame();

        // // 2단계: 젬/상자 스폰 (ShouldBeStill은 아직 true라서 이 순간엔 조작 불가)
        // if (spawnGemsOnStart != null)
        //     spawnGemsOnStart.GenGemsAndChest();

        // 3단계: 스폰이 끝난 직후 조작 허용
        // if (Player.instance != null)
        //     Player.instance.ShouldBeStill = false;
    }

    // 애니메이션 이벤트로 재생
    public void SpawnGemOnStart()
    {
        if (spawnGemsOnStart != null)
            spawnGemsOnStart.GenGemsAndChest();
    }
    public void CanControlplayer()
    {
        if (Player.instance != null)
            Player.instance.ShouldBeStill = false;
    }
    public void CameraZoomIn()
    {
        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController !=null) cameraController.ZoomInOnStart();
    }
    public void PlayTextOutSound()
    {
        if (stageTextSwipeOutSound != null)
            SoundManager.instance.Play(stageTextSwipeOutSound);
    }
    public void PlayGetReadySound()
    {
        if (getReadySound != null)
            SoundManager.instance.Play(getReadySound);
    }
    public void PlayGoSound()
    {
        if (goSound != null)
            SoundManager.instance.Play(goSound);
    }
    public void PlayStageStartVoice()
    {
        if (stageStartVoice != null)
            SoundManager.instance.Play(stageStartVoice);
    }
    public void PlayStageStartUISwooshSound()
    {
        if (stageStartUISwooshSound != null)
            SoundManager.instance.Play(stageStartUISwooshSound);
    }
    public void SetWobbleImageActive(int active)
    {
        bool isActive = active == 1 ? true : false;
        foreach (var item in wobbleImages)
        {
            item.SetActive(isActive);
        }
    }
    public void PlayNotificationSound()
    {
        if (notificationSound != null)
            SoundManager.instance.Play(notificationSound);
    }
    public void SetSwipeImageActive(int active)
    {
        bool isActive = active == 1 ? true : false;
        foreach (var item in swipeImages)
        {
            item.SetActive(isActive);
        }
    }
    // 애니메이션 이벤트로 재생
    public void DeactivateStartUI()
    {
        gameObject.SetActive(false);
    }
}
