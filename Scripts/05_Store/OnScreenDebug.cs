using UnityEngine;
using TMPro;

/// <summary>
/// 안드로이드 기기에서 로그를 화면에 표시
/// </summary>
public class OnScreenDebug : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private TextMeshProUGUI debugText;
    
    [Header("로그 타입 필터")]
    [SerializeField] private bool showLog = true;         // Debug.Log
    [SerializeField] private bool showWarning = true;     // Debug.LogWarning
    [SerializeField] private bool showError = true;       // Debug.LogError
    [SerializeField] private bool showException = true;   // Debug.LogException
    
    [Header("필터 설정")]
    [SerializeField] private bool showAllLogs = false; // false면 특정 키워드만
    [SerializeField] private string[] filterKeywords = { "TimeBoxButton", "ShopManager", ">>>",  "[!]"};

    [Header("표시 설정")]
    [SerializeField] private int maxLines = 30;
    [SerializeField] private bool showFrameInfo = true;
    
    private string logs = "";
    private int logCount = 0;
    
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        Debug.Log("[OnScreenDebug] 활성화됨");
    }
    
    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }
    
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 1단계: 로그 타입 필터링
        if (!ShouldShowLogType(type))
        {
            return;
        }

        // 필터링
        if (!showAllLogs)
        {
            bool shouldShow = false;
            foreach (string keyword in filterKeywords)
            {
                if (logString.Contains(keyword))
                {
                    shouldShow = true;
                    break;
                }
            }
            
            if (!shouldShow) return;
        }
        
        logCount++;
        
        // 타입별 색상
        string color = GetColorForLogType(type);
        
        // 로그 추가
        logs += $"\n<color={color}>[{logCount}] {logString}</color>";

        // 최대 라인 수 유지
        string[] lines = logs.Split('\n');
        if (lines.Length > maxLines)
        {
            logs = string.Join("\n", lines, lines.Length - maxLines, maxLines);
        }

        UpdateDebugText();
    }

    /// <summary>
    /// 로그 타입에 따라 표시 여부 결정
    /// </summary>
    bool ShouldShowLogType(LogType type)
    {
        switch (type)
        {
            case LogType.Log:
                return showLog;
            case LogType.Warning:
                return showWarning;
            case LogType.Error:
                return showError;
            case LogType.Exception:
                return showException;
            case LogType.Assert:
                return showError; // Assert는 Error와 동일하게 처리
            default:
                return true;
        }
    }

    void Update()
    {
        if (showFrameInfo)
        {
            UpdateDebugText();
        }
    }
    
    void UpdateDebugText()
    {
        if (debugText == null) return;
        
        string output = "";
        
        // 프레임 정보
        if (showFrameInfo)
        {
            output += $"<color=cyan>===== 디버그 정보 =====</color>\n";
            output += $"프레임: {Time.frameCount}\n";
            output += $"FPS: {(int)(1f / Time.deltaTime)}\n";
            output += $"시간: {System.DateTime.Now:HH:mm:ss}\n";
            output += CheckManagers();
            output += "\n<color=cyan>===== 로그 =====</color>";
        }
        
        output += logs;
        
        debugText.text = output;
    }
    
    string CheckManagers()
    {
        string info = "";
        
        // 주요 매니저 확인
        info += $"TimeBoxButton: {(FindObjectOfType<TimeBoxButton>() != null ? "V" : "X")}\n";
        info += $"ShopManager: {(ShopManager.Instance != null ? "V" : "X")}\n";
        info += $"TimeBasedBoxManager: {(TimeBasedBoxManager.Instance != null ? "V" : "X")}\n";
        info += $"AdsManager: {(AdsManager.Instance != null ? "V" : "XS")}\n";
        
        return info;
    }
    
    string GetColorForLogType(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                return "red";
            case LogType.Warning:
                return "yellow";
            default:
                return "white";
        }
    }
    
    /// <summary>
    /// 로그 초기화 (버튼에서 호출 가능)
    /// </summary>
    public void ClearLogs()
    {
        logs = "";
        logCount = 0;
        UpdateDebugText();
    }
}