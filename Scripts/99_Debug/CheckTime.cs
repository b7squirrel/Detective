using UnityEngine;

public class CheckTime : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI timerText; // 시간을 표시할 UI Text
    private float elapsedTime = 0f;

    void Update()
    {
        // 시간 증가
        elapsedTime += Time.deltaTime;

        // 초 단위 정수로 변환
        int seconds = Mathf.FloorToInt(elapsedTime);

        // UI 텍스트에 표시
        timerText.text = seconds.ToString() + "s";
    }
}
