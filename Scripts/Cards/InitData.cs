using UnityEngine;

/// <summary>
/// 데이터의 초기화 순서를 꼬이지 않게 정리
/// </summary>
public class InitData : MonoBehaviour
{
    CardDataManager cardDataManager;
    CardsDictionary cardsDictionary;

    EquipmentDataManager equipmentDataManager;

    // Start is called before the first frame update
    void Awake()
    {
        // cardDataManager = FindObjectOfType<CardDataManager>();
        // cardsDictionary = FindObjectOfType<CardsDictionary>();
        // equipmentDataManager = FindObjectOfType<EquipmentDataManager>();

        // cardDataManager.Init();
        // cardsDictionary.Init();
        // equipmentDataManager.Init();
    }
}
