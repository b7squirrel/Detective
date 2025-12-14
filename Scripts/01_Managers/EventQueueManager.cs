using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventQueueManager : MonoBehaviour
{
    // public static EventQueueManager Instance;
    // Queue<IEnumerator> eventQueue = new Queue<IEnumerator>();
    // bool isProcessing = false;

    // [Header("디버그")]
    // public bool processing = false;

    // void Awake()
    // {
    //     Instance = this;
    // }

    // public void EnqueueEvent(IEnumerator eventCoroutine)
    // {
    //     eventQueue.Enqueue(eventCoroutine);
    //     if (!isProcessing)
    //     {
    //         StartCoroutine(ProcessQueue());
    //     }
    //     Debug.Log("대기 중인 이벤트 수 = " + eventQueue.Count);

    // }

    // private IEnumerator ProcessQueue()
    // {
    //     isProcessing = true;

    //     while (eventQueue.Count > 0)
    //     {
    //         yield return StartCoroutine(eventQueue.Dequeue());
    //     }

    //     isProcessing = false;
    // }
}