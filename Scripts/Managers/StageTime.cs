using UnityEngine;

/// <summary>
/// GameManager
/// </summary>
public class StageTime : MonoBehaviour
{
    public float time { get; set; }
    TimerUI timerUI;

    private void Awake()
    {
        timerUI = FindObjectOfType<TimerUI>();
    }

    void Update()
    {
        time += Time.deltaTime;
        timerUI.UpdateTime(time);
    }
}
