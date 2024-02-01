using System;
using UnityEngine;

public class KillManager : MonoBehaviour
{
    int currentKills;
    public event Action OnKill;

    void Start()
    {
        // 킬 수 UI를 0으로 초기화
        OnKill?.Invoke();
    }
    public void UpdateCurrentKills()
    {
        currentKills++;
        OnKill?.Invoke();
    }
    public int GetCurrentKills() => currentKills;
}