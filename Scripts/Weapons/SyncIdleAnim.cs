using UnityEngine;

public class SyncIdleAnim : MonoBehaviour
{
    // debug용으로 직렬화
    [SerializeField] Transform essential;
    [SerializeField] Transform weaponContainer;
    [SerializeField] Transform weapon;
    bool isIdle;
    bool needToSync;

    void Update()
    {
        if (isIdle)
        {
            weapon.position = essential.position;
            Debug.Log("In Sync - IDle");
        }
        else
        {
            weapon.position = weaponContainer.position;
            Debug.Log("In Sync - Walk");

        }
    }
    public void Init(Transform _essential, Transform _weaponcontainer, Transform _weapon)
    {
        essential = _essential;
        weaponContainer = _weaponcontainer;
        weapon = _weapon;
    }

    public void SetState(bool isIdle)
    {
        if (needToSync == false)
            return;
        this.isIdle = isIdle;
    }

    public void SetIdleSync(bool needToSync)
    {
        this.needToSync = needToSync;
    }
}
