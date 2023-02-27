using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class FireBallWeapon : WeaponBase
{
    [SerializeField] GameObject weapon;
    [SerializeField] AudioClip shoot;

    [Header("Effects")]
    [SerializeField] Transform muzzleFlash;

    protected override void Attack()
    {
        Vector2 closestEnemyPosition = FindTarget();
        if (closestEnemyPosition == Vector2.zero)
        {
            Debug.Log("No ememies");
            return;
        }

        StartCoroutine(AttackCo());
    }

    IEnumerator AttackCo()
    {
        for (int i = 0; i < weaponStats.numberOfAttacks; i++)
        {
            AnimShoot();
            SoundManager.instance.Play(shoot);
            Transform muzzleEffect =
                Instantiate(muzzleFlash, EffectPoint.position, Quaternion.identity);
            GameObject fireBall = Instantiate(weapon);
            fireBall.transform.position = transform.position;
            fireBall.transform.rotation = Quaternion.FromToRotation(Vector2.up, dir);

            ProjectileBase projectile = fireBall.GetComponent<ProjectileBase>();
            projectile.Direction = dir;
            projectile.Speed = weaponStats.projectileSpeed;

            projectile.Damage = GetDamage();
            projectile.KnockBackChance = GetKnockBackChance();
            
            projectile.KnockBackChance = Wielder.knockBackChance;

            yield return new WaitForSeconds(.3f);
        }
    }
}
