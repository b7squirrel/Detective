using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    private Queue<UIEvent> uiEventQueue = new Queue<UIEvent>();
    [SerializeField] bool isProcessing = false;
    public bool IsUIDone { get; set; } = false; // UI
    public bool IsBlocked { get; private set; } = false; // ⭐ 추가: 부활 패널 등이 최우선일 때 true
    [SerializeField] int maxUpgradeQueue;
    UIEvent currentProcessingEvent;
    Coroutine processQueueCoroutine; // ⭐ 추가: 강제 종료 시 코루틴을 멈추기 위해 참조 저장
    int currentUpgradeCount = 0;
    int currentEggCount = 0;

    [Header("디버그")]
    [SerializeField] bool debugMode;
    [SerializeField] GameObject debugPanel;
    [SerializeField] List<string> queueContents = new List<string>();
    [SerializeField] DebugQueueContents contents;

    void Start()
    {
        debugPanel.SetActive(debugMode);
    }

    void Update()
    {
        if (IsBlocked || isProcessing || uiEventQueue.Count <= 0) // ⭐ IsBlocked 체크 추가
            return;
        ProcessQueue();
    }

    public void EnqueueUIEvent(UIEvent uiEvent)
    {
        if (IsBlocked) return; // ⭐ 차단 중이면 새 이벤트 등록 자체를 막음

        if (uiEvent.EventName == "Upgrade")
        {
            if (currentUpgradeCount >= maxUpgradeQueue)
            {
                if (debugMode)
                {
                    Debug.Log($"현재 사이클에서 최대 업그레이드 개수에 도달했습니다. ({currentUpgradeCount}/{maxUpgradeQueue}) - 새로운 Upgrade 이벤트를 무시합니다.");
                }
                return;
            }
            currentUpgradeCount++;
        }
        else if (uiEvent.EventName == "Egg")
        {
            if (currentEggCount >= 1)
            {
                if (debugMode)
                {
                    Debug.Log($"현재 사이클에서 최대 Egg 개수에 도달했습니다. ({currentEggCount}/1) - 새로운 Egg 이벤트를 무시합니다.");
                }
                return;
            }
            currentEggCount++;
        }

        uiEventQueue.Enqueue(uiEvent);
        DIsplayQueueContents();
    }

    void ProcessQueue()
    {
        processQueueCoroutine = StartCoroutine(ProcessQueueCo()); // ⭐ 참조 저장
    }

    IEnumerator ProcessQueueCo()
    {
        isProcessing = true;
        if (debugMode) DebugQueueInProcess.Instance.SetInProcess();

        currentProcessingEvent = uiEventQueue.Dequeue();

        if (GameManager.instance.IsPlayerDead &&
            currentProcessingEvent.EventName == "Upgrade")
        {
            uiEventQueue.Clear();
            currentUpgradeCount = 0;
            currentEggCount = 0;
            isProcessing = false;
            currentProcessingEvent = null;
            yield break;
        }

        currentProcessingEvent.ShowUI?.Invoke();

        yield return new WaitUntil(() => IsUIDone);

        if (uiEventQueue.Count == 0)
        {
            currentUpgradeCount = 0;
            currentEggCount = 0;
            if (debugMode)
            {
                Debug.Log("큐가 비워져서 모든 이벤트 카운트를 리셋했습니다.");
            }
        }

        currentProcessingEvent = null;
        isProcessing = false;
        IsUIDone = false;
        if (debugMode) DebugQueueInProcess.Instance.SetDone();
        DIsplayQueueContents();
    }

    // ⭐ 추가: 부활 패널 등장 시 호출 — 신규 등록 차단 + 현재 떠 있는 팝업 강제 종료 + 큐 비우기
    public void BlockForRevival()
    {
        IsBlocked = true;

        if (processQueueCoroutine != null)
        {
            StopCoroutine(processQueueCoroutine);
            processQueueCoroutine = null;
        }

        if (currentProcessingEvent != null)
        {
            currentProcessingEvent.ForceClose?.Invoke(); // 해당 패널의 ForceClose가 있으면 실행
            currentProcessingEvent = null;
        }

        uiEventQueue.Clear();
        currentUpgradeCount = 0;
        currentEggCount = 0;
        isProcessing = false;
        IsUIDone = false;

        DIsplayQueueContents();
    }

    // ⭐ 추가: 부활 완료(게임 계속) 시 호출 — 다시 팝업을 받을 수 있게 해제
    public void UnblockAfterRevival()
    {
        IsBlocked = false;
    }

    void DIsplayQueueContents()
    {
        if (debugMode == false)
            return;

        queueContents.Clear();
        foreach (var item in uiEventQueue)
        {
            queueContents.Add(item.EventName);
        }
        DebugQueueContents.Instance.SetQueueContents(queueContents);
    }
}