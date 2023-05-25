using System.Collections.Generic;
using UnityEngine;

public class WeaponContainer : MonoBehaviour
{
    [SerializeField] Transform containerPrefab;
    [SerializeField] float moveSpeed;
    List<Transform> weaponContainers;
    Player player;
    GameObject weaponContainerContainer;

    private void Awake()
    {
        player = GetComponent<Player>();
        weaponContainers = new List<Transform>();
    }

    private void Start()
    {
        if (weaponContainerContainer == null)
        {
            weaponContainerContainer = new GameObject();
            weaponContainerContainer.transform.position = Vector3.zero;
            weaponContainerContainer.name = "WeaponContainers";
        }
    }
    private void FixedUpdate()
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
        weaponContainers.Add(container);
        SetSortingOrder();

        return container;
    }
    public WeaponData GetCoupleWeaponData(string synergyWeapon)
    {
        foreach (var item in weaponContainers)
        {
            WeaponBase wb = item.GetComponentInChildren<WeaponBase>();
            if(wb.weaponData.SynergyWeapon == synergyWeapon)
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
            if( wb.weaponData == weaponData)
            {
                if(wb.weaponStats.currentLevel == wb.weaponData.upgrades.Count);
                {
                    return true;
                }
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
