using System;
using UnityEngine;

public class KillManager : MonoBehaviour
{
    int currentKills;
    public event Action OnKill;

    public void UpdateCurrentKills()
    {
        currentKills++;
        OnKill?.Invoke();
    }
    public int GetCurrentKills() => currentKills;
}