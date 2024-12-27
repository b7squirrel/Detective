using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AcquireDatas
{
    public string Name;
    public UpgradeData[] acquireDatas;
}

public class WeaponDataDictionary : MonoBehaviour
{
    [SerializeField] WeaponData[] weaponDatas;
    [SerializeField] List<UpgradeData> acquireDatas;
    [SerializeField] List<AcquireDatas> AcquireDatas;
    bool isInitDone;

    public UpgradeData GetAcquireDataFrom(string _weaponName, int _grade)
    {
        if (isInitDone == false)
        {
            acquireDatas = new();
            for (int i = 0; i < AcquireDatas.Count; i++)
            {
                acquireDatas.AddRange(AcquireDatas[i].acquireDatas);
            }
            isInitDone = true;
        }

        for (int i = 0; i < acquireDatas.Count; i++)
        {
            if (_weaponName == acquireDatas[i].weaponData.Name && _grade == acquireDatas[i].weaponData.grade)
            {
                Debug.Log($"{_weaponName} �� ������ϴ�.");
                return acquireDatas[i];
            }
        }
        Debug.Log($"ã�� ���� => {_weaponName}, ���� ������ ��ġ�ϴ� ������ �����ϴ�.");
        return null;
    }
}
