using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Luanch Card, Up Card, EquipCard
/// </summary>
public class SlotPool : MonoBehaviour
{
    // 프리펩을 보관할 변수
    [SerializeField] GameObject[] slots;
    
    // 풀 담당을 하는 리스트들
    List<GameObject>[] pools;

    GameObject slotFoler;

    void Awake()
    {
        InitEnemyPools();
    }

    void InitEnemyPools()
    {
        pools = new List<GameObject>[this.slots.Length];
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<GameObject>();
        }

        slotFoler = new GameObject();
        slotFoler.name = "Slots";
        slotFoler.transform.position = Vector3.zero;
        slotFoler.transform.parent = transform;
    }

    public GameObject GetSlot(int index, Transform _transform)
    {
        GameObject select = null;

        // 선택된 풀에 놀고 있는 게임 오브젝트 접근
        for (int i = 0; i < pools[index].Count; i++)
        {
            if(pools[index][i].activeSelf == false)
            {
                // 발견하면 select에 할당
                select = pools[index][i];
                select.SetActive(true);
                break;
            }
        }

        // 못 찾았으면 새롭게 생성하고 select에 할당
        if(select == false)
        {
            select = Instantiate(slots[index], _transform);
            pools[index].Add(select);
        }

        return select;
    }

    public void ReturnToPool(Transform _slotToReturn)
    {
        _slotToReturn.SetParent(slotFoler.transform);
        _slotToReturn.gameObject.SetActive(false);
    }
}