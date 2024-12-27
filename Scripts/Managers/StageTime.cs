using UnityEngine;

/// <summary>
/// GameManager
/// </summary>
public class StageTime : MonoBehaviour
{
    [SerializeField] float wallDuration; // 벽이 마지막 지점까지 가는데 걸리는 시간
    float elaspedTime;
    TimerUI timerUI;

    private void Awake()
    {
        //timerUI = FindObjectOfType<TimerUI>();
    }
    private void Start()
    {
        Init();
    }
    void Init()
    {
        FindObjectOfType<WallManager>().SetStageDuration(wallDuration);
        timerUI = FindObjectOfType<TimerUI>();
    }

    void Update()
    {
        elaspedTime += Time.deltaTime;
        timerUI.UpdateTime(elaspedTime);
    }
}
