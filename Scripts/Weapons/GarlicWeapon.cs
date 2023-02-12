using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class GarlicWeapon : WeaponBase
{
    [SerializeField] GameObject shootEffect;
    Animator animEffect; // 자식으로 있는 Garlic Effect의 animator
    float effectRadius;

    protected override void Awake()
    {
        base.Awake();
        animEffect = shootEffect.GetComponent<Animator>();
    }
    protected override void Attack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, weaponStats.sizeOfArea);

        effectRadius = weaponStats.sizeOfArea;
        shootEffect.transform.localScale = new Vector3(effectRadius, effectRadius, 1);
        animEffect.SetTrigger("Shoot");

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
        // Debug.Log("Garlic");
    }


}
