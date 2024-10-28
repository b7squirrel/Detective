using System;
using UnityEngine;

[System.Serializable]
public class UIEvent
{
    public Action ShowUI { get; }
    public bool IsDone { get; private set; }// UI�� �������� ����

    public UIEvent(Action showUI)
    {
        ShowUI = showUI;
        this.IsDone = false;
    }

    public void TriggerClose()
    {
        IsDone = true;
        Debug.Log("Event Is Done");
    }
}