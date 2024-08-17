using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEventManager : MonoBehaviour
{
    Queue<Action> uiQueue = new Queue<Action>(); // �̺�Ʈ ť�� ����
    bool isEventActive = false; // ���� �̺�Ʈ�� ���� ������ ���θ� ����

    // UI�̺�Ʈ�� �߰��ϴ� �Լ�
    public void EnqueueUIEvnet(Action _ui, string _eventName)
    {
        uiQueue.Enqueue(_ui);
        
        TryExecuteNextEvent();
    }

    // ���� �̺�Ʈ�� ���� �õ��ϴ� �Լ�
    void TryExecuteNextEvent()
    {
        if (uiQueue.Count > 0 && isEventActive == false)
        {
            StartCoroutine(ExecuteUIEvent());
        }
    }

    // �̺�Ʈ�� �����ϴ� �ڷ�ƾ
    IEnumerator ExecuteUIEvent()
    {
        isEventActive = true; // �̺�Ʈ�� Ȱ��ȭ �Ǿ����� ǥ��

        Action currentEvent = uiQueue.Dequeue();

        currentEvent.Invoke();

        yield return new WaitUntil(() => isEventActive == false); // �̺�Ʈ�� ����� ������ ���
    }
    // ���� Ȱ��ȭ�� �̺�Ʈ�� �����ϴ� �Լ�
    public void CompleteCurrentEvent(string _eventName)
    {
        isEventActive = false;
        Debug.Log(_eventName + "����");

        TryExecuteNextEvent();
    }
}
