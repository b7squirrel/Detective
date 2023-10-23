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

    void Update()
    {
        weapon.position = essentialContainers[0].position;
        weapon.rotation = essentialContainers[0].rotation;
        
    }
    public void Init(Transform[] _essentials, Transform _weaponcontainer, Transform _weapon)
    {
        if(essentialContainers == null) essentialContainers = new();
        essentialContainers.AddRange(_essentials);
        weaponContainer = _weaponcontainer;
        weapon = _weapon;
    }

    public void SetState(bool isIdle, float dir)
    {
        if(dir > 0)
        {
            essentialContainerParent.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            essentialContainerParent.eulerAngles = new Vector3(0, 180f, 0);
        }

        if (needToSync == false)
            return;
        this.isIdle = isIdle;
    }

    public void SetIdleSync(bool needToSync)
    {
        this.needToSync = needToSync;
    }
}
