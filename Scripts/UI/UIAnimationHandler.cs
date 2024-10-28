using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimationHandler : MonoBehaviour
{
    UIEvent uiEvent;

    // Popup Event Manager�� ProcessQueue���� ����
    // ������ UIEvent�� ��� �α�
    public void Initialize(UIEvent uiEvent)
    {
        this.uiEvent = uiEvent;
    }

    // ��, ���׷��̵� �г��� ���� �� ȣ��
    public void OnAnimationComplete()
    {
        uiEvent.TriggerClose();
    }
}
