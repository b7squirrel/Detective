using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    int currentCoins;
    int coinNumsPickedup;
    public event Action OnCoinAcquired;
    PlayerDataManager playerDataManager;

    void Start()
    {
        // ���� ���� ���� ��������
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        currentCoins = playerDataManager.GetCurrentCandyNumber();

        // ���� ������ �ִ� ������ ���� �ʱ�ȭ
        updateCurrentCoinNumbers(0); 
        coinNumsPickedup = 0;
    }
    
    public void updateCurrentCoinNumbers(int coinsToAdd)
    {
        currentCoins += coinsToAdd;
        coinNumsPickedup += coinsToAdd;
        OnCoinAcquired?.Invoke();
    }
    public int GetCurrentCoins() => currentCoins;
    public int GetCoinNumPickedup() => coinNumsPickedup;
}