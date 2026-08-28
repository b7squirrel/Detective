using System.Collections.Generic;
using UnityEngine;

public class DamageTracker : MonoBehaviour
{
    public static DamageTracker instance;

    // 무기별 데이터를 저장하는 딕셔너리 (기존: 플레이어가 가한 데미지)
    private Dictionary<string, WeaponDamageData> weaponDamageDict = new Dictionary<string, WeaponDamageData>();

    // ⭐ 추가: 플레이어가 받은 데미지 (EnemyType 문자열 기준으로 집계)
    private Dictionary<string, WeaponDamageData> incomingDamageDict = new Dictionary<string, WeaponDamageData>();

    // ⭐ 추가: 회피로 무효화된 공격 횟수
    private int dodgedHitCount = 0;

    // ⭐ 추가: 몹 이름별 처치 통계 (TTK, 평균 최대체력)
    private class EnemyKillData
    {
        public int killCount;
        public float totalTTK;
        public long totalMaxHP;
    }
    private Dictionary<string, EnemyKillData> enemyKillDict = new Dictionary<string, EnemyKillData>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region 출력 데미지 (플레이어 무기)
    // 모든 무기 이름 리스트 반환
    public List<string> GetAllWeaponNames()
    {
        return new List<string>(weaponDamageDict.Keys);
    }

    // 무기별 데미지 기록
    public void RecordDamage(string weaponName, int damage)
    {
        if (!weaponDamageDict.ContainsKey(weaponName))
        {
            weaponDamageDict[weaponName] = new WeaponDamageData();
        }
        weaponDamageDict[weaponName].RecordDamage(damage);
    }

    public int GetTotalDamage(string weaponName)
    {
        if (weaponDamageDict.ContainsKey(weaponName))
        {
            return weaponDamageDict[weaponName].GetTotalDamage();
        }
        return 0;
    }

    public float GetDPS_1Second(string weaponName)
    {
        if (weaponDamageDict.ContainsKey(weaponName))
        {
            return weaponDamageDict[weaponName].GetDPS(1f);
        }
        return 0f;
    }

    public float GetDPS_5Second(string weaponName)
    {
        if (weaponDamageDict.ContainsKey(weaponName))
        {
            return weaponDamageDict[weaponName].GetDPS(5f);
        }
        return 0f;
    }
    #endregion

    #region ⭐ 받는 데미지 (적 -> 플레이어)
    public void RecordIncomingDamage(string sourceName, int damage)
    {
        if (!incomingDamageDict.ContainsKey(sourceName))
        {
            incomingDamageDict[sourceName] = new WeaponDamageData();
        }
        incomingDamageDict[sourceName].RecordDamage(damage);
    }

    public List<string> GetAllIncomingSourceNames()
    {
        return new List<string>(incomingDamageDict.Keys);
    }

    public int GetIncomingDamage(string sourceName)
    {
        if (incomingDamageDict.ContainsKey(sourceName))
        {
            return incomingDamageDict[sourceName].GetTotalDamage();
        }
        return 0;
    }

    public float GetIncomingDPS_1Second(string sourceName)
    {
        if (incomingDamageDict.ContainsKey(sourceName))
        {
            return incomingDamageDict[sourceName].GetDPS(1f);
        }
        return 0f;
    }

    public float GetIncomingDPS_5Second(string sourceName)
    {
        if (incomingDamageDict.ContainsKey(sourceName))
        {
            return incomingDamageDict[sourceName].GetDPS(5f);
        }
        return 0f;
    }
    #endregion

    #region ⭐ 회피
    public void RecordDodged()
    {
        dodgedHitCount++;
    }

    public int GetDodgedCount()
    {
        return dodgedHitCount;
    }
    #endregion

    #region ⭐ 처치 TTK (Time-To-Kill)
    public void RecordEnemyKill(string enemyName, float ttk, int maxHP)
    {
        if (!enemyKillDict.ContainsKey(enemyName))
        {
            enemyKillDict[enemyName] = new EnemyKillData();
        }
        var data = enemyKillDict[enemyName];
        data.killCount++;
        data.totalTTK += ttk;
        data.totalMaxHP += maxHP;
    }

    public List<string> GetAllKilledEnemyNames()
    {
        return new List<string>(enemyKillDict.Keys);
    }

    // (처치수, 평균 TTK, 평균 최대체력) 반환. 기록 없으면 전부 0.
    public (int count, float avgTTK, float avgHP) GetKillStats(string enemyName)
    {
        if (!enemyKillDict.ContainsKey(enemyName)) return (0, 0f, 0f);
        var data = enemyKillDict[enemyName];
        if (data.killCount == 0) return (0, 0f, 0f);
        return (data.killCount, data.totalTTK / data.killCount, (float)data.totalMaxHP / data.killCount);
    }
    #endregion

    #region 초기화
    // 모든 데이터 초기화 (⭐ 새로 추가된 데이터도 함께 초기화)
    public void ResetAllData()
    {
        weaponDamageDict.Clear();
        incomingDamageDict.Clear();
        enemyKillDict.Clear();
        dodgedHitCount = 0;
    }

    // 특정 무기 데이터만 초기화 (기존, 출력 데미지 전용)
    public void ResetWeaponData(string weaponName)
    {
        if (weaponDamageDict.ContainsKey(weaponName))
        {
            weaponDamageDict.Remove(weaponName);
        }
    }
    #endregion
}

// 각 무기(또는 데미지 출처)의 데미지 데이터를 관리하는 클래스
public class WeaponDamageData
{
    private int totalDamage = 0;
    private List<DamageRecord> damageRecords = new List<DamageRecord>();

    public void RecordDamage(int damage)
    {
        totalDamage += damage;
        damageRecords.Add(new DamageRecord(damage, Time.time));
        CleanOldRecords(5f);
    }

    private void CleanOldRecords(float maxAge)
    {
        float currentTime = Time.time;
        damageRecords.RemoveAll(record => currentTime - record.timestamp > maxAge);
    }

    public int GetTotalDamage()
    {
        return totalDamage;
    }

    public float GetDPS(float duration)
    {
        float currentTime = Time.time;
        int damageInDuration = 0;
        foreach (var record in damageRecords)
        {
            if (currentTime - record.timestamp <= duration)
            {
                damageInDuration += record.damage;
            }
        }
        return damageInDuration / duration;
    }
}

[System.Serializable]
public class DamageRecord
{
    public int damage;
    public float timestamp;

    public DamageRecord(int damage, float timestamp)
    {
        this.damage = damage;
        this.timestamp = timestamp;
    }
}