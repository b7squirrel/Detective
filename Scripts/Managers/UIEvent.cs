using System;
using UnityEngine;

public class UIEvent
{
    public Action ShowUI { get; }
    public float duration; // UI가 지속되는 시간

    public UIEvent(Action showUI, float duration)
    {
        ShowUI = showUI;
        this.duration = duration;
    }
}