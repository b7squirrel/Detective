using System;
using UnityEngine;

/// <summary>
/// 게임 내 코인(캔디) 획득 및 관리를 담당하는 매니저 클래스
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

    // 코인 획득 시 호출 - CoinMultiplier 배율 적용
    public void updateCurrentCoinNumbers(int coinsToAdd)
    {
        // coinsToAdd > 0 조건으로 Start() 초기화 호출 시에는 배율 미적용
        if (coinsToAdd > 0 && FieldItemEffect.instance != null && FieldItemEffect.instance.CoinMultiplier > 1f)
        {
            int multiplied = Mathf.RoundToInt(coinsToAdd * FieldItemEffect.instance.CoinMultiplier);
            Logger.Log($"[CoinManager] 골드 {FieldItemEffect.instance.CoinMultiplier}배 적용! {coinsToAdd} → {multiplied}");
            coinsToAdd = multiplied;
        }

        currentCoins += coinsToAdd;
        coinNumsPickedup += coinsToAdd;
        OnCoinAcquired?.Invoke();
    }

    public int GetCurrentCoins() => currentCoins;
    public int GetCoinNumPickedup() => coinNumsPickedup;

    // UI만 업데이트 (MoveToUI 도착 시 호출)
    public void UpdateCoinUIOnly(int amount)
    {
        if (amount > 0 && FieldItemEffect.instance != null && FieldItemEffect.instance.CoinMultiplier > 1f)
        {
            amount = Mathf.RoundToInt(amount * FieldItemEffect.instance.CoinMultiplier);
        }

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