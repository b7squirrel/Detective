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
            DropMultipleObjects(toDrop, dropItemProperty[itemIndex].numMultiple);
            return;
        }

        float specialRandomValue = UnityEngine.Random.Range(0f, 100f);
        bool isSpecialDrop = randomValue > specialDropRate ? false : true;
        if (dropItemProperty[itemIndex].hasSpecialItem && isSpecialDrop) // 스페셜 드롭이 가능하고, 확률로 스페셜 드롭이 뽑혔다면
        {
            // 특수 우유처럼 특수한 아이템 드롭
            if (dropItemProperty[itemIndex].SpecialDrop != null)
            {
                toDrop = dropItemProperty[itemIndex].SpecialDrop;
            }
        }

        if (toDrop == null)
        {
            Debug.LogWarning("DropOnDestroy, drop Item Prefab이 null입니다.");
            return;
        }

        if (GetComponent<Enemy>() != null)
        {
            exp = GetComponent<Enemy>().ExperienceReward;
        }

        if (toDrop.GetComponent<Collectable>() != null)
        {
            isGem = toDrop.GetComponent<Collectable>().IsGem;
        }

        // 보석이라면 확률에 따라 드롭
        if (isGem)
        {
            float randomDrop = UnityEngine.Random.Range(0f, 1f);
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
        GameManager.instance.fieldItemSpawner.SpawnMultipleObjects(_numberOfDrops, _toDrop, transform.position, exp);
    }
}
