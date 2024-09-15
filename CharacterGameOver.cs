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
        Debug.Log("HERE!");
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

        // ������������ ȹ���� ���θ� ǥ��.
        int killNum = GameManager.instance.GetComponent<KillManager>().GetCurrentKills();
        int coinNum = GameManager.instance.GetComponent<CoinManager>().GetCoinNumPickedup();

        // ���� ��������
        int stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();

        gameOverPanel.GetComponent<ResultPanel>().InitAwards(killNum, coinNum, stageNum);
    }
}
