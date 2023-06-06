using System.Collections.Generic;
using UnityEngine;

public class WeaponContainer : MonoBehaviour
{
    [SerializeField] Transform containerPrefab;
    [SerializeField] float moveSpeed;
    List<Transform> weaponContainers; // 각각의 아이들, weaponBase 및 무기 스크립트가 붙는 오브젝트
    Player player;
    GameObject weaponContainerContainer; // 아이들을 묶어주는 부모 오브젝트

    private void Awake()
    {
        player = GetComponent<Player>();
        weaponContainers = new List<Transform>();
    }

    void Start()
    {
        if (weaponContainerContainer == null)
        {
            weaponContainerContainer = new GameObject();
            weaponContainerContainer.transform.position = Vector3.zero;
            weaponContainerContainer.name = "WeaponContainers";
        }
    }
    void FixedUpdate()
    {
        ApplyMovements();
    }

    void ApplyMovements()
    {
        if (player.IsPlayerMoving() == false)
            return;

        for (int i = weaponContainers.Count - 1; i > 0; i--)
        {
            weaponContainers[i].position =
                Vector2.Lerp(weaponContainers[i].position, weaponContainers[i - 1].position, moveSpeed * Time.deltaTime);
        }
    }

    public Transform GetContainer(WeaponData weaponData, bool isInitialWeapon)
    {
        Transform container = Instantiate(containerPrefab, transform.position, Quaternion.identity);
        if (isInitialWeapon)
        {
            container.SetParent(transform);
            container.GetComponent<SpriteRenderer>().sprite = null;
        }
        else
        {
            container.parent = weaponContainerContainer.transform;

            if (weaponData.animatorController != null)
                container.GetComponent<Animator>().runtimeAnimatorController = weaponData.animatorController;
        }
        container.gameObject.name = weaponData.Name;
        weaponContainers.Add(container);
        SetSortingOrder();

        return container;
    }

    // 플레이어가 coupleweapon을 가지고 있는지 검색
    public WeaponData GetCoupleWeaponData(string synergyWeapon)
    {
        foreach (var item in weaponContainers)
        {
            WeaponBase wb = item.GetComponentInChildren<WeaponBase>();
            if (wb.weaponData.SynergyWeapon == synergyWeapon)
            {
                return wb.weaponData;
            }
        }
        return null;
    }
    public bool IsWeaponMaxLevel(WeaponData weaponData)
    {
        foreach (var item in weaponContainers)
        {
            WeaponBase wb = item.GetComponentInChildren<WeaponBase>();
            if (wb.weaponData == weaponData)
            {
                if (wb.weaponStats.currentLevel == wb.weaponData.upgrades.Count) ;
                {
                    return true;
                }
            }
        }
        return false;
    }

    public int GetWeaponLevel(WeaponData weaponData)
    {
        foreach (var item in weaponContainers)
        {
            WeaponBase wb = item.GetComponentInChildren<WeaponBase>();
            if (wb.weaponData.Name == weaponData.Name)
            {
                return wb.weaponStats.currentLevel;
            }
        }
        return 0; // 가지고 있는 무기가 아니라 새로운 무기라면 레벨 0
    }
    public void SetSynergyWeaponActive(WeaponData weaponData)
    {
        // 이 함수가 실행되는 시점에서 해당 weapon이 null일 수가 없음.
        foreach (var item in weaponContainers)
        {
            WeaponBase wb = item.GetComponentInChildren<WeaponBase>();
            if (wb.weaponData.Name == weaponData.Name)
            {
                wb.ActivateSynergyWeapon();
                Debug.Log("시너지 웨폰 액티베이트 in weaponContainer");
            }
        }
    }
    public bool CheckSynergyWeaponActivated(UpgradeData synergyUpgrade)
    {
        if(weaponContainers.Count == 0)  return false;
        foreach (var item in weaponContainers)
        {
            WeaponBase wb = item.GetComponentInChildren<WeaponBase>();
            if (wb.weaponData.Name == synergyUpgrade.weaponData.Name)
            {
                return wb.IsSynergyWeaponActivated();
            }
        }
        
        return false;
    }

    void SetSortingOrder()
    {
        for (int i = weaponContainers.Count - 1; i > 0; i--)
        {
            weaponContainers[i].GetComponent<SpriteRenderer>().sortingOrder = -i;
        }
    }
}
