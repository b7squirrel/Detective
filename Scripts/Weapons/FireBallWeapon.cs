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
    [SerializeField] GameObject muzzleFlash;

    protected override void Attack()
    {
        List<Vector2> closestEnemyPosition = FindTarget(1);
        if (closestEnemyPosition[0] == Vector2.zero)
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
            GameObject effect = GameManager.instance.poolManager.GetMisc(muzzleFlash);
            effect.transform.position = EffectPoint.position;
            
            GameObject fireBall = GameManager.instance.poolManager.GetMisc(weapon);
            // GameObject fireBall = Instantiate(weapon, transform.position, Quaternion.identity);
            fireBall.transform.position = transform.position;
            fireBall.transform.rotation = Quaternion.FromToRotation(Vector2.up, dir);

            ProjectileBase projectile = fireBall.GetComponent<ProjectileBase>();
            projectile.Direction = dir;
            projectile.Speed = weaponStats.projectileSpeed;

            projectile.Damage = GetDamage();
            projectile.KnockBackChance = GetKnockBackChance();

            yield return new WaitForSeconds(.3f);
        }
    }
}
