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
    [SerializeField] TMPro.TextMeshProUGUI stageNumText;
    PlayerDataManager PlayerDataManager;
    void Start()
    {
        StartCoroutine(StageStartSequence());
    }

    public void InitUI()
    {
        if (PlayerDataManager == null)
            PlayerDataManager = FindObjectOfType<PlayerDataManager>();

        int currentStageIndex = PlayerDataManager.GetCurrentStageNumber();

        // Logger.LogError($"[StageInfoUI] currentStageIndex: {currentStageIndex}");
        // Logger.LogError($"[StageInfoUI] stageBossName Length: {LocalizationManager.Game.stageBossName.Length}");

        // 텍스트 업데이트
        stageNumText.text = currentStageIndex.ToString();
    }

    IEnumerator StageStartSequence()
    {
        yield return new WaitForSecondsRealtime(.1f);

        GameManager.instance.pauseManager.PauseGame();

        GetComponent<Animator>().SetTrigger("Init");
        SoundManager.instance.Play(stageTextSwipeInSound);
        SoundManager.instance.Play(getReadySound);


        SoundManager.instance.Play(stageStartSound);

        yield return new WaitForSecondsRealtime(1.5f);

        SoundManager.instance.Play(goSound);
        GameManager.instance.pauseManager.UnPauseGame();
    }

    // 애니메이션 이벤트로 재생
    public void PlayTextOutSound()
    {
        SoundManager.instance.Play(stageTextSwipeOutSound);
    }
}
