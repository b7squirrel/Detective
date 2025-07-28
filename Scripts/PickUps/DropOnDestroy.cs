using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DropItemProperty
{
    public GameObject Item;
    public GameObject SpecialDrop;
    public bool isMultipleDropable;
    public bool hasSpecialItem;
    public int numMultiple; // 몇 개를 한꺼번에 생성할 것인지.
}
public class DropOnDestroy : MonoBehaviour
{
    [SerializeField] List<DropItemProperty> dropItemProperty;
    [SerializeField] int exp;
    [SerializeField] int hp;
    [SerializeField] bool isChest; // 상자는 플레이어의 체력에 따라 우유를 떨어트려야 하므로 구별해야 함
    [SerializeField] float multiDropRate; // 무더기를 드롭할 확률
    [SerializeField] float specialDropRate; // 스페셜 드롭할 확률

    bool isQuiting;

    [Header("Debug")]
    [SerializeField] bool isChestDebuggingOn;
    [SerializeField] int dropItemIndex;

    //void OnApplicationQuit()
    //{
    //    isQuiting = true;
    //}

    /// <summary>
    /// 일반 드롭
    /// </summary>
    public void CheckDrop()
    {
        //if (isQuiting) return;

        if (dropItemProperty.Count <= 0)
        {
            Debug.LogWarning("DropOnDestory, dropItemPrefab 리스트가 비어 있습니다.");
            return;
        }

        bool isGem = false;
        int itemIndex = UnityEngine.Random.Range(0, dropItemProperty.Count);
        GameObject toDrop = dropItemProperty[itemIndex].Item;

        // 체력이 30%이하로 내려가면 무조건 힐링을 할 수 있는 아이템이 드롭되도록
        if (isChest)
        {
            Character character = Player.instance.GetComponent<Character>();
            if ((float)character.GetCurrentHP() / (float)character.MaxHealth < .3f)
            {
                for (int i = 0; i < dropItemProperty.Count; i++)
                {
                    if (dropItemProperty[i].Item.GetComponent<HealPickUpObject>() != null)
                    {
                        toDrop = dropItemProperty[i].Item;
                        itemIndex = i; // 아래에서는 itemIndex로 아이템을 처리하므로
                        break;
                    }
                }
            }
        } // 여기까지 상자만의 특성

        // 무더기 드롭의 확률도 고려
        // random Value 무더기 드롭확률이나 특수 아이템의 확률
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        bool isMultiDrop = randomValue > multiDropRate ? false : true;

        if (dropItemProperty[itemIndex].isMultipleDropable && isMultiDrop) // 멀티플 드롭이 가능한 아이템이고, 확률로 멀티드롭이 뽑혔다면
        {
            DropMultipleObjects(toDrop, dropItemProperty[itemIndex].numMultiple + UnityEngine.Random.Range(-5, 10));
            return;
        }

        float specialRandomValue = UnityEngine.Random.Range(0f, 100f);
        bool isSpecialDrop = specialRandomValue > specialDropRate ? false : true;
        if (dropItemProperty[itemIndex].hasSpecialItem && isSpecialDrop) // 스페셜 드롭이 가능하고, 확률로 스페셜 드롭이 뽑혔다면
        {
            // 특수 우유처럼 특수한 아이템 드롭
            if (dropItemProperty[itemIndex].SpecialDrop != null)
            {
                toDrop = dropItemProperty[itemIndex].SpecialDrop;
            }
        }

        if (toDrop.GetComponent<EggPickUpObject>() != null)
        {
            bool spawnable = GameManager.instance.fieldItemSpawner.isEggSpawnable();
            if (spawnable == false) return; // 알이 스폰될 것이지만 스폰되어서는 안될 조건이라면 여기서 끝 (알의 중복 생성 방지)
        }

        if (toDrop == null)
        {
            Debug.LogWarning("DropOnDestroy, drop Item Prefab이 null입니다.");
            return;
        }

        // 적이라면 보석 드롭
        if (GetComponent<Enemy>() != null)
        {
            exp = GetComponent<Enemy>().ExperienceReward;
        }
        
        // 보석인지 아닌지 판별
        if (toDrop.GetComponent<Collectable>() != null)
        {
            isGem = toDrop.GetComponent<Collectable>().IsGem;
        }

        // 보석이라면 확률에 따라 드롭
        if (isGem)
        {
            float randomDrop = UnityEngine.Random.Range(0f, 1f);
            if (isChest)
            {
                randomDrop = 0; // 상자에서 보석이 나오는 경우라면 무조건 나오도록
                exp = toDrop.GetComponent<GemPickUpObject>().ExpAmount;
            }

            if (randomDrop > StaticValues.GemDropRate)
                return;
        }

        // 디버깅
        if (isChestDebuggingOn) toDrop = dropItemProperty[dropItemIndex].Item;

        SpawnManager.instance.SpawnObject(transform.position, toDrop, isGem, exp);
    }
    
    public void DropMultipleObjects(GameObject _toDrop, int _numberOfDrops)
    {
        if (GetComponent<Enemy>() != null)
        {
            exp = GetComponent<Enemy>().ExperienceReward;
        }
        else
        {
            GemPickUpObject gemPick = _toDrop.GetComponent<GemPickUpObject>();
            if (gemPick != null)
            {
                exp = gemPick.ExpAmount;
            }
            
        }
        GameManager.instance.fieldItemSpawner.SpawnMultipleObjects(_numberOfDrops, _toDrop, transform.position, exp);
    }

    public bool isThisChest()
    {
        return isChest;
    }
}
