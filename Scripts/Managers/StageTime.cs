using UnityEngine;

/// <summary>
/// GameManager
/// </summary>
public class StageTime : MonoBehaviour
{
    [SerializeField] float stageDuration;
    float elaspedTime;
    TimerUI timerUI;

    private void Awake()
    {
        timerUI = FindObjectOfType<TimerUI>();
    }
    private void Start()
    {
        Init();
    }
    void Init()
    {
        FindObjectOfType<WallManager>().SetStageDuration(stageDuration);
    }

    void Update()
    {
        elaspedTime += Time.deltaTime;
        timerUI.UpdateTime(elaspedTime);
    }
}
