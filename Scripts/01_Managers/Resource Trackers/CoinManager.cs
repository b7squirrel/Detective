using System;
using UnityEngine;

/// <summary>
/// 게임 내 코인(캔디) 획득 및 관리를 담당하는 매니저 클래스
/// DoubleCoin 버프는 CoinManager 배율이 아닌 EnemyDrop의 추가 드롭으로 처리
/// </summary>
public class CoinManager : MonoBehaviour
{
    int currentCoins;
    int coinNumsPickedup;
    public event Action OnCoinAcquired;
    PlayerDataManager playerDataManager;

    void Start()
    {
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        currentCoins = playerDataManager.GetCurrentCoinNumber();
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

    public void UpdateCoinUIOnly(int amount)
    {
        currentCoins += amount;
        coinNumsPickedup += amount;
        OnCoinAcquired?.Invoke();
    }

    public void SyncWithPlayerData()
    {
        currentCoins = playerDataManager.GetCurrentCoinNumber();
        OnCoinAcquired?.Invoke();
    }
}