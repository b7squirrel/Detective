using UnityEngine;

public class WeaponDataDictionary : MonoBehaviour
{
    [SerializeField] WeaponData[] weaponDatas;
    [SerializeField] UpgradeData[] acquireDatas;

    public UpgradeData GetAcquireDataFrom(string _weaponName, int _grade)
    {
        for (int i = 0; i < acquireDatas.Length; i++)
        {
            if (_weaponName == acquireDatas[i].weaponData.Name && _grade == acquireDatas[i].weaponData.grade)
            {
                return acquireDatas[i];
            }
        }
        Debug.Log("무기 사전에 일치하는 오리가 없습니다.");
        return null;
    }
}
