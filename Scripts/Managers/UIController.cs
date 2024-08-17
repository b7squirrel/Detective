using UnityEngine;

/// <summary>
/// 이벤트를 등록시키면 ui event manager에서 바로 실행을 시도
/// </summary>
public class UIController : MonoBehaviour
{
    UIEventManager _uiEventManager;
    [SerializeField] string eventName;

    public virtual void RegisterEventToQueue()
    {
        if (_uiEventManager == null)
        {
            _uiEventManager = FindObjectOfType<UIEventManager>();
        }
        _uiEventManager.EnqueueUIEvnet(() => ActivateUI(), eventName);
    }
    protected virtual void ActivateUI()
    {
        // 각 UI에서 구현
    }
    protected virtual void DeactivateUI()
    {
        _uiEventManager.CompleteCurrentEvent(eventName);
    }
}
