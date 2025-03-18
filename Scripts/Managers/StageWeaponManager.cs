using System.Collections.Generic;
using UnityEngine;

public class StageWeaponManager : MonoBehaviour
{
    
    [SerializeField] List<StageItemData> stageWeaopnDatas;
    int stageNum;

    public List<UpgradeData> GetUpgradePool(int maxUpgradeNum)
    {
        stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
        List<StageItemData> weaponPool = new List<StageItemData>(); // 하나씩 빼면서 랜덤하게 뽑을 풀
        List<UpgradeData> pickedWeapons = new List<UpgradeData>(); // 뽑은 아이템을 저장할 리스트
        weaponPool.AddRange(stageWeaopnDatas);

        int itemNumbers = 0;
        while (itemNumbers < maxUpgradeNum && weaponPool.Count > 0)
        {
            int index = Random.Range(0, weaponPool.Count);
            if (weaponPool[index].stage <= stageNum) // 현재 스테이지보다 낮거나 같은 스테이지의 아이템만 선택
            {
                pickedWeapons.Add(weaponPool[index].upgrade); // 나중에 Level에 넘겨줄 아이템 리스트
                itemNumbers++;
            }
            weaponPool.RemoveAt(index); // 뽑혔거나 조건이 충족하지 않는 아이템은 삭제
        }

        return pickedWeapons;
    }
}
