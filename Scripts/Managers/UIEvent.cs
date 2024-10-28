using System;
using UnityEngine;

public class UIEvent
{
    public Action ShowUI { get; }
    public float duration; // UI�� ���ӵǴ� �ð�

    public UIEvent(Action showUI, float duration)
    {
        ShowUI = showUI;
        this.duration = duration;
    }
}