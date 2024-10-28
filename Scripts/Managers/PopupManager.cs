using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    private Queue<UIEvent> uiEventQueue = new Queue<UIEvent>();
    [SerializeField] bool isProcessing = false;
    public UIAnimationHandler eggAnimHandler, upgradeAnimHandler;

    void Update()
    {
        if (isProcessing || uiEventQueue.Count <= 0)
            return;
        ProcessQueue();
    }

    public void EnqueueUIEvent(UIEvent uiEvent)
    {
        uiEventQueue.Enqueue(uiEvent);
    }

    void ProcessQueue()
    {
        StartCoroutine(ProcessQueueCo());
    }

    private IEnumerator ProcessQueueCo()
    {
        isProcessing = true;

        UIEvent currentEvent = uiEventQueue.Dequeue();

        // UI 표시
        currentEvent.ShowUI?.Invoke();

        // UI가 끝날 때까지 대기
        yield return new WaitUntil(() => currentEvent.IsDone);

        isProcessing = false;
    }
}
