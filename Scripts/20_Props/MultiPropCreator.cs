using UnityEngine;

public class MultiPropCreator : MonoBehaviour
{
    [Header("보상 설정")]
    [SerializeField] GameObject propPrefab;           // 코인, 보석, 경험치 보석 등
    [SerializeField] RewardType rewardType;           // GEM, COIN, NONE
    [SerializeField] int actualAmount = 100;          // 실제 획득량
    [SerializeField] int visualAmount = 10;           // 시각적 생성 개수
    
    // ⭐ Public 메서드로 외부에서 호출
    public void Initialize(Vector3 spawnPosition)
    {
        // 위치 설정
        transform.position = spawnPosition;
        
        // 1. 데이터 저장 (COIN, GEM만)
        if (rewardType != RewardType.NONE)
        {
            SaveRewardImmediately();
        }
        
        // 2. 시각적 오브젝트 생성
        SpawnVisualProps();
        
        // 3. 자기 자신은 비활성화
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 실제 획득량을 즉시 PlayerData에 저장 (UI 업데이트 없이)
    /// </summary>
    void SaveRewardImmediately()
    {
        PlayerDataManager pdm = PlayerDataManager.Instance;
        
        if (rewardType == RewardType.COIN)
        {
            int current = pdm.GetCurrentCoinNumber();
            pdm.SetCoinNumberAsSilent(current + actualAmount);
            Logger.Log($"[MultiPropCreator] 코인 {actualAmount}개 즉시 저장 완료");
        }
        else if (rewardType == RewardType.GEM)
        {
            int current = pdm.GetCurrentCristalNumber();
            pdm.SetCristalNumberAsSilent(current + actualAmount);
            Logger.Log($"[MultiPropCreator] 크리스탈 {actualAmount}개 즉시 저장 완료");
        }
    }

    /// <summary>
    /// 시각적 오브젝트 생성
    /// </summary>
    void SpawnVisualProps()
    {
        // ⭐ NONE 타입은 비주얼 = 실제 (나누기 없음)
        if (rewardType == RewardType.NONE)
        {
            SpawnNoneTypeProps();
        }
        else
        {
            SpawnCurrencyProps();
        }
    }

    /// <summary>
    /// ⭐ NONE 타입 (경험치 보석 등): 비주얼 = 실제
    /// </summary>
    void SpawnNoneTypeProps()
    {
        // actualAmount만큼 그대로 생성
        for (int i = 0; i < actualAmount; i++)
        {
            // 랜덤 위치 계산

            // 풀에서 오브젝트 가져오기
            GameObject prop = GameManager.instance.poolManager.GetMisc(propPrefab);
            
            if (prop == null)
            {
                Logger.LogError("[MultiPropCreator] PoolManager가 null을 반환했습니다!");
                continue;
            }

            prop.transform.position = transform.position;
            prop.SetActive(true);
        }
        
        Logger.Log($"[MultiPropCreator] NONE 타입 {actualAmount}개 생성 완료 at {transform.position}");
    }

    /// <summary>
    /// ⭐ COIN/GEM 타입: 비주얼 < 실제 (나눠서 생성)
    /// </summary>
    void SpawnCurrencyProps()
    {
        // 나머지 처리: 103개를 10개로 나누면 → 10+10+10+10+10+10+10+10+10+13
        int baseValue = actualAmount / visualAmount;
        int remainder = actualAmount % visualAmount;

        for (int i = 0; i < visualAmount; i++)
        {
            // 각 오브젝트가 대표하는 값 계산
            int thisValue = baseValue;
            if (i == visualAmount - 1) 
            {
                thisValue += remainder; // 마지막에 나머지 추가
            }

            // 풀에서 오브젝트 가져오기
            GameObject prop = GameManager.instance.poolManager.GetMisc(propPrefab);
            
            if (prop == null)
            {
                Logger.LogError("[MultiPropCreator] PoolManager가 null을 반환했습니다!");
                continue;
            }

            prop.transform.position = transform.position;

            // ⭐ MoveToUI가 있다면 값 전달
            MoveToUI moveToUI = prop.GetComponent<MoveToUI>();
            if (moveToUI != null)
            {
                moveToUI.SetValuePerProp(thisValue);
            }

            prop.SetActive(true);
        }
        
        Logger.Log($"[MultiPropCreator] {rewardType} 타입 {visualAmount}개 생성 (실제: {actualAmount}) at {transform.position}");
    }
}