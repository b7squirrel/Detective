using UnityEngine;

/// <summary>
/// GameManager
/// </summary>
public class StageTime : MonoBehaviour
{
    [SerializeField] float wallDuration; // ���� ������ �������� ���µ� �ɸ��� �ð�
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
