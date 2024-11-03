using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    private Queue<UIEvent> uiEventQueue = new Queue<UIEvent>();
    [SerializeField] bool isProcessing = false;
    public bool IsUIDone { get; set; } = false; // UI �� ��������

    [Header("�����")]
    [SerializeField] bool debugMode;
    [SerializeField] List<string> queueContents = new List<string>();
    [SerializeField] DebugQueueContents contents;

    void Update()
    {
        if (isProcessing || uiEventQueue.Count <= 0)
            return;
        ProcessQueue();
    }

    public void EnqueueUIEvent(UIEvent uiEvent)
    {
        uiEventQueue.Enqueue(uiEvent);
        DIsplayQueueContents();
    }

    void ProcessQueue()
    {
        StartCoroutine(ProcessQueueCo());
    }

    private IEnumerator ProcessQueueCo()
    {
        isProcessing = true;
        if (debugMode) DebugQueueInProcess.Instance.SetInProcess();

        UIEvent currentEvent = uiEventQueue.Dequeue();

        // UI ǥ��
        currentEvent.ShowUI?.Invoke();
        Debug.Log("ť �̸� = " + currentEvent.EventName.ToString());

        // UI�� ���� ������ ���
        yield return new WaitUntil(() => IsUIDone);

        isProcessing = false;
        IsUIDone = false;
        if (debugMode) DebugQueueInProcess.Instance.SetDone();
        DIsplayQueueContents();
    }

    // �����
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
