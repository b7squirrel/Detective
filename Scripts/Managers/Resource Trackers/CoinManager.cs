using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    int currentCoins;
    public event Action OnCoinAcquired;
    PlayerDataManager playerDataManager;

    void Start()
    {
        // 현재 코인 갯수 가져오기
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        currentCoins = playerDataManager.GetCurrentCandyNumber();

        // 현재 가지고 있는 코인의 수로 초기화
        updateCurrentCoinNumbers(0); 
    }
    
    public void updateCurrentCoinNumbers(int coinsToAdd)
    {
        currentCoins += coinsToAdd;
        OnCoinAcquired?.Invoke();
    }
    public int GetCurrentCoins() => currentCoins;
}