using System.Collections;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    void Start()
    {
        UnPauseGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        Player.instance.IsPauseing = true;
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1;
        Player.instance.IsPauseing = false;
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
