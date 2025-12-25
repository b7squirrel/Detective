using UnityEngine;

/// <summary>
/// GameManager
/// </summary>
public class StageTime : MonoBehaviour
{
    float elaspedTime;
    TimerUI timerUI;

    public void Init(float _time)
    {
        FindObjectOfType<WallManager>().SetStageDuration(_time);
        timerUI = FindObjectOfType<TimerUI>();
    }

    void Update()
    {
        elaspedTime += Time.deltaTime;
        timerUI.UpdateTime(elaspedTime);
    }
}
