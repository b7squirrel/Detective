using System.Collections;
using UnityEngine;

public class CharacterGameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject infiniteGameOverPanel;
    [SerializeField] GameObject weaponsGroup;

    [Header("Regular Feedback")]
    // [SerializeField] AudioClip gameOverSound;
    // [SerializeField] AudioClip gameOverVocalSound;
    [SerializeField] AudioClip gameOverPanelSound;

    
    GameMode gameMode;

    public void GameOver()
    {
        gameMode = PlayerDataManager.Instance.GetGameMode();
        // ★ deathVocalSound를 제외한 모든 사운드 정지
        Character character = GetComponent<Character>();
        SoundManager.instance.StopAllSoundsExcept(character.deathVocalSound);
        MusicManager.instance.Stop();
        StartCoroutine(GameOverCo());
        GameManager.instance.joystick.SetActive(false);
    }
    IEnumerator GameOverCo()
    {
        GameManager.instance.pauseManager.PauseGame();
        yield return new WaitForSecondsRealtime(.5f);
        // SoundManager.instance.Play(gameOverVocalSound);

        yield return new WaitForSecondsRealtime(1.2f);
        SoundManager.instance.Play(gameOverPanelSound);

        weaponsGroup.SetActive(false);

        // 스테이지에서 획득한 코인만 표시.
        int killNum = GameManager.instance.GetComponent<KillManager>().GetCurrentKills();
        int coinNum = GameManager.instance.GetComponent<CoinManager>().GetCoinNumPickedup();

        // 게임 모드 체크
        if (gameMode == GameMode.Regular)
        {
            gameOverPanel.SetActive(true);

            // 현재 스테이지
            int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
            int killGold = GoldRewardManager.Instance.GetKillGold();
            gameOverPanel.GetComponent<ResultPanel>().InitAwards(killNum, coinNum, stageNum, false, killGold, 0);
        }
        else
        {
            infiniteGameOverPanel.SetActive(true);

            InfiniteStageManager infiniteManager = FindObjectOfType<InfiniteStageManager>();
            if (infiniteManager == null)
            {
                Logger.LogError($"[Character Game Over] infinite manager null");
                yield break;
            }

            // 현재 기록
            int currentWave = infiniteManager.GetCurrentWave();
            int clearedWaves = infiniteManager.GetClearedWaves();
            float currentTime = infiniteManager.GetSurvivalTime();
            string timeFormatted = new GeneralFuctions().FormatTime(currentTime);

            // 최고 기록
            int bestWave = PlayerDataManager.Instance.GetBestWave();
            float bestTime = PlayerDataManager.Instance.GetBestSurvivalTime();
            string bestRecordFormatted = new GeneralFuctions().FormatTime(bestTime);

            // 기록 갱신 체크
            bool isBreakingRecords = currentTime > bestTime ? true : false;

            // 무한모드 골드 계산
            int killGold = GoldRewardManager.Instance.GetKillGold();
            int waveBonus = GoldRewardManager.Instance.CalculateInfiniteGold(clearedWaves, isBreakingRecords); // killGold 빼기
            int infiniteGold = waveBonus + killGold; // 기존 합산은 유지 (저장용)

            infiniteGameOverPanel.GetComponent<ResultPanel>().InitInfiniteAwards(
                killNum, coinNum, currentWave, timeFormatted,
                bestRecordFormatted, isBreakingRecords,
                infiniteGold, killGold, waveBonus // ★ 추가
            );
        }
    }
}