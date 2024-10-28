using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    private Queue<UIEvent> uiEventQueue = new Queue<UIEvent>();
    private bool isProcessing = false;

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

        // UI ǥ��
        currentEvent.ShowUI?.Invoke();

        // UI�� ���� ������ ���
        yield return new WaitForSeconds(currentEvent.duration);

        isProcessing = false;
    }
}
