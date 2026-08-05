using System;

[Serializable]
public class UIEvent
{
    public Action ShowUI { get; }
    public Action ForceClose { get; } // ⭐ 추가: 부활 등 최우선 상황에서 강제로 닫을 때 호출
    public bool IsDone { get; private set; }
    public string EventName;

    public UIEvent(Action showUI, string eventName, Action forceClose = null) // ⭐ 3번째 매개변수, 기본값 null이라 기존 호출부는 수정 불필요
    {
        ShowUI = showUI;
        this.IsDone = false;
        EventName = eventName;
        ForceClose = forceClose;
    }

    public void TriggerClose()
    {
        IsDone = true;
    }
}