using UnityEngine;

/// <summary>
/// GameManager
/// </summary>
public class StageTime : MonoBehaviour
{
    [SerializeField] float stageDuration;
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
        if(stageDuration < 0)
        {
            stageDuration = 0;
        }
        stageDuration -= Time.deltaTime;
        timerUI.UpdateTime(stageDuration);
    }
}
