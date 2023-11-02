using System.Collections.Generic;
using UnityEngine;

public class SyncIdleAnim : MonoBehaviour
{
    // debug용으로 직렬화
    [SerializeField] Transform essentialContainers;
    [SerializeField] Transform faceGroup;
    [SerializeField] Transform weaponContainer;
    [SerializeField] Transform weapon;
    bool isIdle;
    bool needToSync;

    void Update()
    {
        if (needToSync)
        {
            weapon.position = essentialContainers.position;
            weapon.rotation = essentialContainers.rotation;
        }
    }
    public void Init(Transform _essentials, Transform _weaponcontainer, Transform _weapon)
    {
        essentialContainers = _essentials;
        weaponContainer = _weaponcontainer;
        weapon = _weapon;
    }

    public void SetState(bool isIdle, float dir)
    {
        // if(dir > 0)
        // {
        //     faceGroup.eulerAngles = new Vector3(0, 0, 0);
        // }
        // else
        // {
        //     faceGroup.eulerAngles = new Vector3(0, 180f, 0);
        // }

        // if (needToSync == false)
        //     return;
        // this.isIdle = isIdle;
    }

    public void SetIdleSync(bool needToSync)
    {
        this.needToSync = needToSync;
    }
}
