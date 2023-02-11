using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class GarlicWeapon : WeaponBase
{
    protected override void Attack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, weaponStats.sizeOfArea);
        ApplyDamage(colliders);
    }

    private void ApplyDamage(Collider2D[] colliders)
    {
        foreach (var item in colliders)
        {
            Idamageable enemy = item.transform.GetComponent<Idamageable>();

            if (enemy != null)
            {
                PostMessage(weaponStats.damage, item.transform.position);
                enemy.TakeDamage(weaponStats.damage);
            }
        }
    }

    protected override void FlipWeaponTools()
    {
        Debug.Log("Garlic");
    }
}
