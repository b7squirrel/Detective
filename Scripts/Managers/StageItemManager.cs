using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageItemData
{
    public int stage;
    public UpgradeData upgrade;
}
public class StageItemManager : MonoBehaviour
{
    [SerializeField] List<StageItemData> stageItemDatas;
    int stageNum;

    public List<UpgradeData> GetUpgradePool(int maxUpgradeNum)
    {
        stageNum = FindObjectOfType<PlayerDataManager>().GetCurrentStageNumber();
        List<StageItemData> itemsPool = new List<StageItemData>(); // 하나씩 빼면서 랜덤하게 뽑을 풀
        List<UpgradeData> pickedItems = new List<UpgradeData>(); // 뽑은 아이템을 저장할 리스트
        itemsPool.AddRange(stageItemDatas);

        int itemNumbers = 0;
        while (itemNumbers < maxUpgradeNum && itemsPool.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, itemsPool.Count);
            if (itemsPool[index].stage >= stageNum) // 현재 스테이지보다 크거나 같은 스테이지의 아이템만 선택
            {
                pickedItems.Add(itemsPool[index].upgrade); // 나중에 Level에 넘겨줄 아이템 리스트
                itemNumbers++;
            }
            itemsPool.RemoveAt(index); // 뽑혔거나 조건이 충족하지 않는 아이템은 삭제
        }

        return pickedItems;
    }
}
