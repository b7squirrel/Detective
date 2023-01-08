using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class WhipWeapon : WeaponBase
{
    [SerializeField] GameObject[] weapons;
    Player player;

    [SerializeField] Vector2 attackSize = new Vector2(6f, 2f);

    void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void ApplyDamage(Collider2D[] enemies)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            Idamageable hit = enemies[i].GetComponent<Idamageable>();
            if (hit != null) 
            {
                PostMessage(weaponStats.damage, enemies[i].transform.position);
                hit.TakeDamage(weaponStats.damage);
            }
        }
    }

    protected override void Attack()
    {
        base.Attack();
        if (player.FacingDir < 0)
        {
            weapons[0].SetActive(true);
            Collider2D[] enemies =
            Physics2D.OverlapBoxAll(weapons[0].transform.position, attackSize, 0f);
            ApplyDamage(enemies);
        }
        else
        {
            weapons[1].SetActive(true);
            Collider2D[] enemies =
            Physics2D.OverlapBoxAll(weapons[1].transform.position, attackSize, 0f);
            ApplyDamage(enemies);
        }
    }
}
