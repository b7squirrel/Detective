using System.Collections.Generic;
using UnityEngine;

public class SyncIdleAnim : MonoBehaviour
{
    // debug용으로 직렬화
    [SerializeField] List<Transform> essentialContainers;
    [SerializeField] Transform essentialContainerParent;
    [SerializeField] Transform weaponContainer;
    [SerializeField] Transform weapon;
    bool isIdle;
    bool needToSync;
    int index;

    void Update()
    {
        if (isIdle)
        {
            weapon.position = essentialContainers[0].position;
            Debug.Log("In Sync - IDle");
        }
        else
        {
            weapon.position = weaponContainer.position;
            Debug.Log("In Sync - Walk");

        }
    }
    public void Init(Transform[] _essentials, Transform _weaponcontainer, Transform _weapon)
    {
        if(essentialContainers == null) essentialContainers = new();
        Debug.Log("INIT IN SYNC");
        essentialContainers.AddRange(_essentials);
        weaponContainer = _weaponcontainer;
        weapon = _weapon;
    }

    public void SetState(bool isIdle, float dir)
    {
        if (needToSync == false)
            return;
        this.isIdle = isIdle;
        
        if(dir > 0)
        {
            index = 0;
            essentialContainerParent.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            essentialContainerParent.eulerAngles = new Vector3(0, 180f, 0);

            index = 1;
        }
    }

    public void SetIdleSync(bool needToSync)
    {
        this.needToSync = needToSync;
    }
}
