using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    int currentCoins;
    public event Action OnCoinAcquired;
    PlayerDataManager playerDataManager;

    void Start()
    {
        // ���� ���� ���� ��������
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        currentCoins = playerDataManager.GetCurrentCandyNumber();

        // ���� ������ �ִ� ������ ���� �ʱ�ȭ
        updateCurrentCoinNumbers(0); 
    }
    
    public void updateCurrentCoinNumbers(int coinsToAdd)
    {
        currentCoins += coinsToAdd;
        OnCoinAcquired?.Invoke();
    }
    public int GetCurrentCoins() => currentCoins;
}