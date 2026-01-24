using System.Collections.Generic;
using UnityEngine;

public class DamageTracker : MonoBehaviour
{
    public static DamageTracker instance;

    // 무기별 데이터를 저장하는 딕셔너리
    private Dictionary<string, WeaponDamageData> weaponDamageDict = new Dictionary<string, WeaponDamageData>();

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

    // 모든 무기 이름 리스트 반환
    public List<string> GetAllWeaponNames()
    {
        return new List<string>(weaponDamageDict.Keys);
    }

    // 무기별 데미지 기록
    public void RecordDamage(string weaponName, int damage)
    {
        // 해당 무기가 아직 딕셔너리에 없으면 새로 생성
        if (!weaponDamageDict.ContainsKey(weaponName))
        {
            weaponDamageDict[weaponName] = new WeaponDamageData();
        }

        // 데이터 기록
        weaponDamageDict[weaponName].RecordDamage(damage);
    }

    // 특정 무기의 총 누적 데미지
    public int GetTotalDamage(string weaponName)
    {
        if (weaponDamageDict.ContainsKey(weaponName))
        {
            return weaponDamageDict[weaponName].GetTotalDamage();
        }
        return 0;
    }

    // 특정 무기의 1초 DPS
    public float GetDPS_1Second(string weaponName)
    {
        if (weaponDamageDict.ContainsKey(weaponName))
        {
            return weaponDamageDict[weaponName].GetDPS(1f);
        }
        return 0f;
    }

    // 특정 무기의 5초 DPS
    public float GetDPS_5Second(string weaponName)
    {
        if (weaponDamageDict.ContainsKey(weaponName))
        {
            return weaponDamageDict[weaponName].GetDPS(5f);
        }
        return 0f;
    }

    // 모든 데이터 초기화
    public void ResetAllData()
    {
        weaponDamageDict.Clear();
    }

    // 특정 무기 데이터만 초기화
    public void ResetWeaponData(string weaponName)
    {
        if (weaponDamageDict.ContainsKey(weaponName))
        {
            weaponDamageDict.Remove(weaponName);
        }
    }
}

// 각 무기의 데미지 데이터를 관리하는 클래스
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