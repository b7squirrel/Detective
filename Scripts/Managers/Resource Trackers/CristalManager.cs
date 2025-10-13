using System;
using UnityEngine;

public class CristalManager : MonoBehaviour
{
    // 현재 플레이어가 보유한 총 코인 수
    int currentCristals;
    
    // 이번 게임(세션)에서 획득한 코인 수
    int cristalNumsPickedup;
    
    // 코인 획득 시 발생하는 이벤트 (UI 업데이트용)
    public event Action OnCristalAcquired;
    
    // 플레이어 데이터 관리자 참조
    PlayerDataManager playerDataManager;
    
    void Start()
    {
        // 저장된 데이터 매니저 가져오기
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        
        // 저장된 캔디(코인) 개수 불러오기
        currentCristals = playerDataManager.GetCurrentHighCoinNumber();
        
        // 현재 보유한 코인으로 UI 초기화 (증가량 0)
        updateCurrentCristalNumbers(0);
        
        // 이번 세션에서 획득한 코인 수 초기화
        cristalNumsPickedup = 0;
    }
    
    //코인 획득 시 호출 - 코인 수 업데이트 및 이벤트 발생
    public void updateCurrentCristalNumbers(int cristalsToAdd)
    {
        // 총 보유 코인에 추가
        currentCristals += cristalsToAdd;
        
        // 이번 세션 획득 코인에 추가
        cristalNumsPickedup += cristalsToAdd;
        
        // UI 업데이트를 위한 이벤트 발생
        OnCristalAcquired?.Invoke();
    }
    
    // 현재 보유한 총 코인 수 반환
    public int GetCurrentCristals() => currentCristals;
    
    // 이번 게임에서 획득한 코인 수 반환
    public int GetCristalNumPickedup() => cristalNumsPickedup;
}
