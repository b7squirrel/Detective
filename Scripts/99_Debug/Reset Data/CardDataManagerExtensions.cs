using UnityEngine;

/// <summary>
/// CardDataManager의 확장 메서드
/// GameDataResetter에서 사용
/// </summary>
public static class CardDataManagerExtensions
{
    /// <summary>
    /// CardDataManager의 데이터를 완전히 초기화
    /// </summary>
    public static void ResetData(this CardDataManager manager)
    {
        Logger.Log("[CardDataManager] 데이터 리셋 시작");
        
        // 런타임 데이터 초기화
        var myCardsList = manager.GetMyCardList();
        if (myCardsList != null)
        {
            myCardsList.Clear();
            Logger.Log("[CardDataManager] MyCardsList 초기화됨");
        }
        
        // CardList 초기화
        var cardList = manager.GetComponent<CardList>();
        if (cardList != null)
        {
            cardList.InitCardList();
            Logger.Log("[CardDataManager] CardList 초기화됨");
        }
        
        Logger.Log("[CardDataManager] 데이터 리셋 완료");
    }
}

/// <summary>
/// EquipmentDataManager의 확장 메서드
/// </summary>
public static class EquipmentDataManagerExtensions
{
    /// <summary>
    /// EquipmentDataManager의 데이터를 완전히 초기화
    /// </summary>
    public static void ResetData(this EquipmentDataManager manager)
    {
        Logger.Log("[EquipmentDataManager] 데이터 리셋 시작");
        
        // 런타임 데이터 초기화
        var myEquipmentsList = manager.GetMyEquipmentsList();
        if (myEquipmentsList != null)
        {
            myEquipmentsList.Clear();
            Logger.Log("[EquipmentDataManager] MyEquipmentsList 초기화됨");
        }
        
        Logger.Log("[EquipmentDataManager] 데이터 리셋 완료");
    }
}

/// <summary>
/// PlayerDataManager의 확장 메서드
/// </summary>
public static class PlayerDataManagerExtensions
{
    /// <summary>
    /// PlayerDataManager의 데이터를 기본값으로 리셋
    /// </summary>
    public static void ResetToDefault(this PlayerDataManager manager)
    {
        Logger.Log("[PlayerDataManager] 데이터 리셋 시작");
        
        // 기본값으로 설정
        manager.SetCurrentStageNumber(1);
        manager.SetCoinNumberAsSilent(100);
        manager.SetCristalNumberAsSilent(50);
        
        Logger.Log("[PlayerDataManager] 데이터 리셋 완료: Stage 1, Coin 100, Cristal 50");
    }
}