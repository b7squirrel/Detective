using System;
using UnityEngine;

public class KillManager : MonoBehaviour
{
    int currentKills;
    public event Action OnKill;

    void Start()
    {
        // ų �� UI�� 0���� �ʱ�ȭ
        OnKill?.Invoke();
    }
    public void UpdateCurrentKills()
    {
        currentKills++;
        OnKill?.Invoke();
    }
    public int GetCurrentKills() => currentKills;
}