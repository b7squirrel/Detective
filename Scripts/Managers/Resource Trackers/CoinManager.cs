using System;
using UnityEngine;

/// <summary>
/// 게임 내 코인(캔디) 획득 및 관리를 담당하는 매니저 클래스
/// </summary>
public class CoinManager : MonoBehaviour
{
    // 현재 플레이어가 보유한 총 코인 수
    int currentCoins;
    
    // 이번 게임(세션)에서 획득한 코인 수
    int coinNumsPickedup;
    
    // 코인 획득 시 발생하는 이벤트 (UI 업데이트용)
    public event Action OnCoinAcquired;
    
    // 플레이어 데이터 관리자 참조
    PlayerDataManager playerDataManager;
    
    void Start()
    {
        // 저장된 데이터 매니저 가져오기
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        
        // 저장된 캔디(코인) 개수 불러오기
        currentCoins = playerDataManager.GetCurrentCoinNumber();
        
        // 현재 보유한 코인으로 UI 초기화 (증가량 0)
        updateCurrentCoinNumbers(0);
        
        // 이번 세션에서 획득한 코인 수 초기화
        coinNumsPickedup = 0;
    }
    
    //코인 획득 시 호출 - 코인 수 업데이트 및 이벤트 발생
    public void updateCurrentCoinNumbers(int coinsToAdd)
    {
        // 총 보유 코인에 추가
        currentCoins += coinsToAdd;
        
        // 이번 세션 획득 코인에 추가
        coinNumsPickedup += coinsToAdd;
        
        // UI 업데이트를 위한 이벤트 발생
        OnCoinAcquired?.Invoke();
    }
    
    // 현재 보유한 총 코인 수 반환
    public int GetCurrentCoins() => currentCoins;
    
    // 이번 게임에서 획득한 코인 수 반환
    public int GetCoinNumPickedup() => coinNumsPickedup;
}