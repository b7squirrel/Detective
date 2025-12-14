using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    private Queue<UIEvent> uiEventQueue = new Queue<UIEvent>();
    [SerializeField] bool isProcessing = false;
    public bool IsUIDone { get; set; } = false; // UI
    [SerializeField] int maxUpgradeQueue; // 업그레이드가 최대치를 넘어서 쌓이면 그 이상은 삭제하기. 끝없이 업그레이드를 해야 하는 상황을 피하기 위해
    UIEvent currentProcessingEvent; // 현재 처리 중인 이벤트를 추적하기 위한 변수
    int currentUpgradeCount = 0; // 현재 사이클에서 처리한 업그레이드 개수
    int currentEggCount = 0; // 현재 사이클에서 처리한 Egg 개수

    [Header("디버그")]
    [SerializeField] bool debugMode;
    [SerializeField] GameObject debugPanel; // 디버그 모드일때만 활성화
    [SerializeField] List<string> queueContents = new List<string>();
    [SerializeField] DebugQueueContents contents;

    void Start()
    {
        debugPanel.SetActive(debugMode);
    }
    void Update()
    {
        if (isProcessing || uiEventQueue.Count <= 0)
            return;
        ProcessQueue();
    }

    public void EnqueueUIEvent(UIEvent uiEvent)
    {
        // "Upgrade" 이벤트의 개수를 세기
        if (uiEvent.EventName == "Upgrade")
        {
            // 현재 사이클에서 이미 5개의 업그레이드 이벤트를 처리했다면 무시
            if (currentUpgradeCount >= maxUpgradeQueue)
            {
                if (debugMode)
                {
                    Debug.Log($"현재 사이클에서 최대 업그레이드 개수에 도달했습니다. ({currentUpgradeCount}/5) - 새로운 Upgrade 이벤트를 무시합니다.");
                }
                return;
            }

            // 업그레이드 이벤트 카운트 증가
            currentUpgradeCount++;
        }
        // "Egg" 이벤트의 개수를 세기
        else if (uiEvent.EventName == "Egg")
        {
            // 현재 사이클에서 이미 2개의 Egg 이벤트를 처리했다면 무시
            if (currentEggCount >= 1)
            {
                if (debugMode)
                {
                    Debug.Log($"현재 사이클에서 최대 Egg 개수에 도달했습니다. ({currentEggCount}/2) - 새로운 Egg 이벤트를 무시합니다.");
                }
                return;
            }

            // Egg 이벤트 카운트 증가
            currentEggCount++;
        }

        uiEventQueue.Enqueue(uiEvent);
        DIsplayQueueContents();
    }

    void ProcessQueue()
    {
        StartCoroutine(ProcessQueueCo());
    }

    IEnumerator ProcessQueueCo()
    {
        isProcessing = true;
        if (debugMode) DebugQueueInProcess.Instance.SetInProcess();

        currentProcessingEvent = uiEventQueue.Dequeue(); // 현재 처리 중인 이벤트 저장

        // UI 이벤트 실행
        currentProcessingEvent.ShowUI?.Invoke();

        // UI가 완료될 때까지 대기
        yield return new WaitUntil(() => IsUIDone);

        // 큐가 완전히 비워졌고 처리 중인 이벤트도 없을 때 모든 카운트 리셋
        if (uiEventQueue.Count == 0)
        {
            currentUpgradeCount = 0; // 업그레이드 사이클 리셋
            currentEggCount = 0; // Egg 사이클 리셋
            if (debugMode)
            {
                Debug.Log("큐가 비워져서 모든 이벤트 카운트를 리셋했습니다.");
            }
        }

        currentProcessingEvent = null; // 처리 완료 후 초기화
        isProcessing = false;
        IsUIDone = false;
        if (debugMode) DebugQueueInProcess.Instance.SetDone();
        DIsplayQueueContents();
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