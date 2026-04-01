using UnityEngine;

public class WeaponAnimEventBridge : MonoBehaviour
{
    WeaponBase weapon;

    public void SetWeapon(WeaponBase w)
    {
        weapon = w;
    }

    // Animation Event에서 호출
    public void OnAnimAttack()
    {
        weapon?.OnAnimEvent();
    }
}
