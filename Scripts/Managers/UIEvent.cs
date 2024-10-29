using System;
using UnityEngine;

[System.Serializable]
public class UIEvent
{
    public Action ShowUI { get; }
    public bool IsDone { get; private set; }// UI가 끝났는지 여부
    public string EventName;

    public UIEvent(Action showUI, string eventName)
    {
        ShowUI = showUI;
        this.IsDone = false;
        EventName = eventName;
    }

    public void TriggerClose()
    {
        IsDone = true;
    }
}