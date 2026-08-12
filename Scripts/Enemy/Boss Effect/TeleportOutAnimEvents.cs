using System;
using UnityEngine;

public class TeleportOutAnimEvents : MonoBehaviour
{
    // 애니메이션이 완전히 끝났을 때 발생하는 이벤트
    public event Action OnAnimationFinished;

    // ⭐ 추가: 애니메이션이 "거의" 끝나갈 때(끝나기 조금 전) 발생하는 이벤트
    public event Action OnAnimationNearlyFinished;

    // 애니메이션 클립의 마지막 프레임에 Animation Event로 연결할 함수
    public void OnTeleportOutAnimFinished()
    {
        OnAnimationFinished?.Invoke();
    }

    // ⭐ 추가: 애니메이션 클립의 "끝나기 조금 전" 프레임에 Animation Event로 연결할 함수
    public void OnTeleportOutAnimNearlyFinished()
    {
        OnAnimationNearlyFinished?.Invoke();
    }
}