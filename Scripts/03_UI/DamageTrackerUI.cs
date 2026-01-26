using System.Collections.Generic;
using UnityEngine;

public class DamageTrackerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cardContainer; // DamageTrackerPanel
    [SerializeField] private DamageTrackerCard cardPrefab;
    
    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    private float updateTimer;
    
    private Dictionary<string, DamageTrackerCard> weaponCards = new Dictionary<string, DamageTrackerCard>();

    void Update()
    {
        if (GameManager.instance.IsPaused) return;
        
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            UpdateAllCards();
            updateTimer = 0f;
        }
    }

    void UpdateAllCards()
    {
        if (DamageTracker.instance == null) return;

        List<string> weaponNames = DamageTracker.instance.GetAllWeaponNames();
        
        foreach (string weaponName in weaponNames)
        {
            // 카드가 없으면 생성
            if (!weaponCards.ContainsKey(weaponName))
            {
                CreateCard(weaponName);
            }
            
            // 카드 업데이트
            UpdateCard(weaponName);
        }
    }

    void CreateCard(string weaponName)
    {
        DamageTrackerCard newCard = Instantiate(cardPrefab, cardContainer);
        newCard.InitDamageTrackerCard(weaponName);
        weaponCards[weaponName] = newCard;
    }

    void UpdateCard(string weaponName)
    {
        if (!weaponCards.ContainsKey(weaponName)) return;

        int totalDamage = DamageTracker.instance.GetTotalDamage(weaponName);
        float dps1 = DamageTracker.instance.GetDPS_1Second(weaponName);
        float dps5 = DamageTracker.instance.GetDPS_5Second(weaponName);

        weaponCards[weaponName].UpdateCard(totalDamage, dps1, dps5);
    }

    #region 디버그
    // 누적 데미지 초기화 메서드
    public void ResetAllDamageData()
    {
        if (DamageTracker.instance != null)
        {
            DamageTracker.instance.ResetAllData();

            // ✨ UI 카드들도 제거
            foreach (var card in weaponCards.Values)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            weaponCards.Clear();

            Debug.Log("모든 데미지 데이터와 UI 카드가 초기화되었습니다.");
        }
    }

    // UI 토글 메서드
    public void ToggleDamageTrackerUI()
    {
        gameObject.SetActive(gameObject.activeSelf);
    }
    #endregion
}