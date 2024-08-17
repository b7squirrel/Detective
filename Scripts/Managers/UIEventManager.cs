using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEventManager : MonoBehaviour
{
    Queue<Action> uiQueue = new Queue<Action>(); // 이벤트 큐를 생성
    bool isEventActive = false; // 현재 이벤트가 진행 중인지 여부를 저장

    // UI이벤트를 추가하는 함수
    public void EnqueueUIEvnet(Action _ui, string _eventName)
    {
        uiQueue.Enqueue(_ui);
        
        TryExecuteNextEvent();
    }

    // 다음 이벤트를 실행 시도하는 함수
    void TryExecuteNextEvent()
    {
        if (uiQueue.Count > 0 && isEventActive == false)
        {
            StartCoroutine(ExecuteUIEvent());
        }
    }

    // 이벤트를 실행하는 코루틴
    IEnumerator ExecuteUIEvent()
    {
        isEventActive = true; // 이벤트가 활성화 되었음을 표시

        Action currentEvent = uiQueue.Dequeue();

        currentEvent.Invoke();

        yield return new WaitUntil(() => isEventActive == false); // 이벤트가 종료될 때까지 대기
    }
    // 현재 활성화된 이벤트를 종료하는 함수
    public void CompleteCurrentEvent(string _eventName)
    {
        isEventActive = false;
        Debug.Log(_eventName + "종료");

        TryExecuteNextEvent();
    }
}
