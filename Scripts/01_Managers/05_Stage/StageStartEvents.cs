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
    [SerializeField] AudioClip stageTextSwipeInSound;
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
    PlayerDataManager PlayerDataManager;
    void Start()
    {
        StartCoroutine(StageStartSequence());
    }

    void InitUI()
    {
        if (PlayerDataManager == null)
            PlayerDataManager = FindObjectOfType<PlayerDataManager>();

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

        yield return new WaitForSecondsRealtime(.1f);


        GameManager.instance.pauseManager.PauseGame();

        GetComponent<Animator>().SetTrigger("Init");
        // SoundManager.instance.Play(stageTextSwipeInSound);


        yield return new WaitForSecondsRealtime(1.5f);
        SoundManager.instance.Play(stageStartSound);

        GameManager.instance.pauseManager.UnPauseGame();
    }

    // 애니메이션 이벤트로 재생
    public void PlayTextOutSound()
    {
        SoundManager.instance.Play(stageTextSwipeOutSound);
    }
    public void PlayGetReadySound()
    {
        SoundManager.instance.Play(getReadySound);
    }
    public void PlayGoSound()
    {
        SoundManager.instance.Play(goSound);
    }
    public void SetWobbleImageActive(int active)
    {
        bool isActive = active == 1 ? true : false;
        foreach (var item in wobbleImages)
        {
            item.SetActive(isActive);
        }
    }
    public void SetSwipeImageActive(int active)
    {
        bool isActive = active == 1 ? true : false;
        foreach (var item in swipeImages)
        {
            item.SetActive(isActive);
        }
    }
}
