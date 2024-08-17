using UnityEngine;

/// <summary>
/// �̺�Ʈ�� ��Ͻ�Ű�� ui event manager���� �ٷ� ������ �õ�
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
        // �� UI���� ����
    }
    protected virtual void DeactivateUI()
    {
        _uiEventManager.CompleteCurrentEvent(eventName);
    }
}
