using System.Collections.Generic;
using UnityEngine;

public class WeaponContainer : MonoBehaviour
{
    [SerializeField] Transform containerPrefab;
    [SerializeField] float moveSpeed;
    List<Transform> weaponContainers;
    Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
        weaponContainers = new List<Transform>();
    }

    private void Start()
    {

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
            if (weaponData.animatorController != null)
                container.GetComponent<Animator>().runtimeAnimatorController = weaponData.animatorController;
        }
        weaponContainers.Add(container);

        return container;
    }
}
