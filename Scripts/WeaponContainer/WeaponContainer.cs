using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponContainer : MonoBehaviour
{
    [SerializeField] Transform containerPrefab;
    [SerializeField] float moveSpeed;
    [SerializeField] List<Transform> weaponContainers; // 각각의 아이들, weaponBase 및 무기 스크립트가 붙는 오브젝트
    Player player;
    GameObject weaponContainerGroup; // 아이들을 묶어주는 부모 오브젝트
    List<WeaponContainerAnim> weaponContainerAnims; // 아이들의 방향에 접근하기 위해

    private void Awake()
    {
        player = GetComponent<Player>();
        weaponContainers = new List<Transform>();
    }

    void Start()
    {
        if (weaponContainerGroup == null)
        {
            weaponContainerGroup = new GameObject();
            weaponContainerGroup.transform.position = Vector3.zero;
            weaponContainerGroup.name = "WeaponContainerGroup";
        }
        
    }
    void Update()
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

            weaponContainerAnims[i - 1].FacingRight = 
                (weaponContainers[i - 1].position.x - weaponContainers[i].position.x) > 0;
        }
    }

    public Transform CreateContainer(WeaponData wd, bool isInitialWeapon)
    {
        Transform container = Instantiate(containerPrefab, transform.position, Quaternion.identity);
        container.gameObject.name = wd.Name;
        weaponContainers.Add(container);

        if (isInitialWeapon)
        {
            container.SetParent(transform);
            container.GetComponent<Animator>().enabled = false;
        }
        else
        {
            container.SetParent(weaponContainerGroup.transform);

            // if (weaponData.animatorController != null)
            //     container.GetComponent<Animator>().runtimeAnimatorController = weaponData.animatorController;
            if (weaponContainerAnims == null) weaponContainerAnims = new List<WeaponContainerAnim>();
            WeaponContainerAnim wa = container.GetComponent<WeaponContainerAnim>();
            weaponContainerAnims.Add(wa);
            wa.SetEquipmentSprites(wd.Animators.InGamePlayerAnim, wd.DefaultHead, wd.DefaultChest, wd.DefaultFace, wd.DefaultHand);
        }

        SetSortingOrder();
        return container;
    }

    // 플레이어가 coupleweapon을 가지고 있는지 검색
    public WeaponData GetCoupleWeaponData(string synergyWeapon)
    {
        for (int i = 0; i < weaponContainers.Count; i++)
        {
            WeaponBase wb = weaponContainers[i].GetComponentInChildren<WeaponBase>();
            if (wb.weaponData.SynergyWeapon == synergyWeapon)
            {
                return wb.weaponData;
            }
        }
        return null;
    }
    public bool IsWeaponMaxLevel(WeaponData weaponData)
    {
        for (int i = 0; i < weaponContainers.Count; i++)
        {
            WeaponBase wb = weaponContainers[i].GetComponentInChildren<WeaponBase>();
            if (wb.weaponData == weaponData)
            {
                if (wb.weaponStats.currentLevel == wb.weaponData.upgrades.Count)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    public int GetWeaponLevel(WeaponData weaponData)
    {
        for (int i = 0; i < weaponContainers.Count; i++)
        {
            WeaponBase wb = weaponContainers[i].GetComponentInChildren<WeaponBase>();
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
        for (int i = 0; i < weaponContainers.Count; i++)
        {
            WeaponBase wb = weaponContainers[i].GetComponentInChildren<WeaponBase>();
            if (wb.weaponData.Name == weaponData.Name)
            {
                wb.ActivateSynergyWeapon();
            }
        }
    }
    public bool CheckSynergyWeaponActivated(UpgradeData synergyUpgrade)
    {
        if (weaponContainers.Count == 0) return false;
        for (int i = 0; i < weaponContainers.Count; i++)
        {
            WeaponBase wb = weaponContainers[i].GetComponentInChildren<WeaponBase>();
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
