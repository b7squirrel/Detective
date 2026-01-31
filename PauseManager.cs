using System.Collections;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    // ⭐ 현재 게임의 기본 timeScale 저장 (무한 모드면 1.5, 레귤러면 1.0)
    float normalTimeScale = 1.0f;

    void Start()
    {
        UnPauseGame();
    }

    // ⭐ 새로운 메서드: 외부에서 기본 timeScale 설정
    public void SetNormalTimeScale(float timeScale)
    {
        normalTimeScale = timeScale;
        // Logger.Log($"[PauseManager] Normal timeScale set to {normalTimeScale}");
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        GameManager.instance.SetPauseState(true);
        // Logger.Log($"시간 정지. 타임스케일 = {Time.timeScale}");
        //Player.instance.IsPauseing = true;
    }

    public void UnPauseGame()
    {
        Time.timeScale = normalTimeScale;
        GameManager.instance.SetPauseState(false);
        // Logger.Log($"시간 다시. 타임스케일 = {normalTimeScale}");
        //Player.instance.IsPauseing = false;
    }
    public void SetTimeScale(float timeScale, float waitingTime)
    {
        StartCoroutine(SlowMotion(timeScale, waitingTime));
    }
    IEnumerator SlowMotion(float desiredTimeScale, float waitingTime)
    {
        Time.timeScale = desiredTimeScale;
        yield return new WaitForSecondsRealtime(waitingTime);
        UnPauseGame();
    }
}
