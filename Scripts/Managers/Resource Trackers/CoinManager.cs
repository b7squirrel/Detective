using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    int currentCoins;
    public event Action OnCoinAcquired;
    PlayerDataManager playerDataManager;

    void Start()
    {
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        currentCoins = playerDataManager.GetCurrentCandyNumber();
        Debug.Log("Current coins = " + currentCoins);
        updateCurrentCoinNumbers(0); // 현재 가지고 있는 코인의 수로 초기화
    }
    
    public void updateCurrentCoinNumbers(int coinsToAdd)
    {
        currentCoins += coinsToAdd;
        OnCoinAcquired?.Invoke();
    }
    public int GetCurrentCoins() => currentCoins;
}