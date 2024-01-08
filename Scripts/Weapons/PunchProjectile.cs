using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchProjectile : MonoBehaviour
{
    PunchWeapon punchWeapon;

    void Awake()
    {
        punchWeapon = GetComponentInParent<PunchWeapon>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Idamageable enemy = collision.transform.GetComponent<Idamageable>();
        

        if (enemy != null)
        {
            // base.Attck()에서 damge와 knockback 값을 가져와서 저장했음
            punchWeapon.CastDamage(enemy, collision.transform, collision.ClosestPoint(transform.position));
        }
    }
}
