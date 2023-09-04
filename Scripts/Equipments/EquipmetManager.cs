using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmetManager : MonoBehaviour
{
    List<Equipment> characterEquips;

    EquipmentDataManager equipmentDatamanager;
    void Awake()
    {
        equipmentDatamanager = GetComponent<EquipmentDataManager>();
    }


    void Start()
    {
        // mycardList를 돌면서 equipmant data 의 첫 column만 받아와서 담기 - 장비가 장착될 오리 카드들
        // 두 번째부터 마지막 번째까지 돌면서 장착
        // 장착 시 - Stats, Ui, Data 업데이트 
    }
}
