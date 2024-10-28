using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimationHandler : MonoBehaviour
{
    UIEvent uiEvent;

    // Popup Event Manager의 ProcessQueue에서 실행
    // 생성한 UIEvent를 담아 두기
    public void Initialize(UIEvent uiEvent)
    {
        this.uiEvent = uiEvent;
    }

    // 알, 업그레이드 패널이 닫힐 때 호출
    public void OnAnimationComplete()
    {
        uiEvent.TriggerClose();
    }
}
