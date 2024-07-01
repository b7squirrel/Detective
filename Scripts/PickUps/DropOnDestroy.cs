using System.Collections.Generic;
using UnityEngine;

public class DropOnDestroy : MonoBehaviour
{
    [SerializeField] List<GameObject> dropItemPrefab; // 맞을 때마다 떨어트리는 아이템 (chest는 그냥 이걸로 드롭)
    [SerializeField] bool noMultipleDrops; // 파괴될 때 여러개를 드롭하지 않는다면 이 아래의 드롭 변수들은 의미가 없다
    //[SerializeField] GameObject dropLastItemPrefab; // 파괴될 때 떨어트리는 아이템
    [SerializeField] int numberOfLastDrops; // 파괴될 때 떨어트리는 아이템 갯수
    [SerializeField] GameObject multipleDrops; // 여러개를 떨어트릴 때
    [SerializeField] GameObject specialDrop; // 특수 아이템 드롭
    [SerializeField] int exp;
    [SerializeField] int hp;
    [SerializeField] bool isChest; // 상자는 플레이어의 체력에 따라 우유를 떨어트려야 하므로 구별해야 함
    [SerializeField] float multiDropRate; // 무더기를 드롭할 확률

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

        if (dropItemPrefab.Count <= 0)
        {
            Debug.LogWarning("DropOnDestory, dropItemPrefab 리스트가 비어 있습니다.");
            return;
        }

        bool isGem = false;

        // 체력이 30%이하로 내려가면 무조건 힐링을 할 수 있는 아이템이 드롭되도록
        GameObject toDrop = dropItemPrefab[Random.Range(0, dropItemPrefab.Count)];

        if (isChest)
        {
            Character character = Player.instance.GetComponent<Character>();
            if ((float)character.GetCurrentHP() / (float)character.MaxHealth < .3f)
            {
                Debug.Log("체력 비율 = " + (float)character.GetCurrentHP() / (float)character.MaxHealth);
                for (int i = 0; i < dropItemPrefab.Count; i++)
                {
                    if (dropItemPrefab[i].GetComponent<HealPickUpObject>() != null)
                    {
                        toDrop = dropItemPrefab[i];
                        break;
                    }
                }
            }

            // 상자일 때는 무더기 드롭의 확률도 고려
            bool isMultiDrop = Random.Range(0f, 100f) > multiDropRate ? false : true;
            if (isMultiDrop)
            {
                if (toDrop.GetComponent<MagnetPickUPObject>() == null &&
                        toDrop.GetComponent<ClockPickUpObject>() == null &&
                        toDrop.GetComponent<InvinciblePickUpObject>() == null) // 자석은 무더기 드롭이 의미가 없어보인다.
                {
                    if(toDrop.name == "Gem" || toDrop.name == "Candy") // 동전, 보석은 무더기로 
                    {
                        multipleDrops = toDrop;
                        DropMultipleObjects();
                        return;
                    }
                    // 우유는 특수 우유
                    toDrop = specialDrop;
                    Debug.Log("Special Drop name = " + toDrop.name);
                }
            }

            if (isChestDebuggingOn) toDrop = dropItemPrefab[dropItemIndex];
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
            float randomDrop = Random.Range(0f, 1f);
            if (randomDrop > StaticValues.GemDropRate)
                return;
        }

        SpawnManager.instance.SpawnObject(transform.position, toDrop, isGem, exp);
    }

    /// <summary>
    /// 체력이 있는 오브젝트가 파괴될 때 맞을 때마다 떨어트렸던 것과 다른 아이템을 떨어트림
    /// </summary>
    public void DropLastItem()
    {
        //GameObject toDrop = dropLastItemPrefab;
        //if (toDrop == null)
        //{
        //    Debug.LogWarning("DropOnDestroy, drop Last Item Prefab이 null입니다.");
        //    return;
        //}
        //bool isGem = false;
        //if (GetComponent<Enemy>() != null)
        //{
        //    exp = GetComponent<Enemy>().ExperienceReward;
        //}
        //if (toDrop.GetComponent<Collectable>() != null)
        //{
        //    isGem = toDrop.GetComponent<Collectable>().IsGem;
        //}
        
        //SpawnManager.instance.SpawnObject(transform.position, toDrop, isGem, exp);
    }
    
    public void DropMultipleObjects()
    {
        if (GetComponent<Enemy>() != null)
        {
            exp = GetComponent<Enemy>().ExperienceReward;
        }
        GameManager.instance.fieldItemSpawner.SpawnMultipleObjects(numberOfLastDrops, multipleDrops, transform.position, exp);
        //GameObject drops = Instantiate(multipleDrops, transform.position, Quaternion.identity);
        //drops.GetComponent<MultiDrops>().Init(numberOfLastDrops, dropLastItemPrefab);
    }
    // 보스가 드롭하는 아이템들은 모두 플레이어에게 흡수되도록
    public void DropMultipleBossObjects()
    {
        GameObject toDrop = dropItemPrefab[Random.Range(0, dropItemPrefab.Count)];

        GameObject drops = Instantiate(multipleDrops, transform.position, Quaternion.identity);
        drops.GetComponent<MultiDrops>().InitBossItems(numberOfLastDrops, toDrop);
    }


}
