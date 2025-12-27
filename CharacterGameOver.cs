using System.Collections;
using UnityEngine;

public class CharacterGameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    [SerializeField] GameObject weaponsGroup;

    [Header("Feedback")]
    [SerializeField] AudioClip gameOverSound;
    [SerializeField] AudioClip gameOverVocalSound;
    [SerializeField] AudioClip gameOverPanelSound;

    public void GameOver()
    {
        SoundManager.instance.StopAllSounds();
        SoundManager.instance.Play(gameOverSound);
        MusicManager.instance.Stop();
        StartCoroutine(GameOverCo());
        // GameManager.instance.joystick.SetActive(false);
    }
    IEnumerator GameOverCo()
    {
        GameManager.instance.pauseManager.PauseGame();
        yield return new WaitForSecondsRealtime(.5f);
        SoundManager.instance.Play(gameOverVocalSound);

        yield return new WaitForSecondsRealtime(1.2f);
        SoundManager.instance.Play(gameOverPanelSound);

        gameOverPanel.SetActive(true);
        weaponsGroup.SetActive(false);

        // 스테이지에서 획득한 코인만 표시.
        int killNum = GameManager.instance.GetComponent<KillManager>().GetCurrentKills();
        int coinNum = GameManager.instance.GetComponent<CoinManager>().GetCoinNumPickedup();

        // 게임 모드 체크
        GameMode mode = PlayerDataManager.Instance.GetGameMode();
        if (mode == GameMode.Regular)
        {
            // 현재 스테이지
            int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
            gameOverPanel.GetComponent<ResultPanel>().InitAwards(killNum, coinNum, stageNum, false);
        }
        else
        {
            InfiniteStageManager infiniteManager = FindObjectOfType<InfiniteStageManager>();
            if (infiniteManager == null)
            {
                Logger.LogError($"[Character Game Over] infinite manager null");
                yield break;
            }

            // 현재 기록
            int currentWave = infiniteManager.GetCurrentWave();
            float currentTime = infiniteManager.GetSurvivalTime();
            string timeFormatted = new GeneralFuctions().FormatTime(currentTime);

            // 최고 기록
            int bestWave = PlayerDataManager.Instance.GetBestWave();
            float bestTime = PlayerDataManager.Instance.GetBestSurvivalTime();
            string bestRecordFormatted = new GeneralFuctions().FormatTime(bestTime);

            // 기록 갱신 체크
            bool isBreakingRecords = currentTime > bestTime ? true : false;

            gameOverPanel.GetComponent<ResultPanel>().InitInfiniteAwards(killNum, coinNum, currentWave, timeFormatted, bestRecordFormatted, isBreakingRecords);
        }
    }
}