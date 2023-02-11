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

            Transform muzzleEffect = Instantiate(muzzleFlash, ShootPoint.position, Quaternion.identity);
            GameObject fireBall = Instantiate(weapon);
            fireBall.transform.position = ShootPoint.position;

            FireBallProjectile fireBallProjectile = fireBall.GetComponent<FireBallProjectile>();
            fireBallProjectile.Direction = dir;
            fireBallProjectile.Damage = GetDamage();

            yield return new WaitForSeconds(.3f);
        }
    }
}
