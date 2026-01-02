using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 안드로이드 기기에서 로그를 화면에 표시
/// </summary>
public class OnScreenDebug : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private ScrollRect scrollRect; // 추가
    
    [Header("스크롤 설정")]
    [SerializeField] private bool autoScrollToBottom = true; // 새 로그 시 자동 스크롤
    
    [Header("로그 타입 필터")]
    [SerializeField] private bool showLog = true;
    [SerializeField] private bool showWarning = true;
    [SerializeField] private bool showError = true;
    [SerializeField] private bool showException = true;
    
    [Header("필터 설정")]
    [SerializeField] private bool showAllLogs = false;
    [SerializeField] private string[] filterKeywords = { "TimeBoxButton", "ShopManager", ">>>", "[!]"};

    [Header("표시 설정")]
    [SerializeField] private int maxLines = 100; // 스크롤 가능하니 더 많이
    [SerializeField] private bool showFrameInfo = true;
    
    private string logs = "";
    private int logCount = 0;
    private bool needScrollUpdate = false;
    
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
        if (!ShouldShowLogType(type))
        {
            return;
        }

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
        
        string color = GetColorForLogType(type);
        
        logs += $"\n<color={color}>[{logCount}] {logString}</color>";

        string[] lines = logs.Split('\n');
        if (lines.Length > maxLines)
        {
            logs = string.Join("\n", lines, lines.Length - maxLines, maxLines);
        }

        UpdateDebugText();
        
        // 자동 스크롤 플래그 설정
        if (autoScrollToBottom)
        {
            needScrollUpdate = true;
        }
    }

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
                return showError;
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
        
        // 스크롤을 맨 아래로
        if (needScrollUpdate && scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            needScrollUpdate = false;
        }
    }
    
    void UpdateDebugText()
    {
        if (debugText == null) return;
        
        string output = "";
        
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
        
        info += $"TimeBoxButton: {(FindObjectOfType<TimeBoxButton>() != null ? "V" : "X")}\n";
        info += $"ShopManager: {(ShopManager.Instance != null ? "V" : "X")}\n";
        info += $"TimeBasedBoxManager: {(TimeBasedBoxManager.Instance != null ? "V" : "X")}\n";
        info += $"AdsManager: {(AdsManager.Instance != null ? "V" : "X")}\n";
        
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
    
    public void ClearLogs()
    {
        logs = "";
        logCount = 0;
        UpdateDebugText();
    }
    
    /// <summary>
    /// 스크롤을 맨 위로
    /// </summary>
    public void ScrollToTop()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
    
    /// <summary>
    /// 스크롤을 맨 아래로
    /// </summary>
    public void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}